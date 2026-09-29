using ArchiFlow.Application.Dashboard.DTOs;
using ArchiFlow.Application.Dashboard.Facades;
using ArchiFlow.Application.Interfaces.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Dashboard;

public class DashboardFacadeTests
{
    private readonly Mock<IDashboardService> _serviceMock;
    private readonly DashboardFacade _facade;

    public DashboardFacadeTests()
    {
        _serviceMock = new Mock<IDashboardService>();
        _facade = new DashboardFacade(_serviceMock.Object);
    }

    [Fact]
    public async Task ObterMetricasAsync_Should_Call_Service()
    {
        var expectedDto = new DashboardMetricasDto(
            new DashboardKpisDto(1, 0, 1, 0, 0, 1, 1, 1000, 1000),
            new List<ProjetosPorStatusDto>(),
            new List<ProjetosPorTipoDto>(),
            new List<LeadsPorStatusDto>(),
            new List<LeadsPorOrigemDto>(),
            new List<PropostasMensalDto>(),
            new List<ProjetoResumoDashboardDto>(),
            new List<LeadResumoDashboardDto>(),
            new List<PropostaResumoDashboardDto>()
        );

        _serviceMock.Setup(s => s.ObterMetricasAsync())
            .ReturnsAsync(expectedDto);

        var result = await _facade.ObterMetricasAsync();

        result.Should().Be(expectedDto);
        _serviceMock.Verify(s => s.ObterMetricasAsync(), Times.Once);
    }

    [Fact]
    public async Task ObterPreferenciasAsync_Parameterless_Should_Call_Service()
    {
        var expectedDto = new PreferenciaDashboardDto(Guid.NewGuid(), "{}", DateTime.UtcNow);

        _serviceMock.Setup(s => s.ObterPreferenciasAsync())
            .ReturnsAsync(expectedDto);

        var result = await _facade.ObterPreferenciasAsync();

        result.Should().Be(expectedDto);
        _serviceMock.Verify(s => s.ObterPreferenciasAsync(), Times.Once);
    }

    [Fact]
    public async Task SalvarPreferenciasAsync_Should_Call_Service()
    {
        var command = new SalvarPreferenciaDashboardCommand("[]");
        var expectedDto = new PreferenciaDashboardDto(Guid.NewGuid(), "[]", DateTime.UtcNow);

        _serviceMock.Setup(s => s.SalvarPreferenciasAsync(command))
            .ReturnsAsync(expectedDto);

        var result = await _facade.SalvarPreferenciasAsync(command);

        result.Should().Be(expectedDto);
        _serviceMock.Verify(s => s.SalvarPreferenciasAsync(command), Times.Once);
    }
}
