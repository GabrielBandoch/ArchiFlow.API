using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Application.Usuarios.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Controllers;

public class AutenticacaoControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AutenticacaoController _sut;

    public AutenticacaoControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _sut = new AutenticacaoController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarOk()
    {
        var request = new LoginRequestDto("email@test.com", "senha123");
        var responseDto = new LoginResponseDto("token-valido", "Administrador", "Arquiteto", "email@test.com", Guid.NewGuid());
        _authServiceMock.Setup(s => s.Login(request)).ReturnsAsync(responseDto);

        var result = await _sut.Login(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(responseDto);
    }

    [Fact]
    public async Task Login_ComCredenciaisInvalidas_DeveRetornarUnauthorized()
    {
        var request = new LoginRequestDto("email@test.com", "senha-errada");
        _authServiceMock.Setup(s => s.Login(request)).ThrowsAsync(new UnauthorizedAccessException("Invalido"));

        var result = await _sut.Login(request);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Register_ComDadosValidos_DeveRetornarCreated()
    {
        var request = new RegisterRequestDto("Nome", "novo@test.com", "senha123", "Administrador");
        var newId = Guid.NewGuid();
        _authServiceMock.Setup(s => s.Registrar(request)).ReturnsAsync(newId);

        var result = await _sut.Register(request);

        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.Value.Should().BeEquivalentTo(new { id = newId });
    }

    [Fact]
    public async Task Register_ComEmailDuplicado_DeveRetornarBadRequest()
    {
        var request = new RegisterRequestDto("Nome", "duplicado@test.com", "senha123", "Administrador");
        _authServiceMock.Setup(s => s.Registrar(request)).ThrowsAsync(new InvalidOperationException("E-mail já cadastrado."));

        var result = await _sut.Register(request);

        var badResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badResult.Value.Should().BeEquivalentTo(new { message = "E-mail já cadastrado." });
    }
}
