using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ArchiFlow.Domain.Usuarios;
using ArchiFlow.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Services;

public class UserContextServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock = new();
    private readonly UserContextService _service;

    public UserContextServiceTests()
    {
        _service = new UserContextService(_httpContextAccessorMock.Object, _usuarioRepoMock.Object);
    }

    [Fact]
    public async Task ObterUsuarioContextoAsync_QuandoUsuarioAutenticadoComEscritorio_DeveRetornarUsuarioEEscritorioId()
    {
        var usuarioId = Guid.NewGuid();
        var escritorioId = Guid.NewGuid();
        var usuario = new Usuario
        {
            Id = usuarioId,
            Nome = "Carlos Arquiteto",
            EscritorioId = escritorioId
        };

        var httpContext = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()) };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);
        _usuarioRepoMock.Setup(r => r.GetById(usuarioId)).ReturnsAsync(usuario);

        var (u, escId) = await _service.ObterUsuarioContextoAsync();

        u.Should().Be(usuario);
        escId.Should().Be(escritorioId);
    }

    [Fact]
    public async Task ObterUsuarioContextoAsync_QuandoEscritorioIdEhNulo_DeveUsarUsuarioIdComoEscritorioId()
    {
        var usuarioId = Guid.NewGuid();
        var usuario = new Usuario
        {
            Id = usuarioId,
            Nome = "Admin Raiz",
            EscritorioId = null
        };

        var httpContext = new DefaultHttpContext();
        var claims = new[] { new Claim("sub", usuarioId.ToString()) };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);
        _usuarioRepoMock.Setup(r => r.GetById(usuarioId)).ReturnsAsync(usuario);

        var (u, escId) = await _service.ObterUsuarioContextoAsync();

        u.Should().Be(usuario);
        escId.Should().Be(usuarioId);
    }

    [Fact]
    public async Task ObterUsuarioContextoAsync_QuandoClaimNameIdPresente_DeveResolverCorretamente()
    {
        var usuarioId = Guid.NewGuid();
        var usuario = new Usuario { Id = usuarioId, Nome = "Test", EscritorioId = Guid.NewGuid() };

        var httpContext = new DefaultHttpContext();
        var claims = new[] { new Claim("nameid", usuarioId.ToString()) };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);
        _usuarioRepoMock.Setup(r => r.GetById(usuarioId)).ReturnsAsync(usuario);

        var (u, _) = await _service.ObterUsuarioContextoAsync();

        u.Should().Be(usuario);
    }

    [Fact]
    public async Task ObterUsuarioContextoAsync_SemHttpContext_DeveLancarUnauthorizedAccessException()
    {
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var act = () => _service.ObterUsuarioContextoAsync();

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*não autenticado*");
    }

    [Fact]
    public async Task ObterUsuarioContextoAsync_ComClaimInvalida_DeveLancarUnauthorizedAccessException()
    {
        var httpContext = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "id-invalido-nao-guid") };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        var act = () => _service.ObterUsuarioContextoAsync();

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*não autenticado ou identidade inválida*");
    }

    [Fact]
    public async Task ObterUsuarioContextoAsync_QuandoUsuarioNaoEncontradoNoRepositorio_DeveLancarUnauthorizedAccessException()
    {
        var usuarioId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()) };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);
        _usuarioRepoMock.Setup(r => r.GetById(usuarioId)).ReturnsAsync((Usuario?)null);

        var act = () => _service.ObterUsuarioContextoAsync();

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*não encontrado*");
    }
}
