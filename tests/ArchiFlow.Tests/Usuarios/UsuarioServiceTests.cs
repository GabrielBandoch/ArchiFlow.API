using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Application.Usuarios.Commands;
using ArchiFlow.Application.Usuarios.Services;
using ArchiFlow.Domain.Shared;
using ArchiFlow.Domain.Usuarios;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Usuarios;

public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ILogger<UsuarioService>> _loggerMock;
    private readonly UsuarioService _service;

    private readonly Guid _usuarioLogadoId = Guid.NewGuid();
    private readonly Guid _escritorioId = Guid.NewGuid();
    private readonly Usuario _usuarioLogado;

    public UsuarioServiceTests()
    {
        _usuarioRepoMock = new Mock<IUsuarioRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _emailServiceMock = new Mock<IEmailService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _loggerMock = new Mock<ILogger<UsuarioService>>();

        _usuarioLogado = new Usuario
        {
            Id = _usuarioLogadoId,
            EscritorioId = _escritorioId,
            Nome = "Arquiteto Titular",
            Email = "titular@studio.com",
            Role = Roles.ArquitetoAdmin,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        SetupHttpContext(_usuarioLogadoId);

        _usuarioRepoMock.Setup(r => r.GetById(_usuarioLogadoId))
            .ReturnsAsync(_usuarioLogado);

        _service = new UsuarioService(
            _usuarioRepoMock.Object,
            _unitOfWorkMock.Object,
            _emailServiceMock.Object,
            _httpContextAccessorMock.Object,
            _loggerMock.Object
        );
    }

    private void SetupHttpContext(Guid? userId)
    {
        var claims = new List<Claim>();
        if (userId.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(httpContext);
    }

    [Fact]
    public async Task ObterEquipeAsync_DeveRetornarMembrosDoEscritorio()
    {
        // Arrange
        var membros = new List<Usuario>
        {
            _usuarioLogado,
            new() { Id = Guid.NewGuid(), EscritorioId = _escritorioId, Nome = "Colaborador 1", Email = "colab@studio.com", Role = Roles.ArquitetoColaborador, Ativo = true }
        };

        _usuarioRepoMock.Setup(r => r.ObterPorEscritorioIdAsync(_escritorioId))
            .ReturnsAsync(membros);

        // Act
        var result = (await _service.ObterEquipeAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Select(m => m.Nome).Should().Contain("Arquiteto Titular");
        result.Select(m => m.Nome).Should().Contain("Colaborador 1");
    }

    [Fact]
    public async Task ObterMembroPorIdAsync_QuandoPertenceAoEscritorio_DeveRetornarDto()
    {
        // Arrange
        var membroId = Guid.NewGuid();
        var membro = new Usuario
        {
            Id = membroId,
            EscritorioId = _escritorioId,
            Nome = "Estagiario Teste",
            Email = "estagio@studio.com",
            Role = Roles.Estagiario,
            Cargo = "Estagiário de Arquitetura",
            Ativo = true
        };

        _usuarioRepoMock.Setup(r => r.GetById(membroId)).ReturnsAsync(membro);

        // Act
        var result = await _service.ObterMembroPorIdAsync(membroId);

        // Assert
        result.Should().NotBeNull();
        result!.Nome.Should().Be("Estagiario Teste");
        result.Cargo.Should().Be("Estagiário de Arquitetura");
    }

    [Fact]
    public async Task ObterMembroPorIdAsync_QuandoPertenceAOutroEscritorio_DeveRetornarNull()
    {
        // Arrange
        var outroMembroId = Guid.NewGuid();
        var membroOutroEscritorio = new Usuario
        {
            Id = outroMembroId,
            EscritorioId = Guid.NewGuid(),
            Nome = "Outro",
            Email = "outro@escritorio2.com",
            Role = Roles.Colaborador
        };

        _usuarioRepoMock.Setup(r => r.GetById(outroMembroId)).ReturnsAsync(membroOutroEscritorio);

        // Act
        var result = await _service.ObterMembroPorIdAsync(outroMembroId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ConvidarMembroAsync_ComDadosValidos_DevePersistirEnviarEmailERetornarDto()
    {
        // Arrange
        var command = new ConvidarMembroEquipeCommand(
            "Maria Designer",
            "maria@studio.com",
            Roles.ArquitetoColaborador,
            "Arquiteta Júnior",
            "(11) 98888-7777",
            "SenhaSegura@123"
        );

        _usuarioRepoMock.Setup(r => r.GetByEmail("maria@studio.com")).ReturnsAsync((Usuario?)null);

        // Act
        var result = await _service.ConvidarMembroAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be("Maria Designer");
        result.Email.Should().Be("maria@studio.com");
        result.Role.Should().Be(Roles.ArquitetoColaborador);
        result.EscritorioId.Should().Be(_escritorioId);

        _usuarioRepoMock.Verify(r => r.Create(It.Is<Usuario>(u =>
            u.Email == "maria@studio.com" &&
            u.EscritorioId == _escritorioId &&
            u.Role == Roles.ArquitetoColaborador
        )), Times.Once);

        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendEmailAsync("maria@studio.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ConvidarMembroAsync_QuandoEmailJaCadastrado_DeveLancarInvalidOperationException()
    {
        // Arrange
        var command = new ConvidarMembroEquipeCommand(
            "Duplicado",
            "existente@studio.com",
            Roles.Colaborador,
            null,
            null,
            null
        );

        _usuarioRepoMock.Setup(r => r.GetByEmail("existente@studio.com"))
            .ReturnsAsync(new Usuario { Id = Guid.NewGuid(), Email = "existente@studio.com" });

        // Act & Assert
        var act = async () => await _service.ConvidarMembroAsync(command);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Já existe um usuário cadastrado*");
    }

    [Theory]
    [InlineData("", "maria@studio.com", Roles.Colaborador)]
    [InlineData("Maria", "email-invalido", Roles.Colaborador)]
    [InlineData("Maria", "maria@studio.com", "RoleInexistente")]
    public async Task ConvidarMembroAsync_QuandoDadosInvalidos_DeveLancarArgumentException(string nome, string email, string role)
    {
        // Arrange
        var command = new ConvidarMembroEquipeCommand(nome, email, role, null, null, null);

        // Act & Assert
        var act = async () => await _service.ConvidarMembroAsync(command);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AtualizarMembroAsync_ComDadosValidos_DeveAtualizarComSucesso()
    {
        // Arrange
        var membroId = Guid.NewGuid();
        var membro = new Usuario
        {
            Id = membroId,
            EscritorioId = _escritorioId,
            Nome = "Antigo Nome",
            Email = "membro@studio.com",
            Role = Roles.Colaborador,
            Ativo = true
        };

        _usuarioRepoMock.Setup(r => r.GetById(membroId)).ReturnsAsync(membro);

        var command = new AtualizarMembroEquipeCommand(
            "Novo Nome",
            Roles.Financeiro,
            "Coordenador Financeiro",
            "(11) 99999-0000"
        );

        // Act
        var result = await _service.AtualizarMembroAsync(membroId, command);

        // Assert
        result.Nome.Should().Be("Novo Nome");
        result.Role.Should().Be(Roles.Financeiro);
        result.Cargo.Should().Be("Coordenador Financeiro");
        _usuarioRepoMock.Verify(r => r.Update(membro), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarMembroAsync_QuandoPertenceAOutroEscritorio_DeveLancarUnauthorizedAccessException()
    {
        // Arrange
        var membroId = Guid.NewGuid();
        var membro = new Usuario
        {
            Id = membroId,
            EscritorioId = Guid.NewGuid(), // Outro escritório
            Nome = "Outro",
            Email = "outro@teste.com",
            Role = Roles.Colaborador
        };

        _usuarioRepoMock.Setup(r => r.GetById(membroId)).ReturnsAsync(membro);

        var command = new AtualizarMembroEquipeCommand("Nome", Roles.Colaborador, null, null);

        // Act & Assert
        var act = async () => await _service.AtualizarMembroAsync(membroId, command);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task AtualizarMembroAsync_ComNovoEmailValido_DeveAtualizarEmailComSucesso()
    {
        // Arrange
        var membroId = Guid.NewGuid();
        var membro = new Usuario
        {
            Id = membroId,
            EscritorioId = _escritorioId,
            Nome = "Nome",
            Email = "antigo@studio.com",
            Role = Roles.Colaborador
        };

        _usuarioRepoMock.Setup(r => r.GetById(membroId)).ReturnsAsync(membro);
        _usuarioRepoMock.Setup(r => r.GetByEmail("novo@studio.com")).ReturnsAsync((Usuario?)null);

        var command = new AtualizarMembroEquipeCommand("Nome Atualizado", Roles.Colaborador, null, null, "novo@studio.com");

        // Act
        var result = await _service.AtualizarMembroAsync(membroId, command);

        // Assert
        result.Email.Should().Be("novo@studio.com");
        membro.Email.Should().Be("novo@studio.com");
        _usuarioRepoMock.Verify(r => r.Update(membro), Times.Once);
    }

    [Fact]
    public async Task AtualizarMembroAsync_ComEmailJaExistenteEmOutroUsuario_DeveLancarInvalidOperationException()
    {
        // Arrange
        var membroId = Guid.NewGuid();
        var membro = new Usuario
        {
            Id = membroId,
            EscritorioId = _escritorioId,
            Nome = "Nome",
            Email = "membro@studio.com",
            Role = Roles.Colaborador
        };

        var outroUsuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "outro@studio.com"
        };

        _usuarioRepoMock.Setup(r => r.GetById(membroId)).ReturnsAsync(membro);
        _usuarioRepoMock.Setup(r => r.GetByEmail("outro@studio.com")).ReturnsAsync(outroUsuario);

        var command = new AtualizarMembroEquipeCommand("Nome", Roles.Colaborador, null, null, "outro@studio.com");

        // Act & Assert
        var act = async () => await _service.AtualizarMembroAsync(membroId, command);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Já existe outro usuário cadastrado com este e-mail*");
    }

    [Fact]
    public async Task AtualizarMembroAsync_ComEmailInvalido_DeveLancarArgumentException()
    {
        // Arrange
        var membroId = Guid.NewGuid();
        var membro = new Usuario
        {
            Id = membroId,
            EscritorioId = _escritorioId,
            Nome = "Nome",
            Email = "membro@studio.com",
            Role = Roles.Colaborador
        };

        _usuarioRepoMock.Setup(r => r.GetById(membroId)).ReturnsAsync(membro);

        var command = new AtualizarMembroEquipeCommand("Nome", Roles.Colaborador, null, null, "email-invalido");

        // Act & Assert
        var act = async () => await _service.AtualizarMembroAsync(membroId, command);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*E-mail em formato inválido*");
    }

    [Fact]
    public async Task AlterarStatusMembroAsync_QuandoTentaDesativarPropriaConta_DeveLancarInvalidOperationException()
    {
        // Arrange
        var command = new AlterarStatusMembroCommand(Ativo: false);

        // Act & Assert
        var act = async () => await _service.AlterarStatusMembroAsync(_usuarioLogadoId, command);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Não é permitido desativar a própria conta*");
    }

    [Fact]
    public async Task AlterarStatusMembroAsync_QuandoOutroMembro_DeveAlterarComSucesso()
    {
        // Arrange
        var membroId = Guid.NewGuid();
        var membro = new Usuario
        {
            Id = membroId,
            EscritorioId = _escritorioId,
            Nome = "Colaborador",
            Email = "colab@studio.com",
            Role = Roles.Colaborador,
            Ativo = true
        };

        _usuarioRepoMock.Setup(r => r.GetById(membroId)).ReturnsAsync(membro);
        var command = new AlterarStatusMembroCommand(Ativo: false);

        // Act
        var result = await _service.AlterarStatusMembroAsync(membroId, command);

        // Assert
        result.Ativo.Should().BeFalse();
        membro.Ativo.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RedefinirSenhaMembroAsync_ComSucesso_DeveAtualizarSenhaEEnviarEmail()
    {
        // Arrange
        var membroId = Guid.NewGuid();
        var membro = new Usuario
        {
            Id = membroId,
            EscritorioId = _escritorioId,
            Nome = "Colaborador",
            Email = "colab@studio.com",
            Role = Roles.Colaborador,
            Ativo = true
        };

        _usuarioRepoMock.Setup(r => r.GetById(membroId)).ReturnsAsync(membro);
        var command = new RedefinirSenhaMembroCommand(NovaSenha: "NovaSenha123!");

        // Act
        await _service.RedefinirSenhaMembroAsync(membroId, command);

        // Assert
        BCrypt.Net.BCrypt.Verify("NovaSenha123!", membro.SenhaHash).Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendEmailAsync("colab@studio.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ExcluirMembroAsync_QuandoTentaExcluirSiMesmo_DeveLancarInvalidOperationException()
    {
        // Act & Assert
        var act = async () => await _service.ExcluirMembroAsync(_usuarioLogadoId);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Não é permitido excluir a própria conta*");
    }

    [Fact]
    public async Task ExcluirMembroAsync_QuandoOutroMembro_DeveExcluirComSucesso()
    {
        // Arrange
        var membroId = Guid.NewGuid();
        var membro = new Usuario
        {
            Id = membroId,
            EscritorioId = _escritorioId,
            Nome = "Membro",
            Email = "membro@studio.com",
            Role = Roles.Colaborador
        };

        _usuarioRepoMock.Setup(r => r.GetById(membroId)).ReturnsAsync(membro);

        // Act
        await _service.ExcluirMembroAsync(membroId);

        // Assert
        _usuarioRepoMock.Verify(r => r.Delete(membroId), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Contexto_QuandoUsuarioNaoAutenticado_DeveLancarUnauthorizedAccessException()
    {
        // Arrange
        SetupHttpContext(null);

        // Act & Assert
        var act = async () => await _service.ObterEquipeAsync();
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
