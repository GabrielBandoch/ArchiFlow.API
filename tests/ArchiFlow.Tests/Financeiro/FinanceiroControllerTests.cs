using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Financeiro.Commands;
using ArchiFlow.Application.Financeiro.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Financeiro;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Financeiro;


public class FinanceiroControllerTests
{
    private readonly Mock<IFinanceiroFacade> _facadeMock;
    private readonly FinanceiroController _controller;

    public FinanceiroControllerTests()
    {
        _facadeMock = new Mock<IFinanceiroFacade>();
        _controller = new FinanceiroController(_facadeMock.Object);
    }

    [Fact]
    public async Task ObterPainel_Should_Return_Ok_With_PainelDto()
    {
        var painelDto = new PainelFinanceiroDto(
            150000m, 100000m, 40000m, 10000m, 15000m, 85000m, 12.5m,
            new List<ReceitaMesDto>(), new List<AlertaFinanceiroDto>(), new List<ParcelaFinanceiraDto>()
        );

        _facadeMock.Setup(f => f.ObterPainelConsolidadoAsync()).ReturnsAsync(painelDto);

        var result = await _controller.ObterPainel();

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(painelDto);
    }

    [Fact]
    public async Task ObterParcelas_Should_Return_Ok_With_List()
    {
        var parcelas = new List<ParcelaFinanceiraDto>();
        _facadeMock.Setup(f => f.ObterParcelasAsync(null, null, null, null)).ReturnsAsync(parcelas);

        var result = await _controller.ObterParcelas(null, null, null, null);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ObterParcelaPorId_When_Found_Should_Return_Ok()
    {
        var id = Guid.NewGuid();
        var dto = new ParcelaFinanceiraDto(
            id, Guid.NewGuid(), "Projeto A", null, null, null,
            1, 1, "Parcela 1", 5000m, DateTime.UtcNow, null,
            StatusParcela.Pendente, "Pendente", null, null, null, null, DateTime.UtcNow
        );

        _facadeMock.Setup(f => f.ObterParcelaPorIdAsync(id)).ReturnsAsync(dto);

        var result = await _controller.ObterParcelaPorId(id);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ObterParcelaPorId_When_Not_Found_Should_Return_NotFound()
    {
        var id = Guid.NewGuid();
        _facadeMock.Setup(f => f.ObterParcelaPorIdAsync(id)).ReturnsAsync((ParcelaFinanceiraDto?)null);

        var result = await _controller.ObterParcelaPorId(id);

        var notFoundResult = result as NotFoundResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CriarContrato_Should_Return_Ok_With_Created_Contract()
    {
        var command = new CriarContratoCommand(Guid.NewGuid(), 20000m, 2, DateTime.UtcNow, 30, "Condições", "Obs");
        var contratoDto = new ContratoFinanceiroDto(Guid.NewGuid(), command.ProjetoId, "Projeto X", 20000m, "Condições", "Obs", DateTime.UtcNow, new List<ParcelaFinanceiraDto>());

        _facadeMock.Setup(f => f.CriarContratoAsync(command)).ReturnsAsync(contratoDto);

        var result = await _controller.CriarContrato(command);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task DarBaixaParcela_Should_Return_Ok()
    {
        var id = Guid.NewGuid();
        var command = new DarBaixaParcelaCommand(DateTime.UtcNow, FormaPagamento.Pix, "Pago", null);
        var dto = new ParcelaFinanceiraDto(
            id, Guid.NewGuid(), "Projeto", null, null, null,
            1, 1, "Parcela 1", 5000m, DateTime.UtcNow, DateTime.UtcNow,
            StatusParcela.Pago, "Pago", FormaPagamento.Pix, "Pix", "Pago", null, DateTime.UtcNow
        );

        _facadeMock.Setup(f => f.DarBaixaParcelaAsync(id, command)).ReturnsAsync(dto);

        var result = await _controller.DarBaixaParcela(id, command);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ExcluirParcela_Should_Return_NoContent()
    {
        var id = Guid.NewGuid();
        _facadeMock.Setup(f => f.ExcluirParcelaAsync(id)).Returns(Task.CompletedTask);

        var result = await _controller.ExcluirParcela(id);

        var noContentResult = result as NoContentResult;
        noContentResult.Should().NotBeNull();
        noContentResult!.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task UploadComprovante_Should_Delegate_To_Facade()
    {
        var fileMock = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
        var command = new UploadComprovanteCommand(fileMock.Object);
        var expectedDto = new ComprovanteUploadResultDto("https://s3.amazonaws.com/recibo.pdf", "recibo.pdf");

        _facadeMock.Setup(f => f.UploadComprovanteAsync(command)).ReturnsAsync(expectedDto);

        var result = await _controller.UploadComprovante(command);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(expectedDto);
    }

    [Fact]
    public async Task ExcluirComprovante_Should_Return_NoContent()
    {
        var url = "https://s3.amazonaws.com/recibo.pdf";
        _facadeMock.Setup(f => f.ExcluirComprovanteAsync(url)).Returns(Task.CompletedTask);

        var result = await _controller.ExcluirComprovante(url);

        var noContentResult = result as NoContentResult;
        noContentResult.Should().NotBeNull();
        noContentResult!.StatusCode.Should().Be(204);
        _facadeMock.Verify(f => f.ExcluirComprovanteAsync(url), Times.Once);
    }
}

