using System;
using System.Security.Claims;
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

public class GoogleAgendaEmpresaTests
{
    private readonly Mock<ICompromissoRepository> _compromissoRepoMock = new();
    private readonly Mock<IConfiguracaoAgendaRepository> _configuracaoRepoMock = new();
    private readonly Mock<IProjetoRepository> _projetoRepoMock = new();
    private readonly Mock<IClienteRepository> _clienteRepoMock = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock = new();
    private readonly Mock<IGoogleCalendarService> _googleCalendarMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();

    private readonly Guid _usuarioId = Guid.NewGuid();
    private readonly Guid _escritorioId = Guid.NewGuid();
    private readonly AgendaService _service;

    public GoogleAgendaEmpresaTests()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _usuarioId.ToString()),
            new Claim(ClaimTypes.Role, Roles.Administrador)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext { User = principal };
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(context);

        var usuario = new Usuario
        {
            Id = _usuarioId,
            EscritorioId = _escritorioId,
            Nome = "Sócio Administrador",
            Email = "socio@arquitetura.com",
            Role = Roles.Administrador
        };
        _usuarioRepoMock.Setup(r => r.GetById(_usuarioId)).ReturnsAsync(usuario);

        _service = new AgendaService(
            _compromissoRepoMock.Object,
            _configuracaoRepoMock.Object,
            _projetoRepoMock.Object,
            _clienteRepoMock.Object,
            _usuarioRepoMock.Object,
            _googleCalendarMock.Object,
            _unitOfWorkMock.Object,
            _httpContextAccessorMock.Object);
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
        // Arrange
        ConfiguracaoAgendaEscritorio? salvo = null;
        _configuracaoRepoMock.Setup(r => r.ObterPorEscritorioIdAsync(_escritorioId))
            .ReturnsAsync(() => salvo);

        _configuracaoRepoMock.Setup(r => r.Create(It.IsAny<ConfiguracaoAgendaEscritorio>()))
            .Callback<ConfiguracaoAgendaEscritorio>(c => salvo = c)
            .ReturnsAsync((ConfiguracaoAgendaEscritorio c) => c);

        var command = new SalvarConfiguracaoAgendaEscritorioCommand
        {
            EmailAgendaEmpresa = "agenda@estudio.com.br",
            GoogleCalendarId = "agenda@estudio.com.br",
            NomeAgenda = "Agenda Corporativa ArchiFlow",
            SincronizacaoAutomaticaAtiva = true
        };

        // Act
        var resultado = await _service.SalvarConfiguracaoAgendaEscritorioAsync(command);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EmailAgendaEmpresa.Should().Be("agenda@estudio.com.br");
        resultado.LinkEmbedGoogleCalendar.Should().Contain("calendar.google.com/calendar/embed");
        resultado.LinkEmbedGoogleCalendar.Should().Contain(Uri.EscapeDataString("agenda@estudio.com.br"));
        _unitOfWorkMock.Verify(u => u.Commit(default), Times.Once);
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
        // Arrange
        var usuarioArquiteto = new Usuario
        {
            Id = _usuarioId,
            EscritorioId = _escritorioId,
            Nome = "Arquiteto Titular",
            Email = "arquiteto@estudio.com",
            Role = Roles.ArquitetoAdmin
        };
        _usuarioRepoMock.Setup(r => r.GetById(_usuarioId)).ReturnsAsync(usuarioArquiteto);

        var command = new SalvarConfiguracaoAgendaEscritorioCommand
        {
            EmailAgendaEmpresa = "agenda@estudio.com.br",
            GoogleCalendarId = "agenda@estudio.com.br",
            NomeAgenda = "Agenda Corporativa ArchiFlow",
            SincronizacaoAutomaticaAtiva = true
        };

        // Act
        var resultado = await _service.SalvarConfiguracaoAgendaEscritorioAsync(command);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EmailAgendaEmpresa.Should().Be("agenda@estudio.com.br");
    }

    [Fact]
    public async Task SalvarConfiguracaoAgendaEscritorio_QuandoUsuarioForCliente_DeveLancarUnauthorizedAccessException()
    {
        // Arrange
        var usuarioCliente = new Usuario
        {
            Id = _usuarioId,
            EscritorioId = _escritorioId,
            Nome = "Cliente Externo",
            Email = "cliente@gmail.com",
            Role = Roles.Cliente
        };
        _usuarioRepoMock.Setup(r => r.GetById(_usuarioId)).ReturnsAsync(usuarioCliente);

        var command = new SalvarConfiguracaoAgendaEscritorioCommand
        {
            EmailAgendaEmpresa = "agenda@estudio.com.br"
        };

        // Act & Assert
        var act = () => _service.SalvarConfiguracaoAgendaEscritorioAsync(command);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Clientes não possuem permissão*");
    }
}

