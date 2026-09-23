using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Honorarios.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Projetos.Enum;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace ArchiFlow.Tests.Controllers;

public class HonorariosControllerTests
{
    private readonly Mock<IPropostaHonorarioFacade> _mockFacade;
    private readonly HonorariosController _controller;

    public HonorariosControllerTests()
    {
        _mockFacade = new Mock<IPropostaHonorarioFacade>();
        _controller = new HonorariosController(_mockFacade.Object);
    }

    [Fact]
    public void Simular_Should_Throw_When_Metragem_Zero()
    {
        var parametros = new SimulacaoParametrosDto(0, TipoProjeto.Residencial, PadraoImovel.Medio, null, null, null);
        _mockFacade.Setup(f => f.Simular(parametros)).Throws<ArgumentException>();

        var act = () => _controller.Simular(parametros);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Simular_Should_Return_Ok_When_Valid()
    {
        var parametros = new SimulacaoParametrosDto(150m, TipoProjeto.Residencial, PadraoImovel.Medio, null, null, null);
        var simulacao = new SimulacaoResultadoDto(150m, TipoProjeto.Residencial, "Residencial", PadraoImovel.Medio, "Médio", 18450m, 123m, 120m, new List<ItemEtapaSimulacaoDto>(), new MemoriaCalculoDto(150m, 95m, 14250m, "Médio", 1m, 0m, "Residencial", 1.1m, 1425m, 100m, 1425m, 120m, 150m));

        _mockFacade.Setup(f => f.Simular(parametros)).Returns(simulacao);

        var result = _controller.Simular(parametros);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(simulacao);
    }
}
