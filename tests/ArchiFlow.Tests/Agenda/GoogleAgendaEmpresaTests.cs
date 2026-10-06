using System;
using System.Reflection;
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

public class GoogleAgendaEmpresaTests
{
    private readonly Mock<ICompromissoRepository> _compromissoRepoMock = new();
    private readonly Mock<IConfiguracaoAgendaRepository> _configuracaoRepoMock = new();
    private readonly Mock<IGoogleCalendarService> _googleCalendarMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserContextService> _userContextMock = new();
    private readonly Mock<IAgendaValidationService> _validationMock = new();
    private readonly Mock<IOAuthStateService> _oauthStateMock = new();

    private readonly Guid _usuarioId = Guid.NewGuid();
    private readonly Guid _escritorioId = Guid.NewGuid();
    private readonly AgendaService _service;

    public GoogleAgendaEmpresaTests()
    {
        var usuarioAdmin = new Usuario
        {
            Id = _usuarioId,
            EscritorioId = _escritorioId,
            Nome = "Sócio Administrador",
            Email = "socio@arquitetura.com",
            Role = Roles.Administrador
        };
        _userContextMock.Setup(u => u.ObterUsuarioContextoAsync())
            .ReturnsAsync((usuarioAdmin, _escritorioId));

        _service = new AgendaService(
            _compromissoRepoMock.Object,
            _configuracaoRepoMock.Object,
            _googleCalendarMock.Object,
            _unitOfWorkMock.Object,
            _userContextMock.Object,
            _validationMock.Object,
            _oauthStateMock.Object);
    }

    [Fact]
    public void ConfiguracaoAgendaEscritorioDto_NaoDeveConterCamposSecretos()
    {
        var dtoType = typeof(ConfiguracaoAgendaEscritorioDto);
        var propNames = dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        propNames.Should().NotContain(p => p.Name.Equals("ChaveGoogleServiceAccountJson", StringComparison.OrdinalIgnoreCase));
        propNames.Should().NotContain(p => p.Name.Equals("GoogleClientSecret", StringComparison.OrdinalIgnoreCase));
        propNames.Should().NotContain(p => p.Name.Equals("GoogleOAuthRefreshToken", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ConfiguracaoAgendaEmpresa_DeveArmazenarDadosDaAgendaCentralizadaDoEscritorio()
    {
        var agora = DateTime.UtcNow;

        var config = new ConfiguracaoAgendaEscritorio
        {
            Id = Guid.NewGuid(),
            EscritorioId = _escritorioId,
            EmailAgendaEmpresa = "agenda@estudioarquitetura.com.br",
            GoogleCalendarId = "c_188abcde12345@group.calendar.google.com",
            NomeAgenda = "Agenda Oficial do Estúdio",
            SincronizacaoAutomaticaAtiva = true,
            ConectadoEm = agora
        };

        config.EscritorioId.Should().Be(_escritorioId);
        config.EmailAgendaEmpresa.Should().Be("agenda@estudioarquitetura.com.br");
        config.GoogleCalendarId.Should().NotBeNullOrEmpty();
        config.SincronizacaoAutomaticaAtiva.Should().BeTrue();
    }

    [Fact]
    public async Task SalvarEObterConfiguracaoAgendaEscritorio_DevePersistirESincronizarParaTodaEquipe()
    {
        ConfiguracaoAgendaEscritorio? salvo = null;
        _configuracaoRepoMock.Setup(r => r.ObterPorEscritorioIdAsync(_escritorioId))
            .ReturnsAsync(() => salvo);

        _configuracaoRepoMock.Setup(r => r.Create(It.IsAny<ConfiguracaoAgendaEscritorio>()))
            .Callback<ConfiguracaoAgendaEscritorio>(c => salvo = c)
            .ReturnsAsync((ConfiguracaoAgendaEscritorio c) => c);

        var command = new SalvarConfiguracaoAgendaEscritorioCommand
        {
            EmailAgendaEmpresa = "contato@studio.com.br",
            GoogleCalendarId = "contato@studio.com.br",
            NomeAgenda = "Agenda Oficial Studio",
            TipoIntegracao = "ServiceAccount",
            ChaveGoogleServiceAccountJson = "{\"type\": \"service_account\"}",
            SincronizacaoAutomaticaAtiva = true
        };

        var resultado = await _service.SalvarConfiguracaoAgendaEscritorioAsync(command);

        resultado.Should().NotBeNull();
        resultado.EmailAgendaEmpresa.Should().Be("contato@studio.com.br");
        resultado.PossuiChaveServiceAccount.Should().BeTrue();
        resultado.LinkEmbedGoogleCalendar.Should().Contain("calendar.google.com/calendar/embed");

        _configuracaoRepoMock.Verify(r => r.Create(It.IsAny<ConfiguracaoAgendaEscritorio>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObterLinkCompartilhadoGoogleAgenda_QuandoNaoConfigurado_DeveRetornarVazio()
    {
        _configuracaoRepoMock.Setup(r => r.ObterPorEscritorioIdAsync(_escritorioId))
            .ReturnsAsync((ConfiguracaoAgendaEscritorio?)null);

        var link = await _service.ObterLinkCompartilhadoGoogleAgendaAsync();

        link.Should().BeEmpty();
    }

    [Fact]
    public async Task SalvarConfiguracaoAgendaEscritorio_QuandoUsuarioForArquitetoAdmin_DevePermitirSalvar()
    {
        var usuarioArquiteto = new Usuario
        {
            Id = _usuarioId,
            EscritorioId = _escritorioId,
            Nome = "Arquiteto Titular",
            Email = "arquiteto@estudio.com",
            Role = Roles.ArquitetoAdmin
        };
        _userContextMock.Setup(u => u.ObterUsuarioContextoAsync())
            .ReturnsAsync((usuarioArquiteto, _escritorioId));

        var command = new SalvarConfiguracaoAgendaEscritorioCommand
        {
            EmailAgendaEmpresa = "agenda@estudio.com.br",
            GoogleCalendarId = "agenda@estudio.com.br",
            NomeAgenda = "Agenda Corporativa ArchiFlow",
            SincronizacaoAutomaticaAtiva = true
        };

        var resultado = await _service.SalvarConfiguracaoAgendaEscritorioAsync(command);

        resultado.Should().NotBeNull();
        resultado.EmailAgendaEmpresa.Should().Be("agenda@estudio.com.br");
    }

    [Fact]
    public async Task SalvarConfiguracaoAgendaEscritorio_QuandoUsuarioForCliente_DeveLancarUnauthorizedAccessException()
    {
        var usuarioCliente = new Usuario
        {
            Id = _usuarioId,
            EscritorioId = _escritorioId,
            Nome = "Cliente Externo",
            Email = "cliente@gmail.com",
            Role = Roles.Cliente
        };
        _userContextMock.Setup(u => u.ObterUsuarioContextoAsync())
            .ReturnsAsync((usuarioCliente, _escritorioId));

        var command = new SalvarConfiguracaoAgendaEscritorioCommand
        {
            EmailAgendaEmpresa = "agenda@estudio.com.br"
        };

        var act = () => _service.SalvarConfiguracaoAgendaEscritorioAsync(command);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Clientes não possuem permissão*");
    }
}
