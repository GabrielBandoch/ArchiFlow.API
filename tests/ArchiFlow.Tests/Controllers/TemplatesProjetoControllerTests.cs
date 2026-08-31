using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Projetos.Commands;
using ArchiFlow.Application.Projetos.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Controllers;

public class TemplatesProjetoControllerTests
{
    private readonly Mock<IProjetoFacade> _facadeMock;
    private readonly TemplatesProjetoController _sut;

    public TemplatesProjetoControllerTests()
    {
        _facadeMock = new Mock<IProjetoFacade>();
        _sut = new TemplatesProjetoController(_facadeMock.Object);
    }

    [Fact]
    public async Task ObterTemplates_DeveRetornarOkComLista()
    {
        var templates = new List<TemplateProjetoDto>
        {
            new(Guid.NewGuid(), "residencial", "Residencial", "Desc", "home", true, new List<TemplateEtapaDto>())
        };

        _facadeMock.Setup(f => f.ObterTemplates()).ReturnsAsync(templates);

        var result = await _sut.ObterTemplates();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(templates);
    }

    [Fact]
    public async Task ObterPorId_QuandoExiste_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var template = new TemplateProjetoDto(id, "residencial", "Residencial", "Desc", "home", true, new List<TemplateEtapaDto>());

        _facadeMock.Setup(f => f.ObterTemplatePorId(id)).ReturnsAsync(template);

        var result = await _sut.ObterPorId(id);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(template);
    }

    [Fact]
    public async Task ObterPorId_QuandoNaoExiste_DeveRetornarNotFound()
    {
        var id = Guid.NewGuid();
        _facadeMock.Setup(f => f.ObterTemplatePorId(id)).ReturnsAsync((TemplateProjetoDto?)null);

        var result = await _sut.ObterPorId(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Criar_DeveRetornarCreatedAtAction()
    {
        var command = new CriarTemplateProjetoCommand("residencial", "Residencial", "Desc", "home", new List<CriarTemplateEtapaItemCommand>());
        var created = new TemplateProjetoDto(Guid.NewGuid(), "residencial", "Residencial", "Desc", "home", true, new List<TemplateEtapaDto>());

        _facadeMock.Setup(f => f.CriarTemplate(command)).ReturnsAsync(created);

        var result = await _sut.Criar(command);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(TemplatesProjetoController.ObterPorId));
        createdResult.Value.Should().Be(created);
    }

    [Fact]
    public async Task Atualizar_ComIdConsistente_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var command = new AtualizarTemplateProjetoCommand(id, "Residencial Novo", "Desc", "home", new List<CriarTemplateEtapaItemCommand>());
        var updated = new TemplateProjetoDto(id, "residencial", "Residencial Novo", "Desc", "home", true, new List<TemplateEtapaDto>());

        _facadeMock.Setup(f => f.AtualizarTemplate(command)).ReturnsAsync(updated);

        var result = await _sut.Atualizar(id, command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(updated);
    }

    [Fact]
    public async Task Atualizar_ComIdInconsistente_DeveRetornarBadRequest()
    {
        var command = new AtualizarTemplateProjetoCommand(Guid.NewGuid(), "Residencial", "Desc", "home", new List<CriarTemplateEtapaItemCommand>());

        var result = await _sut.Atualizar(Guid.NewGuid(), command);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().Be("O ID informado na rota diverge do corpo da requisição.");
    }

    [Fact]
    public async Task Excluir_DeveRetornarNoContent()
    {
        var id = Guid.NewGuid();
        _facadeMock.Setup(f => f.ExcluirTemplate(id)).Returns(Task.CompletedTask);

        var result = await _sut.Excluir(id);

        result.Should().BeOfType<NoContentResult>();
        _facadeMock.Verify(f => f.ExcluirTemplate(id), Times.Once);
    }
}
