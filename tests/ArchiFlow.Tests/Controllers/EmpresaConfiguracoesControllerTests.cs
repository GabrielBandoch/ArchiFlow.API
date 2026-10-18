using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Empresa.Commands;
using ArchiFlow.Application.Empresa.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Controllers;

public class EmpresaConfiguracoesControllerTests
{
    private readonly Mock<IEmpresaFacade> _empresaFacadeMock = new();
    private readonly EmpresaConfiguracoesController _sut;

    public EmpresaConfiguracoesControllerTests()
    {
        _sut = new EmpresaConfiguracoesController(_empresaFacadeMock.Object);
    }

    private static DadosEmpresaDto CriarDto(string nome = "Studio Teste", string cor = "#2563EB") =>
        new(Guid.NewGuid(), null, nome, null, null, null, null, null, null, null, null, null, null, null, null, cor, DateTime.UtcNow);

    [Fact]
    public async Task ObterDadosEmpresa_DeveRetornarOkComDados()
    {
        var dto = CriarDto("Empresa Teste");
        _empresaFacadeMock.Setup(f => f.ObterDadosEmpresaAsync()).ReturnsAsync(dto);

        var result = await _sut.ObterDadosEmpresa();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task SalvarDadosEmpresa_DeveRetornarOkComDadosSalvos()
    {
        var command = new SalvarDadosEmpresaCommand("Empresa Salva");
        var dto = CriarDto("Empresa Salva");
        _empresaFacadeMock.Setup(f => f.SalvarDadosEmpresaAsync(command)).ReturnsAsync(dto);

        var result = await _sut.SalvarDadosEmpresa(command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task AtualizarCorPrimaria_QuandoCorVazia_DeveRetornarBadRequest()
    {
        var command = new AtualizarCorPrimariaCommand { CorPrimaria = "" };

        var result = await _sut.AtualizarCorPrimaria(command);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AtualizarCorPrimaria_QuandoCorValida_DeveRetornarOk()
    {
        var command = new AtualizarCorPrimariaCommand { CorPrimaria = "#FF0000" };
        var dto = CriarDto(cor: "#FF0000");
        _empresaFacadeMock.Setup(f => f.AtualizarCorPrimariaAsync("#FF0000")).ReturnsAsync(dto);

        var result = await _sut.AtualizarCorPrimaria(command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task UploadLogo_QuandoArquivoNulo_DeveRetornarBadRequest()
    {
        var result = await _sut.UploadLogo(null!);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadLogo_QuandoFormatoInvalido_DeveRetornarBadRequest()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100);
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");

        var result = await _sut.UploadLogo(fileMock.Object);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadLogo_QuandoValido_DeveRetornarOkComUrl()
    {
        var fileMock = new Mock<IFormFile>();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("fake-image-content"));
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.ContentType).Returns("image/png");
        fileMock.Setup(f => f.FileName).Returns("logo.png");
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

        _empresaFacadeMock.Setup(f => f.AtualizarLogotipoAsync(It.IsAny<Stream>(), "logo.png", "image/png"))
            .ReturnsAsync("https://cdn.teste.com/logo.png");

        var result = await _sut.UploadLogo(fileMock.Object);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { logoUrl = "https://cdn.teste.com/logo.png" });
    }

    [Fact]
    public async Task RemoverLogo_DeveRetornarNoContent()
    {
        var result = await _sut.RemoverLogo();

        result.Should().BeOfType<NoContentResult>();
        _empresaFacadeMock.Verify(f => f.RemoverLogotipoAsync(), Times.Once);
    }
}
