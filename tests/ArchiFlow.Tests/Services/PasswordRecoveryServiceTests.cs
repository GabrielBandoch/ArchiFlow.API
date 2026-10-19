using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArchiFlow.Application.Autenticacao.Services;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Shared;
using ArchiFlow.Domain.Usuarios;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Services;

public class PasswordRecoveryServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock = new();
    private readonly Mock<IRepresentanteClienteRepository> _representanteRepoMock = new();
    private readonly Mock<IClienteRepository> _clienteRepoMock = new();
    private readonly Mock<ITokenRecuperacaoSenhaRepository> _tokenRepoMock = new();
    private readonly Mock<IEmailService> _emailMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ILogger<PasswordRecoveryService>> _loggerMock = new();
    private readonly PasswordRecoveryService _service;

    public PasswordRecoveryServiceTests()
    {
        var repositories = new PasswordRecoveryRepositories(
            _usuarioRepoMock.Object,
            _representanteRepoMock.Object,
            _clienteRepoMock.Object,
            _tokenRepoMock.Object);
        _service = new PasswordRecoveryService(
            repositories,
            _emailMock.Object,
            _uowMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task SolicitarRecuperacaoAsync_WhenUserNotFound_CompletesSilentlyWithoutEmail()
    {
        _usuarioRepoMock.Setup(r => r.GetByEmail("inexistente@teste.com")).ReturnsAsync((Usuario?)null);
        _representanteRepoMock.Setup(r => r.ObterPorEmailAsync("inexistente@teste.com")).ReturnsAsync((RepresentanteCliente?)null);
        _clienteRepoMock.Setup(r => r.GetByEmail("inexistente@teste.com")).ReturnsAsync((Cliente?)null);

        await _service.SolicitarRecuperacaoAsync("inexistente@teste.com", "http://localhost");

        _tokenRepoMock.Verify(t => t.AdicionarAsync(It.IsAny<TokenRecuperacaoSenha>()), Times.Never);
        _emailMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SolicitarRecuperacaoAsync_WhenUserExists_StoresHashedTokenAndSendsEmail()
    {
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "arquiteto@teste.com",
            Nome = "Arquiteto Teste",
            Ativo = true,
            SenhaHash = "hash"
        };
        _usuarioRepoMock.Setup(r => r.GetByEmail(usuario.Email)).ReturnsAsync(usuario);

        await _service.SolicitarRecuperacaoAsync(usuario.Email, "http://localhost:4200");

        _tokenRepoMock.Verify(t => t.AdicionarAsync(It.Is<TokenRecuperacaoSenha>(tok =>
            tok.Email == usuario.Email &&
            !string.IsNullOrEmpty(tok.TokenHash) &&
            tok.ExpiraEm > DateTime.UtcNow
        )), Times.Once);

        _emailMock.Verify(e => e.SendEmailAsync(
            usuario.Email,
            It.Is<string>(s => s.Contains("Recuperação de Senha")),
            It.Is<string>(body => body.Contains("redefinir-senha"))
        ), Times.Once);
    }

    [Fact]
    public async Task SolicitarRecuperacaoAsync_WhenBaseUrlNull_UsesFallbackDomain()
    {
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "usuario.fallback@teste.com",
            Nome = "Fallback Teste",
            Ativo = true,
            SenhaHash = "hash"
        };
        _usuarioRepoMock.Setup(r => r.GetByEmail(usuario.Email)).ReturnsAsync(usuario);

        await _service.SolicitarRecuperacaoAsync(usuario.Email, null);

        _emailMock.Verify(e => e.SendEmailAsync(
            usuario.Email,
            It.Is<string>(s => s.Contains("Recuperação de Senha")),
            It.Is<string>(body => body.Contains("cotu.com.br") || body.Contains("http"))
        ), Times.Once);
    }

    [Fact]
    public async Task ValidarTokenAsync_WhenTokenInvalidOrExpired_ReturnsInvalido()
    {
        _tokenRepoMock.Setup(t => t.ObterPorTokenHashAsync(It.IsAny<string>())).ReturnsAsync((TokenRecuperacaoSenha?)null);

        var resultado = await _service.ValidarTokenAsync("token-invalido");

        resultado.Valido.Should().BeFalse();
        resultado.Mensagem.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RedefinirSenhaAsync_WhenTokenValid_UpdatesPasswordAndInvalidatesToken()
    {
        var rawToken = "raw-secret-token-123";
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "user@teste.com",
            Nome = "User",
            Ativo = true,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("senhaAntiga123")
        };

        var tokenEntity = new TokenRecuperacaoSenha
        {
            Id = Guid.NewGuid(),
            TokenHash = tokenHash,
            Email = usuario.Email,
            ExpiraEm = DateTime.UtcNow.AddMinutes(30),
            UsadoEm = null,
            TipoUsuario = "staff"
        };

        _tokenRepoMock.Setup(t => t.ObterPorTokenHashAsync(tokenHash)).ReturnsAsync(tokenEntity);
        _usuarioRepoMock.Setup(u => u.GetByEmail(usuario.Email)).ReturnsAsync(usuario);

        await _service.RedefinirSenhaAsync(rawToken, "NovaSenhaForte@123");

        tokenEntity.UsadoEm.Should().NotBeNull();
        BCrypt.Net.BCrypt.Verify("NovaSenhaForte@123", usuario.SenhaHash).Should().BeTrue();
        _uowMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidarTokenAsync_WhenTokenValid_ReturnsValido()
    {
        var rawToken = "raw-token-valid";
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();
        var tokenEntity = new TokenRecuperacaoSenha
        {
            Id = Guid.NewGuid(),
            TokenHash = tokenHash,
            Email = "teste@teste.com",
            ExpiraEm = DateTime.UtcNow.AddMinutes(30),
            UsadoEm = null
        };
        _tokenRepoMock.Setup(t => t.ObterPorTokenHashAsync(tokenHash)).ReturnsAsync(tokenEntity);

        var resultado = await _service.ValidarTokenAsync(rawToken);

        resultado.Valido.Should().BeTrue();
        resultado.Email.Should().Be("teste@teste.com");
    }

    [Fact]
    public async Task RedefinirSenhaAsync_WhenTokenIsForRepresentative_UpdatesRepresentativePassword()
    {
        var rawToken = "rep-token-123";
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();
        var rep = new RepresentanteCliente
        {
            Id = Guid.NewGuid(),
            Email = "rep@teste.com",
            Nome = "Rep",
            Ativo = true,
            SenhaHash = "old"
        };
        var tokenEntity = new TokenRecuperacaoSenha
        {
            Id = Guid.NewGuid(),
            TokenHash = tokenHash,
            Email = rep.Email,
            ExpiraEm = DateTime.UtcNow.AddMinutes(30),
            UsadoEm = null,
            TipoUsuario = "representante"
        };

        _tokenRepoMock.Setup(t => t.ObterPorTokenHashAsync(tokenHash)).ReturnsAsync(tokenEntity);
        _representanteRepoMock.Setup(r => r.ObterPorEmailAsync(rep.Email)).ReturnsAsync(rep);

        await _service.RedefinirSenhaAsync(rawToken, "NovaSenha@123");

        tokenEntity.UsadoEm.Should().NotBeNull();
        BCrypt.Net.BCrypt.Verify("NovaSenha@123", rep.SenhaHash).Should().BeTrue();
        _representanteRepoMock.Verify(r => r.AtualizarAsync(rep), Times.Once);
    }

    [Fact]
    public async Task RedefinirSenhaAsync_WhenTokenIsForClient_UpdatesClientPassword()
    {
        var rawToken = "client-token-123";
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Email = "cliente@teste.com",
            Nome = "Cliente",
            Ativo = true
        };
        var tokenEntity = new TokenRecuperacaoSenha
        {
            Id = Guid.NewGuid(),
            TokenHash = tokenHash,
            Email = cliente.Email,
            ExpiraEm = DateTime.UtcNow.AddMinutes(30),
            UsadoEm = null,
            TipoUsuario = "cliente"
        };

        _tokenRepoMock.Setup(t => t.ObterPorTokenHashAsync(tokenHash)).ReturnsAsync(tokenEntity);
        _clienteRepoMock.Setup(c => c.GetByEmail(cliente.Email)).ReturnsAsync(cliente);

        await _service.RedefinirSenhaAsync(rawToken, "NovaSenha@123");

        tokenEntity.UsadoEm.Should().NotBeNull();
        BCrypt.Net.BCrypt.Verify("NovaSenha@123", cliente.SenhaPortal).Should().BeTrue();
        _clienteRepoMock.Setup(c => c.Update(cliente));
    }
}
