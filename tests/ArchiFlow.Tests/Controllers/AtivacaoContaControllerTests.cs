using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Clientes.Commands;
using ArchiFlow.Application.Clientes.DTOs;
using ArchiFlow.Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Controllers;

public class AtivacaoContaControllerTests
{
    private readonly Mock<IRepresentanteClienteService> _representanteServiceMock = new();
    private readonly AtivacaoContaController _sut;

    public AtivacaoContaControllerTests()
    {
        _sut = new AtivacaoContaController(_representanteServiceMock.Object);
    }

    [Fact]
    public async Task ValidarTokenConvite_QuandoValido_DeveRetornarOk()
    {
        var tokenDto = new ResultadoValidacaoConviteDto(true, "Empresa Alfa", "rep@alfa.com", null);
        _representanteServiceMock.Setup(s => s.ValidarTokenConviteAsync("token-rep")).ReturnsAsync(tokenDto);

        var result = await _sut.ValidarTokenConvite("token-rep");

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(tokenDto);
    }

    [Fact]
    public async Task AtivarConta_QuandoSucesso_DeveRetornarOk()
    {
        var command = new AtivarContaRepresentanteCommand("token-rep", "Senha@123");

        var result = await _sut.AtivarConta(command);

        result.Should().BeOfType<OkObjectResult>();
        _representanteServiceMock.Verify(s => s.AtivarContaAsync(command), Times.Once);
    }

    [Fact]
    public async Task AtivarConta_QuandoInvalido_DeveRetornarBadRequest()
    {
        var command = new AtivarContaRepresentanteCommand("token-invalido", "Senha@123");
        _representanteServiceMock.Setup(s => s.AtivarContaAsync(command))
            .ThrowsAsync(new InvalidOperationException("Token inválido"));

        var result = await _sut.AtivarConta(command);

        var badResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badResult.Value.Should().BeEquivalentTo(new { message = "Token inválido" });
    }
}
