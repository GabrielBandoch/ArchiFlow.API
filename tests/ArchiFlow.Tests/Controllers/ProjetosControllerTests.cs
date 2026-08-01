using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Projetos.Commands;
using ArchiFlow.Application.Projetos.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Domain.Projetos.Enum;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Controllers;

public class ProjetosControllerTests
{
    private readonly Mock<IProjetoFacade> _facadeMock;
    private readonly ProjetosController _sut;

    public ProjetosControllerTests()
    {
        _facadeMock = new Mock<IProjetoFacade>();
        _sut        = new ProjetosController(_facadeMock.Object);
    }

    [Fact]
    public async Task GetAll_DeveRetornarOkComProjetos()
    {
        var projetos = new List<ProjetoDto>();
        _facadeMock.Setup(f => f.GetAll()).ReturnsAsync(projetos);

        var result = await _sut.GetAll();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(projetos);
    }

    [Fact]
    public async Task GetById_QuandoProjetoExiste_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var dto = new ProjetoDto(id, "P1", "", StatusProjeto.Briefing, "Briefing", TipoProjeto.Residencial, "Residencial", DateTime.UtcNow, null, 100, Guid.NewGuid(), DateTime.UtcNow, null, new List<EtapaProjetoDto>(), 0);
        _facadeMock.Setup(f => f.GetById(id)).ReturnsAsync(dto);

        var result = await _sut.GetById(id);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetById_QuandoProjetoNaoExiste_DeveRetornarNotFound()
    {
        var id = Guid.NewGuid();
        _facadeMock.Setup(f => f.GetById(id)).ReturnsAsync((ProjetoDto?)null);

        var result = await _sut.GetById(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_DeveRetornarCreated()
    {
        var command = new CriarProjetoCommand("Nome", "Desc", TipoProjeto.Residencial, DateTime.UtcNow, null, 150, Guid.NewGuid());
        var dto = new ProjetoDto(Guid.NewGuid(), "Nome", "Desc", StatusProjeto.Briefing, "Briefing", TipoProjeto.Residencial, "Residencial", DateTime.UtcNow, null, 150, Guid.NewGuid(), DateTime.UtcNow, null, new List<EtapaProjetoDto>(), 0);
        _facadeMock.Setup(f => f.Create(command)).ReturnsAsync(dto);

        var result = await _sut.Create(command);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ProjetosController.GetById));
        createdResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Update_ComIdConsistente_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var command = new AtualizarProjetoCommand(id, "Nome", "Desc", TipoProjeto.Residencial, StatusProjeto.Briefing, DateTime.UtcNow, null, 150);
        var dto = new ProjetoDto(id, "Nome", "Desc", StatusProjeto.Briefing, "Briefing", TipoProjeto.Residencial, "Residencial", DateTime.UtcNow, null, 150, Guid.NewGuid(), DateTime.UtcNow, null, new List<EtapaProjetoDto>(), 0);
        _facadeMock.Setup(f => f.Update(command)).ReturnsAsync(dto);

        var result = await _sut.Update(id, command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Update_ComIdInconsistente_DeveRetornarBadRequest()
    {
        var command = new AtualizarProjetoCommand(Guid.NewGuid(), "Nome", "Desc", TipoProjeto.Residencial, StatusProjeto.Briefing, DateTime.UtcNow, null, 150);

        var result = await _sut.Update(Guid.NewGuid(), command);

        var badResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badResult.Value.Should().Be("ID inconsistente.");
    }

    [Fact]
    public async Task UpdateStatus_ComIdConsistente_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var command = new AtualizarStatusProjetoCommand(id, StatusProjeto.Desenvolvimento);
        var dto = new ProjetoDto(id, "Nome", "Desc", StatusProjeto.Desenvolvimento, "Desenvolvimento", TipoProjeto.Residencial, "Residencial", DateTime.UtcNow, null, 150, Guid.NewGuid(), DateTime.UtcNow, null, new List<EtapaProjetoDto>(), 0);
        _facadeMock.Setup(f => f.UpdateStatus(command)).ReturnsAsync(dto);

        var result = await _sut.UpdateStatus(id, command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task UpdateStatus_ComIdInconsistente_DeveRetornarBadRequest()
    {
        var command = new AtualizarStatusProjetoCommand(Guid.NewGuid(), StatusProjeto.Desenvolvimento);

        var result = await _sut.UpdateStatus(Guid.NewGuid(), command);

        var badResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badResult.Value.Should().Be("ID inconsistente.");
    }

    [Fact]
    public async Task CreateEtapa_ComIdConsistente_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var command = new CriarEtapaCommand(id, "Etapa 1", "Desc", 1);
        var dto = new EtapaProjetoDto(Guid.NewGuid(), id, "Etapa 1", "Desc", StatusEtapa.Pendente, "Pendente", 1, null);
        _facadeMock.Setup(f => f.CreateEtapa(command)).ReturnsAsync(dto);

        var result = await _sut.CreateEtapa(id, command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task CreateEtapa_ComIdInconsistente_DeveRetornarBadRequest()
    {
        var command = new CriarEtapaCommand(Guid.NewGuid(), "Etapa 1", "Desc", 1);

        var result = await _sut.CreateEtapa(Guid.NewGuid(), command);

        var badResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badResult.Value.Should().Be("ID inconsistente.");
    }

    [Fact]
    public async Task UpdateStatusEtapa_ComIdConsistente_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var command = new AtualizarStatusEtapaCommand(id, StatusEtapa.Concluida);
        var dto = new EtapaProjetoDto(id, Guid.NewGuid(), "Etapa 1", "Desc", StatusEtapa.Concluida, "Concluida", 1, DateTime.UtcNow);
        _facadeMock.Setup(f => f.UpdateStatusEtapa(command)).ReturnsAsync(dto);

        var result = await _sut.UpdateStatusEtapa(id, command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task UpdateStatusEtapa_ComIdInconsistente_DeveRetornarBadRequest()
    {
        var command = new AtualizarStatusEtapaCommand(Guid.NewGuid(), StatusEtapa.Concluida);

        var result = await _sut.UpdateStatusEtapa(Guid.NewGuid(), command);

        var badResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badResult.Value.Should().Be("ID inconsistente.");
    }

    [Fact]
    public async Task Delete_DeveRetornarNoContent()
    {
        var id = Guid.NewGuid();
        _facadeMock.Setup(f => f.Delete(id)).Returns(Task.CompletedTask);

        var result = await _sut.Delete(id);

        result.Should().BeOfType<NoContentResult>();
        _facadeMock.Verify(f => f.Delete(id), Times.Once);
    }
}
