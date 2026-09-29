using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Dashboard.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Dashboard;

public class DashboardControllerTests
{
    private readonly Mock<IDashboardFacade> _facadeMock;
    private readonly DashboardController _controller;

    public DashboardControllerTests()
    {
        _facadeMock = new Mock<IDashboardFacade>();
        _controller = new DashboardController(_facadeMock.Object);
    }

    [Fact]
    public async Task ObterMetricas_Should_Return_Ok_With_Metricas()
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

        _facadeMock.Setup(f => f.ObterMetricasAsync())
            .ReturnsAsync(expectedDto);

        var actionResult = await _controller.ObterMetricas();

        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedDto);
    }

    [Fact]
    public async Task ObterPreferencias_Should_Return_Ok_With_Preferencias()
    {
        var expectedDto = new PreferenciaDashboardDto(Guid.NewGuid(), "{\"test\":true}", DateTime.UtcNow);

        _facadeMock.Setup(f => f.ObterPreferenciasAsync())
            .ReturnsAsync(expectedDto);

        var actionResult = await _controller.ObterPreferencias();

        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedDto);
    }

    [Fact]
    public async Task SalvarPreferencias_Should_Return_Ok_With_Saved_Preferencias()
    {
        var command = new SalvarPreferenciaDashboardCommand("{\"test\":true}");
        var expectedDto = new PreferenciaDashboardDto(Guid.NewGuid(), command.LayoutJson, DateTime.UtcNow);

        _facadeMock.Setup(f => f.SalvarPreferenciasAsync(command))
            .ReturnsAsync(expectedDto);

        var actionResult = await _controller.SalvarPreferencias(command);

        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedDto);
    }
}
