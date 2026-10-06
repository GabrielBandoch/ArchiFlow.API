using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArchiFlow.Application.Agenda.Commands;
using ArchiFlow.Application.Agenda.DTOs;
using ArchiFlow.Application.Agenda.Services;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Agenda;
using ArchiFlow.Domain.Shared;
using ArchiFlow.Domain.Usuarios;
using FluentAssertions;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Agenda;

public class AgendaServiceTests
{
    private readonly Mock<ICompromissoRepository> _compromissoRepoMock = new();
    private readonly Mock<IConfiguracaoAgendaRepository> _configuracaoRepoMock = new();
    private readonly Mock<IGoogleCalendarService> _googleCalendarMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserContextService> _userContextMock = new();
    private readonly Mock<IAgendaValidationService> _validationMock = new();
    private readonly Mock<IOAuthStateService> _oauthStateMock = new();

    private readonly Guid _usuarioLogadoId = Guid.NewGuid();
    private readonly Guid _escritorioId = Guid.NewGuid();
    private readonly AgendaService _service;

    public AgendaServiceTests()
    {
        var usuarioLogado = new Usuario
        {
            Id = _usuarioLogadoId,
            EscritorioId = _escritorioId,
            Nome = "Arquiteto Gestor",
            Email = "gestor@studio.com",
            Role = Roles.Administrador
        };
        _userContextMock.Setup(u => u.ObterUsuarioContextoAsync())
            .ReturnsAsync((usuarioLogado, _escritorioId));

        _googleCalendarMock.Setup(g => g.GerarLinkWebAdicionarEvento(
            It.IsAny<Compromisso>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .Returns("https://calendar.google.com/test");

        _validationMock.Setup(v => v.ObterNomesRelacionadosAsync(
            It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>()))
            .ReturnsAsync(("Casa Moderna", "Roberto Dias", "Lead Alpha", "Arquiteto Gestor"));

        _validationMock.Setup(v => v.ObterNomesEmLoteAsync(It.IsAny<IEnumerable<Compromisso>>()))
            .ReturnsAsync(new NomesRelacionadosBatch());

        _service = new AgendaService(
            _compromissoRepoMock.Object,
            _configuracaoRepoMock.Object,
            _googleCalendarMock.Object,
            _unitOfWorkMock.Object,
            _userContextMock.Object,
            _validationMock.Object,
            _oauthStateMock.Object
        );
    }

    [Fact]
    public async Task CriarCompromissoAsync_ComDadosValidos_DevePersistirLocalmenteEConectarGoogleSeHouver()
    {
        var inicio = DateTime.UtcNow.AddDays(1);
        var fim = inicio.AddHours(2);
        var cmd = new CriarCompromissoCommand(
            "Medição no Terreno",
            inicio,
            fim,
            TiposCompromisso.MedicaoTecnica,
            "Levar trena laser",
            "Condomínio Alphaville",
            null,
            null,
            null,
            GerarGoogleMeet: true
        );

        var config = new ConfiguracaoAgendaEscritorio
        {
            Id = Guid.NewGuid(),
            EscritorioId = _escritorioId,
            TipoIntegracao = "ServiceAccount",
            ChaveGoogleServiceAccountJson = "{\"type\":\"service_account\"}",
            GoogleCalendarId = "calendar@google.com",
            SincronizacaoAutomaticaAtiva = true
        };
        _configuracaoRepoMock.Setup(c => c.ObterPorEscritorioIdAsync(_escritorioId)).ReturnsAsync(config);

        _googleCalendarMock.Setup(g => g.CriarEventoDiretoNoGoogleCalendarAsync(It.IsAny<GoogleCalendarEventRequest>(), It.IsAny<string>()))
            .ReturnsAsync(new GoogleCalendarSyncResult
            {
                Sucesso = true,
                GoogleEventId = "evt-google-123",
                LinkGoogleMeet = "https://meet.google.com/real-meet-room"
            });

        var result = await _service.CriarCompromissoAsync(cmd);

        result.Should().NotBeNull();
        result.Titulo.Should().Be("Medição no Terreno");
        result.Tipo.Should().Be(TiposCompromisso.MedicaoTecnica);
        result.GoogleEventId.Should().Be("evt-google-123");
        result.LinkGoogleMeet.Should().Be("https://meet.google.com/real-meet-room");

        _compromissoRepoMock.Verify(r => r.Create(It.IsAny<Compromisso>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CriarCompromissoAsync_SemIntegracaoGoogle_NaoDeveGerarLinkFakeDeMeet()
    {
        var inicio = DateTime.UtcNow.AddDays(1);
        var fim = inicio.AddHours(1);
        var cmd = new CriarCompromissoCommand("Reunião Simples", inicio, fim, null, null, null, null, null, null, GerarGoogleMeet: true);

        _configuracaoRepoMock.Setup(c => c.ObterPorEscritorioIdAsync(_escritorioId))
            .ReturnsAsync((ConfiguracaoAgendaEscritorio?)null);

        var result = await _service.CriarCompromissoAsync(cmd);

        result.LinkGoogleMeet.Should().BeNull();
        result.GoogleEventId.Should().BeNull();
    }

    [Fact]
    public async Task CriarCompromissoAsync_QuandoHorarioFimMenorOuIgualInicio_DeveLancarArgumentException()
    {
        var inicio = DateTime.UtcNow.AddDays(1);
        var fim = inicio.AddHours(-1);
        var cmd = new CriarCompromissoCommand("Teste", inicio, fim, null, null, null, null, null, null);

        var act = async () => await _service.CriarCompromissoAsync(cmd);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*término deve ser posterior*");
    }

    [Fact]
    public async Task CriarCompromissoAsync_ComUsuarioDeOutroEscritorio_DeveLancarUnauthorizedAccessException()
    {
        var inicio = DateTime.UtcNow.AddDays(1);
        var fim = inicio.AddHours(1);
        var usuarioOutroEscritorioId = Guid.NewGuid();

        _validationMock.Setup(v => v.ValidarEntidadesRelacionadasAsync(
            _escritorioId, usuarioOutroEscritorioId, null, null, null))
            .ThrowsAsync(new UnauthorizedAccessException("O usuário pertence a outro escritório."));

        var cmd = new CriarCompromissoCommand(
            "Reunião", inicio, fim, null, null, null, null, null, usuarioOutroEscritorioId);

        var act = async () => await _service.CriarCompromissoAsync(cmd);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*outro escritório*");
    }

    [Fact]
    public async Task ObterPorPeriodoAsync_QuandoInicioMaiorQueFim_DeveLancarArgumentException()
    {
        var act = async () => await _service.ObterPorPeriodoAsync(DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(1));

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*data inicial não pode ser superior à data final*");
    }

    [Fact]
    public async Task ObterPorPeriodoAsync_ComPeriodoValido_DeveRetornarCompromissosComNomes()
    {
        var projetoId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var inicio = DateTime.UtcNow;
        var fim = inicio.AddDays(7);

        var lista = new List<Compromisso>
        {
            new()
            {
                Id = Guid.NewGuid(),
                EscritorioId = _escritorioId,
                Titulo = "Apresentação 3D",
                ProjetoId = projetoId,
                ClienteId = clienteId,
                DataHoraInicio = inicio.AddDays(1),
                DataHoraFim = inicio.AddDays(1).AddHours(1)
            }
        };

        _compromissoRepoMock.Setup(r => r.ObterPorPeriodoAsync(_escritorioId, inicio, fim, null, null))
            .ReturnsAsync(lista);

        var batch = new NomesRelacionadosBatch();
        batch.Projetos[projetoId] = "Casa Moderna";
        batch.Clientes[clienteId] = "Roberto Dias";
        _validationMock.Setup(v => v.ObterNomesEmLoteAsync(lista)).ReturnsAsync(batch);

        var result = await _service.ObterPorPeriodoAsync(inicio, fim);

        result.Should().HaveCount(1);
        var item = result.First();
        item.Titulo.Should().Be("Apresentação 3D");
        item.NomeProjeto.Should().Be("Casa Moderna");
        item.NomeCliente.Should().Be("Roberto Dias");
    }

    [Fact]
    public async Task ObterProximosAsync_DeveRetornarListaLimitada()
    {
        var lista = new List<Compromisso>
        {
            new() { Id = Guid.NewGuid(), EscritorioId = _escritorioId, Titulo = "Evento 1", DataHoraInicio = DateTime.UtcNow, DataHoraFim = DateTime.UtcNow.AddHours(1) }
        };
        _compromissoRepoMock.Setup(r => r.ObterProximosAsync(_escritorioId, 5, null)).ReturnsAsync(lista);

        var result = await _service.ObterProximosAsync(5);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task ObterPorIdAsync_QuandoNaoExisteOuOutroEscritorio_DeveRetornarNull()
    {
        var id = Guid.NewGuid();
        _compromissoRepoMock.Setup(r => r.GetById(id)).ReturnsAsync((Compromisso?)null);

        var result = await _service.ObterPorIdAsync(id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task AtualizarCompromissoAsync_ComDadosValidos_DeveAtualizarECommitarESincronizarGoogle()
    {
        var id = Guid.NewGuid();
        var compromisso = new Compromisso
        {
            Id = id,
            EscritorioId = _escritorioId,
            Titulo = "Titulo Antigo",
            Tipo = TiposCompromisso.Geral,
            Status = StatusCompromisso.Agendado,
            GoogleEventId = "evt-google-123",
            DataHoraInicio = DateTime.UtcNow.AddDays(1),
            DataHoraFim = DateTime.UtcNow.AddDays(1).AddHours(1)
        };
        _compromissoRepoMock.Setup(r => r.GetById(id)).ReturnsAsync(compromisso);

        var config = new ConfiguracaoAgendaEscritorio
        {
            Id = Guid.NewGuid(),
            EscritorioId = _escritorioId,
            TipoIntegracao = "ServiceAccount",
            ChaveGoogleServiceAccountJson = "{\"type\":\"service_account\"}",
            GoogleCalendarId = "calendar@google.com"
        };
        _configuracaoRepoMock.Setup(c => c.ObterPorEscritorioIdAsync(_escritorioId)).ReturnsAsync(config);

        var cmd = new AtualizarCompromissoCommand(
            "Titulo Atualizado",
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(2),
            TiposCompromisso.VisitaObra,
            StatusCompromisso.Agendado,
            "Nova desc",
            "Obra 01",
            null,
            null,
            null,
            null
        );

        var result = await _service.AtualizarCompromissoAsync(id, cmd);

        result.Titulo.Should().Be("Titulo Atualizado");
        result.Tipo.Should().Be(TiposCompromisso.VisitaObra);
        _compromissoRepoMock.Verify(r => r.Update(compromisso), Times.Once);
        _googleCalendarMock.Verify(g => g.AtualizarEventoDiretoAsync(It.IsAny<GoogleCalendarEventRequest>(), It.IsAny<string>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AlterarStatusCompromissoAsync_ComStatusValido_DeveAlterarStatus()
    {
        var id = Guid.NewGuid();
        var compromisso = new Compromisso
        {
            Id = id,
            EscritorioId = _escritorioId,
            Status = StatusCompromisso.Agendado,
            DataHoraInicio = DateTime.UtcNow,
            DataHoraFim = DateTime.UtcNow.AddHours(1)
        };
        _compromissoRepoMock.Setup(r => r.GetById(id)).ReturnsAsync(compromisso);

        var cmd = new AlterarStatusCompromissoCommand(StatusCompromisso.Concluido);
        var result = await _service.AlterarStatusCompromissoAsync(id, cmd);

        result.Status.Should().Be(StatusCompromisso.Concluido);
        _compromissoRepoMock.Verify(r => r.Update(compromisso), Times.Once);
    }

    [Fact]
    public async Task AlterarStatusCompromissoAsync_ComStatusInvalido_DeveLancarArgumentException()
    {
        var id = Guid.NewGuid();
        var cmd = new AlterarStatusCompromissoCommand("StatusInexistente");

        var act = async () => await _service.AlterarStatusCompromissoAsync(id, cmd);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Status 'StatusInexistente' inválido*");
    }

    [Fact]
    public async Task ExcluirCompromissoAsync_ComGoogleEventId_DeveExcluirDoGoogleEDoBanco()
    {
        var id = Guid.NewGuid();
        var compromisso = new Compromisso
        {
            Id = id,
            EscritorioId = _escritorioId,
            GoogleEventId = "evt-google-123"
        };
        _compromissoRepoMock.Setup(r => r.GetById(id)).ReturnsAsync(compromisso);

        var config = new ConfiguracaoAgendaEscritorio
        {
            Id = Guid.NewGuid(),
            EscritorioId = _escritorioId,
            TipoIntegracao = "ServiceAccount",
            ChaveGoogleServiceAccountJson = "{\"type\":\"service_account\"}",
            GoogleCalendarId = "calendar@google.com"
        };
        _configuracaoRepoMock.Setup(c => c.ObterPorEscritorioIdAsync(_escritorioId)).ReturnsAsync(config);

        await _service.ExcluirCompromissoAsync(id);

        _googleCalendarMock.Verify(g => g.ExcluirEventoDiretoAsync("evt-google-123", "calendar@google.com", It.IsAny<string>()), Times.Once);
        _compromissoRepoMock.Verify(r => r.Delete(id), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExportarIcsAsync_DeveDelegarParaGoogleCalendarService()
    {
        var compromissos = new List<Compromisso>();
        _compromissoRepoMock.Setup(r => r.ObterPorPeriodoAsync(_escritorioId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null))
            .ReturnsAsync(compromissos);
        _googleCalendarMock.Setup(g => g.ExportarIcs(compromissos, It.IsAny<string>()))
            .Returns("BEGIN:VCALENDAR...END:VCALENDAR");

        var result = await _service.ExportarIcsAsync();

        result.Should().Contain("VCALENDAR");
    }

    [Fact]
    public async Task ObterConfiguracaoAgendaEscritorioAsync_QuandoNaoExiste_DeveRetornarNull()
    {
        _configuracaoRepoMock.Setup(r => r.ObterPorEscritorioIdAsync(_escritorioId)).ReturnsAsync((ConfiguracaoAgendaEscritorio?)null);

        var result = await _service.ObterConfiguracaoAgendaEscritorioAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task ObterUrlGoogleOAuthAsync_DeveGerarStateSeguro()
    {
        var config = new ConfiguracaoAgendaEscritorio
        {
            Id = Guid.NewGuid(),
            EscritorioId = _escritorioId,
            GoogleClientId = "mock-client-id"
        };
        _configuracaoRepoMock.Setup(r => r.ObterPorEscritorioIdAsync(_escritorioId)).ReturnsAsync(config);
        _oauthStateMock.Setup(s => s.GerarState(_usuarioLogadoId, _escritorioId))
            .Returns("crypto-random-state-999");

        _googleCalendarMock.Setup(g => g.GerarUrlAutorizacaoOAuth("mock-client-id", "http://localhost:4200", "crypto-random-state-999"))
            .Returns("https://accounts.google.com/auth?state=crypto-random-state-999");

        var url = await _service.ObterUrlGoogleOAuthAsync("http://localhost:4200");

        url.Should().Contain("state=crypto-random-state-999");
        _oauthStateMock.Verify(s => s.GerarState(_usuarioLogadoId, _escritorioId), Times.Once);
    }

    [Fact]
    public async Task ConectarGoogleOAuthAsync_ComStateInvalido_DeveLancarInvalidOperationException()
    {
        _oauthStateMock.Setup(s => s.ValidarEConsumirState("bad-state", _usuarioLogadoId, _escritorioId))
            .Returns(false);

        var cmd = new ConectarGoogleOAuthCommand { Code = "code", RedirectUri = "http://redir", State = "bad-state" };

        var act = async () => await _service.ConectarGoogleOAuthAsync(cmd);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Estado OAuth inválido*");
    }

    [Fact]
    public async Task ConectarGoogleOAuthAsync_ComStateValido_DeveAtualizarConfiguracao()
    {
        _oauthStateMock.Setup(s => s.ValidarEConsumirState("valid-state", _usuarioLogadoId, _escritorioId))
            .Returns(true);

        var config = new ConfiguracaoAgendaEscritorio
        {
            Id = Guid.NewGuid(),
            EscritorioId = _escritorioId,
            GoogleClientId = "cid",
            GoogleClientSecret = "csec"
        };
        _configuracaoRepoMock.Setup(r => r.ObterPorEscritorioIdAsync(_escritorioId)).ReturnsAsync(config);
        _googleCalendarMock.Setup(g => g.TrocarCodigoPorRefreshTokenAsync("code", "cid", "csec", "http://redir"))
            .ReturnsAsync(("new-refresh-token", "empresa@gmail.com"));

        var cmd = new ConectarGoogleOAuthCommand { Code = "code", RedirectUri = "http://redir", State = "valid-state" };
        var result = await _service.ConectarGoogleOAuthAsync(cmd);

        result.PossuiOAuthConectado.Should().BeTrue();
        result.GoogleOAuthEmail.Should().Be("empresa@gmail.com");
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConectarGoogleOAuthAsync_QuandoConfiguracaoNaoExiste_DeveCriarNovaConfiguracao()
    {
        Environment.SetEnvironmentVariable("GOOGLE_CALENDAR_CLIENT_ID", "mock-env-client-id");
        Environment.SetEnvironmentVariable("GOOGLE_CALENDAR_CLIENT_SECRET", "mock-env-client-secret");

        try
        {
            _oauthStateMock.Setup(s => s.ValidarEConsumirState("valid-state-novo", _usuarioLogadoId, _escritorioId))
                .Returns(true);

            _configuracaoRepoMock.Setup(r => r.ObterPorEscritorioIdAsync(_escritorioId)).ReturnsAsync((ConfiguracaoAgendaEscritorio?)null);
            _googleCalendarMock.Setup(g => g.TrocarCodigoPorRefreshTokenAsync("code", "mock-env-client-id", "mock-env-client-secret", "http://redir"))
                .ReturnsAsync(("new-refresh-token", "empresa-nova@gmail.com"));

            var cmd = new ConectarGoogleOAuthCommand { Code = "code", RedirectUri = "http://redir", State = "valid-state-novo" };
            var result = await _service.ConectarGoogleOAuthAsync(cmd);

            result.PossuiOAuthConectado.Should().BeTrue();
            result.EmailAgendaEmpresa.Should().Be("empresa-nova@gmail.com");
            _configuracaoRepoMock.Verify(r => r.Create(It.IsAny<ConfiguracaoAgendaEscritorio>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GOOGLE_CALENDAR_CLIENT_ID", null);
            Environment.SetEnvironmentVariable("GOOGLE_CALENDAR_CLIENT_SECRET", null);
        }
    }

    [Fact]
    public async Task DesconectarGoogleOAuthAsync_DeveLimparRefreshTokenEAtualizar()
    {
        var config = new ConfiguracaoAgendaEscritorio
        {
            Id = Guid.NewGuid(),
            EscritorioId = _escritorioId,
            GoogleOAuthRefreshToken = "rt-active",
            GoogleOAuthEmail = "user@test.com",
            TipoIntegracao = "OAuth"
        };
        _configuracaoRepoMock.Setup(r => r.ObterPorEscritorioIdAsync(_escritorioId)).ReturnsAsync(config);

        await _service.DesconectarGoogleOAuthAsync();

        config.GoogleOAuthRefreshToken.Should().BeNull();
        config.GoogleOAuthEmail.Should().BeNull();
        config.TipoIntegracao.Should().Be("Nenhum");
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObterLinkCompartilhadoGoogleAgendaAsync_ComEmailConfigurado_DeveRetornarUrlEmbed()
    {
        var config = new ConfiguracaoAgendaEscritorio
        {
            Id = Guid.NewGuid(),
            EscritorioId = _escritorioId,
            GoogleCalendarId = "calendario-oficial@group.calendar.google.com"
        };
        _configuracaoRepoMock.Setup(r => r.ObterPorEscritorioIdAsync(_escritorioId)).ReturnsAsync(config);

        var link = await _service.ObterLinkCompartilhadoGoogleAgendaAsync();

        link.Should().Contain("calendar.google.com/calendar/embed");
        link.Should().Contain(Uri.EscapeDataString("calendario-oficial@group.calendar.google.com"));
    }

    [Fact]
    public async Task SalvarConfiguracaoAgendaEscritorioAsync_QuandoConfiguracaoExiste_DeveAtualizarCampos()
    {
        var config = new ConfiguracaoAgendaEscritorio
        {
            Id = Guid.NewGuid(),
            EscritorioId = _escritorioId,
            NomeAgenda = "Agenda Antiga"
        };
        _configuracaoRepoMock.Setup(r => r.ObterPorEscritorioIdAsync(_escritorioId)).ReturnsAsync(config);

        var cmd = new SalvarConfiguracaoAgendaEscritorioCommand
        {
            EmailAgendaEmpresa = "contato@empresa.com",
            GoogleCalendarId = "cal-id-123",
            ChaveGoogleServiceAccountJson = "{\"json\":true}",
            GoogleClientId = "cid-123",
            GoogleClientSecret = "csec-123",
            TipoIntegracao = "ServiceAccount",
            NomeAgenda = "Nova Agenda Oficial",
            SincronizacaoAutomaticaAtiva = true
        };

        var result = await _service.SalvarConfiguracaoAgendaEscritorioAsync(cmd);

        result.NomeAgenda.Should().Be("Nova Agenda Oficial");
        result.EmailAgendaEmpresa.Should().Be("contato@empresa.com");
        result.PossuiChaveServiceAccount.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SincronizacaoOAuth_NaCriacaoAtualizacaoExclusao_DeveChamarMetodosOAuth()
    {
        var config = new ConfiguracaoAgendaEscritorio
        {
            Id = Guid.NewGuid(),
            EscritorioId = _escritorioId,
            GoogleOAuthRefreshToken = "rt-active",
            GoogleClientId = "cid",
            GoogleClientSecret = "csec",
            TipoIntegracao = "OAuth",
            SincronizacaoAutomaticaAtiva = true
        };
        _configuracaoRepoMock.Setup(r => r.ObterPorEscritorioIdAsync(_escritorioId)).ReturnsAsync(config);

        _googleCalendarMock.Setup(g => g.CriarEventoViaOAuthAsync(
            It.IsAny<GoogleCalendarEventRequest>(), "rt-active", "cid", "csec"))
            .ReturnsAsync(new GoogleCalendarSyncResult { Sucesso = true, GoogleEventId = "evt-oauth-123" });

        var inicio = DateTime.UtcNow.AddDays(1);
        var fim = inicio.AddHours(1);
        var cmdCriar = new CriarCompromissoCommand(
            "Reuniao via OAuth",
            inicio,
            fim,
            TiposCompromisso.ReuniaoCliente,
            "Descricao",
            "Local",
            null,
            null,
            null,
            GerarGoogleMeet: true
        );

        var criado = await _service.CriarCompromissoAsync(cmdCriar);
        criado.GoogleEventId.Should().Be("evt-oauth-123");

        var compromissoExistente = new Compromisso
        {
            Id = criado.Id,
            EscritorioId = _escritorioId,
            UsuarioId = _usuarioLogadoId,
            GoogleEventId = "evt-oauth-123",
            Titulo = "Reuniao via OAuth",
            DataHoraInicio = inicio,
            DataHoraFim = fim
        };
        _compromissoRepoMock.Setup(r => r.GetById(criado.Id)).ReturnsAsync(compromissoExistente);

        var cmdAtualizar = new AtualizarCompromissoCommand(
            "Reuniao Atualizada via OAuth",
            inicio.AddDays(1),
            fim.AddDays(1),
            TiposCompromisso.ReuniaoCliente,
            StatusCompromisso.Agendado,
            "Nova desc",
            "Novo local",
            null,
            null,
            null,
            null,
            null
        );

        await _service.AtualizarCompromissoAsync(criado.Id, cmdAtualizar);
        _googleCalendarMock.Verify(g => g.AtualizarEventoViaOAuthAsync(It.IsAny<GoogleCalendarEventRequest>(), "rt-active", "cid", "csec"), Times.Once);

        await _service.ExcluirCompromissoAsync(criado.Id);
        _googleCalendarMock.Verify(g => g.ExcluirEventoViaOAuthAsync("evt-oauth-123", "primary", "rt-active", "cid", "csec"), Times.Once);
    }
}
