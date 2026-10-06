using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Agenda.Commands;
using ArchiFlow.Application.Agenda.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Agenda;

public class AgendaControllerTests
{
    private readonly Mock<IAgendaFacade> _facadeMock;
    private readonly AgendaController _controller;

    public AgendaControllerTests()
    {
        _facadeMock = new Mock<IAgendaFacade>();
        _controller = new AgendaController(_facadeMock.Object);
    }

    [Fact]
    public async Task ObterPorPeriodo_DeveRetornarOkComCompromissos()
    {
        var lista = new List<CompromissoDto>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), null, null, null, null, null, null, "Visita", null, "VisitaObra", "Agendado", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null, null, null, "http", DateTime.UtcNow, null)
        };
        _facadeMock.Setup(f => f.ObterPorPeriodoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null))
            .ReturnsAsync(lista);

        var result = await _controller.ObterPorPeriodo(null, null, null, null);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(lista);
    }

    [Fact]
    public async Task ObterProximos_DeveRetornarOkComLista()
    {
        var lista = new List<CompromissoDto>();
        _facadeMock.Setup(f => f.ObterProximosAsync(10, null)).ReturnsAsync(lista);

        var result = await _controller.ObterProximos(10, null);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ObterPorId_QuandoEncontrado_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var dto = new CompromissoDto(id, Guid.NewGuid(), null, null, null, null, null, null, "Reunião", null, "ReuniaoCliente", "Agendado", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null, null, null, "http", DateTime.UtcNow, null);
        _facadeMock.Setup(f => f.ObterPorIdAsync(id)).ReturnsAsync(dto);

        var result = await _controller.ObterPorId(id);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ObterPorId_QuandoNaoEncontrado_DeveRetornarNotFound()
    {
        var id = Guid.NewGuid();
        _facadeMock.Setup(f => f.ObterPorIdAsync(id)).ReturnsAsync((CompromissoDto?)null);

        var result = await _controller.ObterPorId(id);

        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Criar_ComComandoValido_DeveRetornarCreatedAtAction()
    {
        var id = Guid.NewGuid();
        var dto = new CompromissoDto(id, Guid.NewGuid(), null, null, null, null, null, null, "Reunião", null, "ReuniaoCliente", "Agendado", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null, null, null, "http", DateTime.UtcNow, null);
        var cmd = new CriarCompromissoCommand("Reunião", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null, null, null, null, null, null);
        _facadeMock.Setup(f => f.CriarCompromissoAsync(cmd)).ReturnsAsync(dto);

        var result = await _controller.Criar(cmd);

        var createdResult = result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult!.StatusCode.Should().Be(201);
        createdResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Atualizar_ComComandoValido_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var dto = new CompromissoDto(id, Guid.NewGuid(), null, null, null, null, null, null, "Reunião Atualizada", null, "ReuniaoCliente", "Agendado", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null, null, null, "http", DateTime.UtcNow, null);
        var cmd = new AtualizarCompromissoCommand("Reunião Atualizada", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null, null, null, null, null, null, null, null);
        _facadeMock.Setup(f => f.AtualizarCompromissoAsync(id, cmd)).ReturnsAsync(dto);

        var result = await _controller.Atualizar(id, cmd);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task AlterarStatus_ComComandoValido_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var dto = new CompromissoDto(id, Guid.NewGuid(), null, null, null, null, null, null, "Reunião", null, "ReuniaoCliente", "Concluido", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null, null, null, "http", DateTime.UtcNow, null);
        var cmd = new AlterarStatusCompromissoCommand("Concluido");
        _facadeMock.Setup(f => f.AlterarStatusCompromissoAsync(id, cmd)).ReturnsAsync(dto);

        var result = await _controller.AlterarStatus(id, cmd);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Excluir_ComIdValido_DeveRetornarNoContent()
    {
        var id = Guid.NewGuid();
        _facadeMock.Setup(f => f.ExcluirCompromissoAsync(id)).Returns(Task.CompletedTask);

        var result = await _controller.Excluir(id);

        var noContentResult = result as NoContentResult;
        noContentResult.Should().NotBeNull();
        noContentResult!.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task ExportarIcs_DeveRetornarFileResult()
    {
        _facadeMock.Setup(f => f.ExportarIcsAsync(null, null)).ReturnsAsync("BEGIN:VCALENDAR...END:VCALENDAR");

        var result = await _controller.ExportarIcs(null, null);

        var fileResult = result as FileContentResult;
        fileResult.Should().NotBeNull();
        fileResult!.ContentType.Should().Be("text/calendar");
        fileResult.FileDownloadName.Should().Be("archiflow-agenda.ics");
    }

    [Fact]
    public async Task ObterConfiguracao_DeveRetornarOkComDto()
    {
        var dto = new ConfiguracaoAgendaEscritorioDto
        {
            Id = Guid.NewGuid(),
            EmailAgendaEmpresa = "contato@arquiteto.com",
            PossuiChaveServiceAccount = true,
            GoogleCalendarId = "primary",
            PossuiOAuthConectado = true,
            GoogleOAuthEmail = "oauth@archiflow.com"
        };
        _facadeMock.Setup(f => f.ObterConfiguracaoAgendaEscritorioAsync()).ReturnsAsync(dto);

        var result = await _controller.ObterConfiguracao();

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task SalvarConfiguracao_DeveRetornarOkComDto()
    {
        var cmd = new SalvarConfiguracaoAgendaEscritorioCommand
        {
            EmailAgendaEmpresa = "contato@arquiteto.com",
            GoogleCalendarId = "primary",
            SincronizacaoAutomaticaAtiva = true
        };
        var dto = new ConfiguracaoAgendaEscritorioDto
        {
            Id = Guid.NewGuid(),
            EmailAgendaEmpresa = "contato@arquiteto.com",
            PossuiChaveServiceAccount = true,
            GoogleCalendarId = "primary"
        };
        _facadeMock.Setup(f => f.SalvarConfiguracaoAgendaEscritorioAsync(cmd)).ReturnsAsync(dto);

        var result = await _controller.SalvarConfiguracao(cmd);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task ObterLinkCompartilhado_DeveRetornarOkComLink()
    {
        _facadeMock.Setup(f => f.ObterLinkCompartilhadoGoogleAgendaAsync()).ReturnsAsync("https://calendar.google.com/embed?src=primary");

        var result = await _controller.ObterLinkCompartilhado();

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ObterUrlOAuth_ComRedirectUriVazio_DeveRetornarBadRequest()
    {
        var result = await _controller.ObterUrlOAuth("");

        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task ObterUrlOAuth_ComRedirectUriValido_DeveRetornarOkComUrlEState()
    {
        var fakeUrl = "https://accounts.google.com/o/oauth2/v2/auth?client_id=123&state=secure-state-123";
        _facadeMock.Setup(f => f.ObterUrlGoogleOAuthAsync("https://app.archiflow.com/callback")).ReturnsAsync(fakeUrl);

        var result = await _controller.ObterUrlOAuth("https://app.archiflow.com/callback");

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ConectarOAuth_DeveRetornarOk()
    {
        var cmd = new ConectarGoogleOAuthCommand
        {
            Code = "code-123",
            RedirectUri = "https://app.archiflow.com/callback",
            State = "state-123"
        };
        var dto = new ConfiguracaoAgendaEscritorioDto
        {
            Id = Guid.NewGuid(),
            EmailAgendaEmpresa = "contato@arquiteto.com",
            PossuiOAuthConectado = true,
            GoogleOAuthEmail = "oauth@archiflow.com"
        };
        _facadeMock.Setup(f => f.ConectarGoogleOAuthAsync(cmd)).ReturnsAsync(dto);

        var result = await _controller.ConectarOAuth(cmd);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task DesconectarOAuth_DeveRetornarNoContent()
    {
        _facadeMock.Setup(f => f.DesconectarGoogleOAuthAsync()).Returns(Task.CompletedTask);

        var result = await _controller.DesconectarOAuth();

        var noContentResult = result as NoContentResult;
        noContentResult.Should().NotBeNull();
        noContentResult!.StatusCode.Should().Be(204);
    }
}
