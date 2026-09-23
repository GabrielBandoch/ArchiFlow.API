using ArchiFlow.Application.Honorarios.Commands;
using ArchiFlow.Application.Honorarios.DTOs;
using ArchiFlow.Application.Honorarios.Facades;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Projetos.Enum;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Facades;

public class PropostaHonorarioFacadeTests
{
    private readonly Mock<ICalculadoraHonorariosService> _mockCalculadora;
    private readonly Mock<IPropostaHonorarioService> _mockService;
    private readonly PropostaHonorarioFacade _facade;

    public PropostaHonorarioFacadeTests()
    {
        _mockCalculadora = new Mock<ICalculadoraHonorariosService>();
        _mockService = new Mock<IPropostaHonorarioService>();
        _facade = new PropostaHonorarioFacade(_mockCalculadora.Object, _mockService.Object);
    }

    [Fact]
    public void Simular_Should_Call_Calculadora()
    {
        var parametros = new SimulacaoParametrosDto(150m, TipoProjeto.Residencial, PadraoImovel.Medio, null, null, null);
        var simulacao = new SimulacaoResultadoDto(150m, TipoProjeto.Residencial, "Residencial", PadraoImovel.Medio, "Médio", 18450m, 123m, 120m, new List<ItemEtapaSimulacaoDto>(), new MemoriaCalculoDto(150m, 95m, 14250m, "Médio", 1m, 0m, "Residencial", 1.1m, 1425m, 100m, 1425m, 120m, 150m));

        _mockCalculadora.Setup(c => c.Calcular(parametros)).Returns(simulacao);

        var result = _facade.Simular(parametros);

        result.Should().BeEquivalentTo(simulacao);
    }

    [Fact]
    public async Task GetAll_And_GetById_Should_Delegate_To_Service()
    {
        var id = Guid.NewGuid();
        var dto = new PropostaHonorarioDto(id, "Prop", "PROP-001", null, null, null, null, TipoProjeto.Residencial, "Residencial", PadraoImovel.Medio, "Médio", 100m, 150m, 95m, 80m, 9500m, 0m, 950m, 950m, 10450m, 10450m, StatusProposta.Rascunho, "Rascunho", null, DateTime.UtcNow, null, new List<ItemPropostaEtapaDto>());

        _mockService.Setup(s => s.GetAll()).ReturnsAsync(new List<PropostaHonorarioDto> { dto });
        _mockService.Setup(s => s.GetById(id)).ReturnsAsync(dto);

        var all = await _facade.GetAll();
        var byId = await _facade.GetById(id);

        all.Should().ContainSingle();
        byId.Should().Be(dto);
    }
}
