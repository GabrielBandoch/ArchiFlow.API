using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Honorarios.Commands;
using ArchiFlow.Application.Honorarios.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Projetos.Enum;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Controllers;

public class PropostasControllerTests
{
    private readonly Mock<IPropostaHonorarioFacade> _mockFacade;
    private readonly PropostasController _controller;

    public PropostasControllerTests()
    {
        _mockFacade = new Mock<IPropostaHonorarioFacade>();
        _controller = new PropostasController(_mockFacade.Object);
    }

    [Fact]
    public async Task GetAll_Should_Return_Ok_With_List()
    {
        _mockFacade.Setup(f => f.GetAll()).ReturnsAsync(new List<PropostaHonorarioDto>());

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_Should_Return_NotFound_When_Null()
    {
        var id = Guid.NewGuid();
        _mockFacade.Setup(f => f.GetById(id)).ReturnsAsync((PropostaHonorarioDto?)null);

        var result = await _controller.GetById(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Criar_Should_Return_CreatedAtAction_When_Valid()
    {
        var cmd = new CriarPropostaCommand("Proposta Casa", null, null, null, null, TipoProjeto.Residencial, PadraoImovel.Medio, 150m, null, null, null, null, null);
        var dto = new PropostaHonorarioDto(Guid.NewGuid(), "Proposta Casa", "PROP-001", null, null, null, null, TipoProjeto.Residencial, "Residencial", PadraoImovel.Medio, "Médio", 150m, 150m, 95m, 120m, 14250m, 0m, 1425m, 1425m, 15675m, 15675m, StatusProposta.Rascunho, "Rascunho", null, DateTime.UtcNow, null, new List<ItemPropostaEtapaDto>());

        _mockFacade.Setup(f => f.Criar(cmd)).ReturnsAsync(dto);

        var result = await _controller.Criar(cmd);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Excluir_Should_Return_NoContent_When_Success()
    {
        var id = Guid.NewGuid();
        _mockFacade.Setup(f => f.Excluir(id)).ReturnsAsync(true);

        var result = await _controller.Excluir(id);

        result.Should().BeOfType<NoContentResult>();
    }
}
