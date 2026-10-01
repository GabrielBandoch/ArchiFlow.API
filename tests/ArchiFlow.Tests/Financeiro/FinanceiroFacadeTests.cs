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

    [Fact]
    public async Task ExcluirComprovanteAsync_Should_Delegate_To_Service()
    {
        var url = "https://s3.amazonaws.com/recibo.pdf";
        _serviceMock.Setup(s => s.ExcluirComprovanteAsync(url)).Returns(Task.CompletedTask);

        await _facade.ExcluirComprovanteAsync(url);

        _serviceMock.Verify(s => s.ExcluirComprovanteAsync(url), Times.Once);
    }

    [Fact]
    public async Task ObterContratoPorProjetoIdAsync_Should_Delegate_To_Service()
    {
        var projId = Guid.NewGuid();
        _serviceMock.Setup(s => s.ObterContratoPorProjetoIdAsync(projId)).ReturnsAsync((ContratoFinanceiroDto?)null);

        var result = await _facade.ObterContratoPorProjetoIdAsync(projId);

        result.Should().BeNull();
        _serviceMock.Verify(s => s.ObterContratoPorProjetoIdAsync(projId), Times.Once);
    }

    [Fact]
    public async Task RegistrarParcelaAsync_Should_Delegate_To_Service()
    {
        var cmd = new CriarParcelaCommand(Guid.NewGuid(), null, 1, 1, "Desc", 100m, DateTime.UtcNow, null);
        var expected = new ParcelaFinanceiraDto(Guid.NewGuid(), cmd.ProjetoId, "P", null, null, null, 1, 1, "Desc", 100m, DateTime.UtcNow, null, StatusParcela.Pendente, "Pendente", null, null, null, null, DateTime.UtcNow);
        _serviceMock.Setup(s => s.RegistrarParcelaAsync(cmd)).ReturnsAsync(expected);

        var result = await _facade.RegistrarParcelaAsync(cmd);

        result.Should().BeEquivalentTo(expected);
        _serviceMock.Verify(s => s.RegistrarParcelaAsync(cmd), Times.Once);
    }

    [Fact]
    public async Task AtualizarParcelaAsync_Should_Delegate_To_Service()
    {
        var id = Guid.NewGuid();
        var cmd = new AtualizarParcelaCommand("Desc", 100m, DateTime.UtcNow, StatusParcela.Pendente, null, null, null, null);
        var expected = new ParcelaFinanceiraDto(id, Guid.NewGuid(), "P", null, null, null, 1, 1, "Desc", 100m, DateTime.UtcNow, null, StatusParcela.Pendente, "Pendente", null, null, null, null, DateTime.UtcNow);
        _serviceMock.Setup(s => s.AtualizarParcelaAsync(id, cmd)).ReturnsAsync(expected);

        var result = await _facade.AtualizarParcelaAsync(id, cmd);

        result.Should().BeEquivalentTo(expected);
        _serviceMock.Verify(s => s.AtualizarParcelaAsync(id, cmd), Times.Once);
    }

    [Fact]
    public async Task DarBaixaParcelaAsync_Should_Delegate_To_Service()
    {
        var id = Guid.NewGuid();
        var cmd = new DarBaixaParcelaCommand(DateTime.UtcNow, FormaPagamento.Pix, null, null);
        var expected = new ParcelaFinanceiraDto(id, Guid.NewGuid(), "P", null, null, null, 1, 1, "Desc", 100m, DateTime.UtcNow, DateTime.UtcNow, StatusParcela.Pago, "Pago", FormaPagamento.Pix, "Pix", null, null, DateTime.UtcNow);
        _serviceMock.Setup(s => s.DarBaixaParcelaAsync(id, cmd)).ReturnsAsync(expected);

        var result = await _facade.DarBaixaParcelaAsync(id, cmd);

        result.Should().BeEquivalentTo(expected);
        _serviceMock.Verify(s => s.DarBaixaParcelaAsync(id, cmd), Times.Once);
    }

    [Fact]
    public async Task ExcluirParcelaAsync_Should_Delegate_To_Service()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.ExcluirParcelaAsync(id)).Returns(Task.CompletedTask);

        await _facade.ExcluirParcelaAsync(id);

        _serviceMock.Verify(s => s.ExcluirParcelaAsync(id), Times.Once);
    }

    [Fact]
    public async Task DespesasOperations_Should_Delegate_To_Service()
    {
        var projId = Guid.NewGuid();
        var despesas = new List<DespesaProjetoDto>();
        _serviceMock.Setup(s => s.ObterDespesasAsync(projId, null, null, null)).ReturnsAsync(despesas);
        var listResult = await _facade.ObterDespesasAsync(projId, null, null, null);
        listResult.Should().BeEquivalentTo(despesas);

        var despesaId = Guid.NewGuid();
        var dto = new DespesaProjetoDto(despesaId, projId, "Proj", "Desc", 100m, DateTime.UtcNow, CategoriaDespesa.Outros, "Outros", null, null, DateTime.UtcNow);
        _serviceMock.Setup(s => s.ObterDespesaPorIdAsync(despesaId)).ReturnsAsync(dto);
        var itemResult = await _facade.ObterDespesaPorIdAsync(despesaId);
        itemResult.Should().BeEquivalentTo(dto);

        var createCmd = new CriarDespesaCommand(projId, "Desc", 100m, DateTime.UtcNow, CategoriaDespesa.Outros, null, null);
        _serviceMock.Setup(s => s.CriarDespesaAsync(createCmd)).ReturnsAsync(dto);
        var created = await _facade.CriarDespesaAsync(createCmd);
        created.Should().BeEquivalentTo(dto);

        var updateCmd = new AtualizarDespesaCommand("Desc", 150m, DateTime.UtcNow, CategoriaDespesa.Outros, null, null);
        _serviceMock.Setup(s => s.AtualizarDespesaAsync(despesaId, updateCmd)).ReturnsAsync(dto);
        var updated = await _facade.AtualizarDespesaAsync(despesaId, updateCmd);
        updated.Should().BeEquivalentTo(dto);

        _serviceMock.Setup(s => s.ExcluirDespesaAsync(despesaId)).Returns(Task.CompletedTask);
        await _facade.ExcluirDespesaAsync(despesaId);
        _serviceMock.Verify(s => s.ExcluirDespesaAsync(despesaId), Times.Once);
    }
}
