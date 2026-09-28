using ArchiFlow.Application.Financeiro.Commands;
using ArchiFlow.Application.Financeiro.DTOs;
using ArchiFlow.Application.Financeiro.Facades;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Financeiro;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Financeiro;

public class FinanceiroFacadeTests
{
    private readonly Mock<IFinanceiroService> _serviceMock;
    private readonly FinanceiroFacade _facade;

    public FinanceiroFacadeTests()
    {
        _serviceMock = new Mock<IFinanceiroService>();
        _facade = new FinanceiroFacade(_serviceMock.Object);
    }

    [Fact]
    public async Task ObterPainelConsolidadoAsync_Should_Delegate_To_Service()
    {
        var painel = new PainelFinanceiroDto(100m, 100m, 0m, 0m, 0m, 100m, 0m, new List<ReceitaMesDto>(), new List<AlertaFinanceiroDto>(), new List<ParcelaFinanceiraDto>());
        _serviceMock.Setup(s => s.ObterPainelConsolidadoAsync()).ReturnsAsync(painel);

        var result = await _facade.ObterPainelConsolidadoAsync();

        result.Should().BeEquivalentTo(painel);
        _serviceMock.Verify(s => s.ObterPainelConsolidadoAsync(), Times.Once);
    }

    [Fact]
    public async Task ObterParcelasAsync_Should_Delegate_To_Service()
    {
        var parcelas = new List<ParcelaFinanceiraDto>();
        _serviceMock.Setup(s => s.ObterParcelasAsync(null, null, null, null)).ReturnsAsync(parcelas);

        var result = await _facade.ObterParcelasAsync(null, null, null, null);

        result.Should().BeEquivalentTo(parcelas);
        _serviceMock.Verify(s => s.ObterParcelasAsync(null, null, null, null), Times.Once);
    }

    [Fact]
    public async Task CriarContratoAsync_Should_Delegate_To_Service()
    {
        var command = new CriarContratoCommand(Guid.NewGuid(), 15000m, 3, DateTime.UtcNow, 30, null, null);
        var contratoDto = new ContratoFinanceiroDto(Guid.NewGuid(), command.ProjetoId, "Projeto", 15000m, null, null, DateTime.UtcNow, new List<ParcelaFinanceiraDto>());
        _serviceMock.Setup(s => s.CriarContratoAsync(command)).ReturnsAsync(contratoDto);

        var result = await _facade.CriarContratoAsync(command);

        result.Should().BeEquivalentTo(contratoDto);
        _serviceMock.Verify(s => s.CriarContratoAsync(command), Times.Once);
    }

    [Fact]
    public async Task ObterAlertasAsync_Should_Delegate_To_Service()
    {
        var alertas = new List<AlertaFinanceiroDto>();
        _serviceMock.Setup(s => s.ObterAlertasAsync()).ReturnsAsync(alertas);

        var result = await _facade.ObterAlertasAsync();

        result.Should().BeEquivalentTo(alertas);
        _serviceMock.Verify(s => s.ObterAlertasAsync(), Times.Once);
    }

    [Fact]
    public async Task UploadComprovanteAsync_Should_Delegate_To_Service()
    {
        var command = new UploadComprovanteCommand(null);
        var expected = new ComprovanteUploadResultDto("https://url.com", "file.pdf");
        _serviceMock.Setup(s => s.UploadComprovanteAsync(command)).ReturnsAsync(expected);

        var result = await _facade.UploadComprovanteAsync(command);

        result.Should().BeEquivalentTo(expected);
        _serviceMock.Verify(s => s.UploadComprovanteAsync(command), Times.Once);
    }
}
