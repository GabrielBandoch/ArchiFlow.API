using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Fornecedores.Commands;
using ArchiFlow.Application.Fornecedores.DTOs;
using ArchiFlow.Application.Fornecedores.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Controllers;

public class FornecedoresControllerTests
{
    private readonly Mock<IFornecedorService> _serviceMock;
    private readonly FornecedoresController _controller;

    public FornecedoresControllerTests()
    {
        _serviceMock = new Mock<IFornecedorService>();
        _controller = new FornecedoresController(_serviceMock.Object);
    }

    [Fact]
    public async Task ObterTodos_DeveRetornarOkComLista()
    {
        _serviceMock.Setup(s => s.ObterTodosAsync(null)).ReturnsAsync(new List<FornecedorDto>());

        var result = await _controller.ObterTodos(null);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ObterPorId_QuandoNaoExiste_DeveRetornarNotFound()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.ObterPorIdAsync(id)).ReturnsAsync((FornecedorDto?)null);

        var result = await _controller.ObterPorId(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ObterPorId_QuandoExiste_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var dto = new FornecedorDto { Id = id, Nome = "Fornecedor Teste" };
        _serviceMock.Setup(s => s.ObterPorIdAsync(id)).ReturnsAsync(dto);

        var result = await _controller.ObterPorId(id);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Criar_DeveRetornarCreatedAtAction()
    {
        var cmd = new CriarFornecedorCommand { Nome = "Novo", Especialidade = "Marcenaria" };
        var dto = new FornecedorDto { Id = Guid.NewGuid(), Nome = "Novo" };
        _serviceMock.Setup(s => s.CriarAsync(cmd)).ReturnsAsync(dto);

        var result = await _controller.Criar(cmd);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Atualizar_ComIdDivergente_DeveRetornarBadRequest()
    {
        var id = Guid.NewGuid();
        var cmd = new AtualizarFornecedorCommand { Id = Guid.NewGuid(), Nome = "Atualizado" };

        var result = await _controller.Atualizar(id, cmd);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Excluir_QuandoSucesso_DeveRetornarNoContent()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.ExcluirAsync(id)).ReturnsAsync(true);

        var result = await _controller.Excluir(id);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Excluir_QuandoNaoExiste_DeveRetornarNotFound()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.ExcluirAsync(id)).ReturnsAsync(false);

        var result = await _controller.Excluir(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task AdicionarAvaliacao_DeveRetornarCreatedComDto()
    {
        var id = Guid.NewGuid();
        var cmd = new AdicionarAvaliacaoCommand { Nota = 5, Comentario = "Top" };
        var dto = new AvaliacaoFornecedorDto { Id = Guid.NewGuid(), FornecedorId = id, Nota = 5 };
        _serviceMock.Setup(s => s.AdicionarAvaliacaoAsync(cmd)).ReturnsAsync(dto);

        var result = await _controller.AdicionarAvaliacao(id, cmd);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.Value.Should().Be(dto);
        cmd.FornecedorId.Should().Be(id);
    }

    [Fact]
    public async Task VincularProjeto_DeveRetornarCreatedComDto()
    {
        var id = Guid.NewGuid();
        var cmd = new VincularProjetoCommand { ProjetoId = Guid.NewGuid(), FuncaoNoProjeto = "Eletrica" };
        var dto = new ProjetoFornecedorDto { Id = Guid.NewGuid(), FornecedorId = id, FuncaoNoProjeto = "Eletrica" };
        _serviceMock.Setup(s => s.VincularProjetoAsync(cmd)).ReturnsAsync(dto);

        var result = await _controller.VincularProjeto(id, cmd);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.Value.Should().Be(dto);
        cmd.FornecedorId.Should().Be(id);
    }

    [Fact]
    public async Task DesvincularProjeto_QuandoExiste_DeveRetornarNoContent()
    {
        var vinculoId = Guid.NewGuid();
        _serviceMock.Setup(s => s.DesvincularProjetoAsync(vinculoId)).ReturnsAsync(true);

        var result = await _controller.DesvincularProjeto(vinculoId);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DesvincularProjeto_QuandoNaoExiste_DeveRetornarNotFound()
    {
        var vinculoId = Guid.NewGuid();
        _serviceMock.Setup(s => s.DesvincularProjetoAsync(vinculoId)).ReturnsAsync(false);

        var result = await _controller.DesvincularProjeto(vinculoId);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ObterFornecedoresDoProjeto_DeveRetornarOkComLista()
    {
        var projetoId = Guid.NewGuid();
        var lista = new List<ProjetoFornecedorDto>
        {
            new() { Id = Guid.NewGuid(), ProjetoId = projetoId, FornecedorNome = "F1" }
        };
        _serviceMock.Setup(s => s.ObterFornecedoresDoProjetoAsync(projetoId)).ReturnsAsync(lista);

        var result = await _controller.ObterFornecedoresDoProjeto(projetoId);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(lista);
    }
}
