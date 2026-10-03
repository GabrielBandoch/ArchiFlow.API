using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ArchiFlow.Application.Agenda.Commands;
using ArchiFlow.Application.Agenda.Services;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Agenda;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Shared;
using ArchiFlow.Domain.Usuarios;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Agenda;

public class AgendaServiceTests
{
    private readonly Mock<ICompromissoRepository> _compromissoRepoMock;
    private readonly Mock<IConfiguracaoAgendaRepository> _configuracaoRepoMock;
    private readonly Mock<IProjetoRepository> _projetoRepoMock;
    private readonly Mock<IClienteRepository> _clienteRepoMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock;
    private readonly Mock<IGoogleCalendarService> _googleCalendarMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    private readonly Guid _usuarioLogadoId = Guid.NewGuid();
    private readonly Guid _escritorioId = Guid.NewGuid();
    private readonly AgendaService _service;

    public AgendaServiceTests()
    {
        _compromissoRepoMock = new Mock<ICompromissoRepository>();
        _configuracaoRepoMock = new Mock<IConfiguracaoAgendaRepository>();
        _projetoRepoMock = new Mock<IProjetoRepository>();
        _clienteRepoMock = new Mock<IClienteRepository>();
        _usuarioRepoMock = new Mock<IUsuarioRepository>();
        _googleCalendarMock = new Mock<IGoogleCalendarService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        SetupHttpContext(_usuarioLogadoId);

        var usuarioLogado = new Usuario
        {
            Id = _usuarioLogadoId,
            EscritorioId = _escritorioId,
            Nome = "Arquiteto Gestor",
            Email = "gestor@studio.com",
            Role = Roles.Administrador
        };
        _usuarioRepoMock.Setup(r => r.GetById(_usuarioLogadoId)).ReturnsAsync(usuarioLogado);

        _googleCalendarMock.Setup(g => g.GerarLinkWebAdicionarEvento(
            It.IsAny<Compromisso>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .Returns("https://calendar.google.com/test");
        _googleCalendarMock.Setup(g => g.GerarLinkGoogleMeet(It.IsAny<string>()))
            .Returns("https://meet.google.com/xyz-test");

        _service = new AgendaService(
            _compromissoRepoMock.Object,
            _configuracaoRepoMock.Object,
            _projetoRepoMock.Object,
            _clienteRepoMock.Object,
            _usuarioRepoMock.Object,
            _googleCalendarMock.Object,
            _unitOfWorkMock.Object,
            _httpContextAccessorMock.Object
        );
    }

    private void SetupHttpContext(Guid? userId)
    {
        if (userId.HasValue)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()) };
            var identity = new ClaimsIdentity(claims, "Test");
            var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
            _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(httpContext);
        }
        else
        {
            _httpContextAccessorMock.Setup(h => h.HttpContext).Returns((HttpContext?)null);
        }
    }

    [Fact]
    public async Task CriarCompromissoAsync_ComDadosValidos_DeveCriarECommitar()
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

        var result = await _service.CriarCompromissoAsync(cmd);

        result.Should().NotBeNull();
        result.Titulo.Should().Be("Medição no Terreno");
        result.Tipo.Should().Be(TiposCompromisso.MedicaoTecnica);
        result.LinkGoogleMeet.Should().Be("https://meet.google.com/xyz-test");
        result.LinkGoogleCalendarWeb.Should().Be("https://calendar.google.com/test");

        _compromissoRepoMock.Verify(r => r.Create(It.IsAny<Compromisso>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarCompromissoAsync_QuandoHorarioFimMenorOuIgualInicio_DeveLancarArgumentException()
    {
        var inicio = DateTime.UtcNow.AddDays(1);
        var fim = inicio.AddHours(-1); // Fim antes do inicio
        var cmd = new CriarCompromissoCommand("Teste", inicio, fim, null, null, null, null, null, null);

        var act = async () => await _service.CriarCompromissoAsync(cmd);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*data/hora de término deve ser posterior*");
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
        _projetoRepoMock.Setup(p => p.GetById(projetoId))
            .ReturnsAsync(new Projeto { Id = projetoId, Nome = "Casa Moderna" });
        _clienteRepoMock.Setup(c => c.GetById(clienteId))
            .ReturnsAsync(new Cliente { Id = clienteId, Nome = "Roberto Dias" });

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
    public async Task AtualizarCompromissoAsync_ComDadosValidos_DeveAtualizarECommitar()
    {
        var id = Guid.NewGuid();
        var compromisso = new Compromisso
        {
            Id = id,
            EscritorioId = _escritorioId,
            Titulo = "Titulo Antigo",
            Tipo = TiposCompromisso.Geral,
            Status = StatusCompromisso.Agendado,
            DataHoraInicio = DateTime.UtcNow.AddDays(1),
            DataHoraFim = DateTime.UtcNow.AddDays(1).AddHours(1)
        };
        _compromissoRepoMock.Setup(r => r.GetById(id)).ReturnsAsync(compromisso);

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
    public async Task ExcluirCompromissoAsync_ComIdValido_DeveRemoverECommitar()
    {
        var id = Guid.NewGuid();
        var compromisso = new Compromisso { Id = id, EscritorioId = _escritorioId };
        _compromissoRepoMock.Setup(r => r.GetById(id)).ReturnsAsync(compromisso);

        await _service.ExcluirCompromissoAsync(id);

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
    public async Task SalvarConfiguracaoAgendaEscritorioAsync_QuandoValido_DeveSalvar()
    {
        var configExistente = new ConfiguracaoAgendaEscritorio { Id = Guid.NewGuid(), EscritorioId = _escritorioId };
        _configuracaoRepoMock.Setup(r => r.ObterPorEscritorioIdAsync(_escritorioId)).ReturnsAsync(configExistente);

        var cmd = new SalvarConfiguracaoAgendaEscritorioCommand
        {
            GoogleCalendarId = "c1@group.calendar.google.com",
            EmailAgendaEmpresa = "empresa@studio.com",
            TipoIntegracao = "ServiceAccount",
            ChaveGoogleServiceAccountJson = "{}",
            SincronizacaoAutomaticaAtiva = true
        };

        var result = await _service.SalvarConfiguracaoAgendaEscritorioAsync(cmd);

        result.GoogleCalendarId.Should().Be("c1@group.calendar.google.com");
        _configuracaoRepoMock.Verify(r => r.Update(configExistente), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DesconectarGoogleOAuthAsync_DeveLimparTokens()
    {
        var config = new ConfiguracaoAgendaEscritorio
        {
            Id = Guid.NewGuid(),
            EscritorioId = _escritorioId,
            GoogleOAuthRefreshToken = "token-secret"
        };
        _configuracaoRepoMock.Setup(r => r.ObterPorEscritorioIdAsync(_escritorioId)).ReturnsAsync(config);

        await _service.DesconectarGoogleOAuthAsync();

        config.GoogleOAuthRefreshToken.Should().BeNull();
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObterUrlGoogleOAuthAsync_DeveRetornarUrl()
    {
        var config = new ConfiguracaoAgendaEscritorio
        {
            Id = Guid.NewGuid(),
            EscritorioId = _escritorioId,
            GoogleClientId = "mock-client-id"
        };
        _configuracaoRepoMock.Setup(r => r.ObterPorEscritorioIdAsync(_escritorioId)).ReturnsAsync(config);

        _googleCalendarMock.Setup(g => g.GerarUrlAutorizacaoOAuth("mock-client-id", "http://localhost:4200", _escritorioId.ToString()))
            .Returns("https://accounts.google.com/auth");

        var url = await _service.ObterUrlGoogleOAuthAsync("http://localhost:4200");

        url.Should().StartWith("https://accounts.google.com");
    }

    [Fact]
    public async Task CriarCompromissoAsync_ComDataLocal_DeveConverterParaUtc()
    {
        var inicioLocal = DateTime.SpecifyKind(DateTime.Now.AddDays(1), DateTimeKind.Local);
        var fimLocal = inicioLocal.AddHours(1);
        var cmd = new CriarCompromissoCommand("Reunião Local", inicioLocal, fimLocal, null, null, null, null, null, null);

        var result = await _service.CriarCompromissoAsync(cmd);

        result.DataHoraInicio.Kind.Should().Be(DateTimeKind.Utc);
        result.DataHoraFim.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task ConectarGoogleOAuthAsync_ComSucesso_DeveAtualizarConfiguracao()
    {
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

        var cmd = new ConectarGoogleOAuthCommand { Code = "code", RedirectUri = "http://redir" };
        var result = await _service.ConectarGoogleOAuthAsync(cmd);

        result.PossuiOAuthConectado.Should().BeTrue();
        result.GoogleOAuthEmail.Should().Be("empresa@gmail.com");
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObterLinkCompartilhadoGoogleAgendaAsync_DeveRetornarEmbedLink()
    {
        var config = new ConfiguracaoAgendaEscritorio
        {
            Id = Guid.NewGuid(),
            EscritorioId = _escritorioId,
            EmailAgendaEmpresa = "empresa@studio.com",
            GoogleCalendarId = "empresa@group.calendar.google.com"
        };
        _configuracaoRepoMock.Setup(r => r.ObterPorEscritorioIdAsync(_escritorioId)).ReturnsAsync(config);

        var link = await _service.ObterLinkCompartilhadoGoogleAgendaAsync();

        link.Should().Contain("calendar.google.com/calendar/embed");
    }

    [Fact]
    public async Task SalvarConfiguracaoAgendaEscritorioAsync_SemCalendarIdNemEmail_DeveLancarArgumentException()
    {
        var cmd = new SalvarConfiguracaoAgendaEscritorioCommand
        {
            GoogleCalendarId = "",
            EmailAgendaEmpresa = ""
        };

        var act = () => _service.SalvarConfiguracaoAgendaEscritorioAsync(cmd);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
