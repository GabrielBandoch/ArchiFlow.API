using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Application.Usuarios.DTOs;
using ArchiFlow.Application.Usuarios.Services;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Shared;
using ArchiFlow.Domain.Usuarios;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUsuarioRepository> _userRepoMock;
    private readonly Mock<IClienteRepository> _clientRepoMock;
    private readonly Mock<IProjetoRepository> _projRepoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUsuarioRepository>();
        _clientRepoMock = new Mock<IClienteRepository>();
        _projRepoMock = new Mock<IProjetoRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _tokenServiceMock = new Mock<ITokenService>();

        _sut = new AuthService(
            _userRepoMock.Object,
            _clientRepoMock.Object,
            _projRepoMock.Object,
            _uowMock.Object,
            _tokenServiceMock.Object
        );
    }

    [Fact]
    public async Task Registrar_QuandoEmailNaoExiste_DeveCadastrarEHashSenha()
    {
        var request = new RegisterRequestDto("Nome", "email@test.com", "senha123", "Administrador");
        _userRepoMock.Setup(r => r.GetByEmail(request.Email)).ReturnsAsync((Usuario?)null);

        var result = await _sut.Registrar(request);

        result.Should().NotBeEmpty();
        _userRepoMock.Verify(r => r.Create(It.Is<Usuario>(u => 
            u.Email == request.Email && 
            u.Nome == request.Nome && 
            u.Role == request.Role && 
            u.SenhaHash != request.Senha
        )), Times.Once);
        _uowMock.Verify(u => u.Commit(default), Times.Once);
    }

    [Fact]
    public async Task Registrar_QuandoEmailJaExiste_DeveLancarInvalidOperationException()
    {
        var request = new RegisterRequestDto("Nome", "email@test.com", "senha123", "Administrador");
        _userRepoMock.Setup(r => r.GetByEmail(request.Email)).ReturnsAsync(new Usuario());

        var act = async () => await _sut.Registrar(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Login_ComCredenciaisValidasDeUsuario_DeveRetornarToken()
    {
        var email = "user@test.com";
        var password = "password123";
        var request = new LoginRequestDto(email, password);

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Nome",
            Email = email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12),
            Role = "Administrador",
            Ativo = true
        };

        _userRepoMock.Setup(r => r.GetByEmail(email)).ReturnsAsync(usuario);
        _tokenServiceMock.Setup(t => t.GenerateToken(usuario)).Returns("valid-token");

        var response = await _sut.Login(request);

        response.Should().NotBeNull();
        response.Token.Should().Be("valid-token");
        response.Perfil.Should().Be("Administrador");
        response.Nome.Should().Be("Nome");
    }

    [Fact]
    public async Task Login_ComCredenciaisValidasDeCliente_DeveRetornarToken()
    {
        var email = "cliente@test.com";
        var password = "password123";
        var request = new LoginRequestDto(email, password);

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = "Cliente",
            Email = email,
            SenhaPortal = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12),
            Ativo = true
        };

        var projeto = new Projeto { Id = Guid.NewGuid() };

        _userRepoMock.Setup(r => r.GetByEmail(email)).ReturnsAsync((Usuario?)null);
        _clientRepoMock.Setup(r => r.GetByEmail(email)).ReturnsAsync(cliente);
        _projRepoMock.Setup(r => r.GetByClienteId(cliente.Id)).ReturnsAsync(new List<Projeto> { projeto });
        _tokenServiceMock.Setup(t => t.GenerateToken(cliente, projeto.Id)).Returns("client-token");

        var response = await _sut.Login(request);

        response.Should().NotBeNull();
        response.Token.Should().Be("client-token");
        response.Perfil.Should().Be("Cliente");
        response.Nome.Should().Be("Cliente");
        response.ProjetoId.Should().Be(projeto.Id);
    }

    [Fact]
    public async Task Login_ComSenhaIncorretaParaUsuario_DeveLancarUnauthorizedAccessException()
    {
        var email = "user@test.com";
        var request = new LoginRequestDto(email, "errada");

        var usuario = new Usuario
        {
            Email = email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("correta", workFactor: 12),
            Ativo = true
        };

        _userRepoMock.Setup(r => r.GetByEmail(email)).ReturnsAsync(usuario);

        var act = async () => await _sut.Login(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Login_ComUsuarioInativo_DeveLancarUnauthorizedAccessException()
    {
        var email = "user@test.com";
        var request = new LoginRequestDto(email, "correta");

        var usuario = new Usuario
        {
            Email = email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("correta", workFactor: 12),
            Ativo = false
        };

        _userRepoMock.Setup(r => r.GetByEmail(email)).ReturnsAsync(usuario);

        var act = async () => await _sut.Login(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Login_ComClienteInativo_DeveLancarUnauthorizedAccessException()
    {
        var email = "cliente@test.com";
        var request = new LoginRequestDto(email, "correta");

        var cliente = new Cliente
        {
            Email = email,
            SenhaPortal = BCrypt.Net.BCrypt.HashPassword("correta", workFactor: 12),
            Ativo = false
        };

        _userRepoMock.Setup(r => r.GetByEmail(email)).ReturnsAsync((Usuario?)null);
        _clientRepoMock.Setup(r => r.GetByEmail(email)).ReturnsAsync(cliente);

        var act = async () => await _sut.Login(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Login_ComEmailInexistente_DeveLancarUnauthorizedAccessException()
    {
        var request = new LoginRequestDto("inexistente@test.com", "senha");

        _userRepoMock.Setup(r => r.GetByEmail(request.Email)).ReturnsAsync((Usuario?)null);
        _clientRepoMock.Setup(r => r.GetByEmail(request.Email)).ReturnsAsync((Cliente?)null);

        var act = async () => await _sut.Login(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Login_ComClienteSemProjetos_DeveRetornarTokenSemProjetoId()
    {
        var email = "cliente-sem-projeto@test.com";
        var password = "password123";
        var request = new LoginRequestDto(email, password);

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = "Cliente Sem Projeto",
            Email = email,
            SenhaPortal = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12),
            Ativo = true
        };

        _userRepoMock.Setup(r => r.GetByEmail(email)).ReturnsAsync((Usuario?)null);
        _clientRepoMock.Setup(r => r.GetByEmail(email)).ReturnsAsync(cliente);
        _projRepoMock.Setup(r => r.GetByClienteId(cliente.Id)).ReturnsAsync(new List<Projeto>());
        _tokenServiceMock.Setup(t => t.GenerateToken(cliente, null)).Returns("client-no-project-token");

        var response = await _sut.Login(request);

        response.Should().NotBeNull();
        response.Token.Should().Be("client-no-project-token");
        response.ProjetoId.Should().BeNull();
    }

    [Fact]
    public async Task Login_ComClienteSenhaNula_DeveLancarUnauthorizedAccessException()
    {
        var email = "cliente-sem-senha@test.com";
        var request = new LoginRequestDto(email, "qualquer");

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Email = email,
            SenhaPortal = null, // Senha nula
            Ativo = true
        };

        _userRepoMock.Setup(r => r.GetByEmail(email)).ReturnsAsync((Usuario?)null);
        _clientRepoMock.Setup(r => r.GetByEmail(email)).ReturnsAsync(cliente);

        var act = async () => await _sut.Login(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
