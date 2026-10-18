using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Autenticacao.Commands;
using ArchiFlow.Application.Autenticacao.DTOs;
using ArchiFlow.Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Controllers;

public class RecuperacaoSenhaControllerTests
{
    private readonly Mock<IPasswordRecoveryService> _recoveryServiceMock = new();
    private readonly Mock<IAppUrlService> _appUrlServiceMock = new();
    private readonly Mock<ILogger<RecuperacaoSenhaController>> _loggerMock = new();
    private readonly RecuperacaoSenhaController _sut;

    public RecuperacaoSenhaControllerTests()
    {
        _sut = new RecuperacaoSenhaController(
            _recoveryServiceMock.Object,
            _appUrlServiceMock.Object,
            _loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task SolicitarRecuperacaoSenha_DeveChamarServicoERetornarOk()
    {
        var command = new SolicitarRecuperacaoCommand("usuario@teste.com");
        _appUrlServiceMock.Setup(u => u.ObterUrlBaseConfiavel(It.IsAny<string?>())).Returns("https://app.cotu.com.br");

        var result = await _sut.SolicitarRecuperacaoSenha(command);

        result.Should().BeOfType<OkObjectResult>();
        _recoveryServiceMock.Verify(s => s.SolicitarRecuperacaoAsync("usuario@teste.com", "https://app.cotu.com.br"), Times.Once);
    }

    [Fact]
    public async Task SolicitarRecuperacaoSenha_QuandoOcorreExcecao_DeveRetornarOkSemVazarDetalhes()
    {
        var command = new SolicitarRecuperacaoCommand("usuario@teste.com");
        _recoveryServiceMock.Setup(s => s.SolicitarRecuperacaoAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ThrowsAsync(new Exception("Falha SMTP"));

        var result = await _sut.SolicitarRecuperacaoSenha(command);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ValidarTokenRecuperacao_QuandoValido_DeveRetornarOkComResultado()
    {
        var command = new ValidarTokenRecuperacaoCommand("token-valido");
        var tokenDto = new ResultadoValidacaoTokenDto(true, "email@teste.com", null);
        _recoveryServiceMock.Setup(s => s.ValidarTokenAsync("token-valido")).ReturnsAsync(tokenDto);

        var result = await _sut.ValidarTokenRecuperacao(command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(tokenDto);
    }

    [Fact]
    public async Task RedefinirSenha_QuandoSucesso_DeveRetornarOk()
    {
        var command = new RedefinirSenhaCommand("token-valido", "NovaSenha@2026");

        var result = await _sut.RedefinirSenha(command);

        result.Should().BeOfType<OkObjectResult>();
        _recoveryServiceMock.Verify(s => s.RedefinirSenhaAsync("token-valido", "NovaSenha@2026"), Times.Once);
    }

    [Fact]
    public async Task RedefinirSenha_QuandoTokenExpirado_DeveRetornarBadRequest()
    {
        var command = new RedefinirSenhaCommand("token-expirado", "NovaSenha@2026");
        _recoveryServiceMock.Setup(s => s.RedefinirSenhaAsync("token-expirado", "NovaSenha@2026"))
            .ThrowsAsync(new InvalidOperationException("Token expirado"));

        var result = await _sut.RedefinirSenha(command);

        var badResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badResult.Value.Should().BeEquivalentTo(new { message = "Token expirado" });
    }
}
