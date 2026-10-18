using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Autenticacao.Commands;
using ArchiFlow.Application.Autenticacao.DTOs;
using ArchiFlow.Application.Clientes.Commands;
using ArchiFlow.Application.Clientes.DTOs;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Application.Usuarios.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Controllers;

public class AutenticacaoControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly Mock<IPasswordRecoveryService> _recoveryServiceMock = new();
    private readonly Mock<IRepresentanteClienteService> _representanteServiceMock = new();
    private readonly Mock<IAppUrlService> _appUrlServiceMock = new();
    private readonly Mock<ILogger<AutenticacaoController>> _loggerMock = new();
    private readonly AutenticacaoController _sut;

    public AutenticacaoControllerTests()
    {
        _sut = new AutenticacaoController(
            _authServiceMock.Object,
            _recoveryServiceMock.Object,
            _representanteServiceMock.Object,
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
