using ArchiFlow.API.Security;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Security;

public class ProjetoOwnerHandlerTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly ProjetoOwnerHandler _sut;

    public ProjetoOwnerHandlerTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _sut = new ProjetoOwnerHandler(_httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task HandleAsync_QuandoUsuarioEhStaff_DeveRetornarSucesso()
    {
        var claims = new[]
        {
            new Claim("user_type", "staff"),
            new Claim(ClaimTypes.Role, "Colaborador")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var requirement = new ProjetoOwnerRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, principal, null);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_QuandoUsuarioEhClienteEProjetoIdCombina_DeveRetornarSucesso()
    {
        var projetoId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim("user_type", "client"),
            new Claim("projeto_id", projetoId.ToString()),
            new Claim(ClaimTypes.Role, "Cliente")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContextMock = new Mock<HttpContext>();
        var routeData = new RouteData();
        routeData.Values.Add("id", projetoId.ToString());

        httpContextMock.Setup(c => c.Features.Get<IRoutingFeature>())
            .Returns(new RoutingFeature { RouteData = routeData });

        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

        var requirement = new ProjetoOwnerRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, principal, null);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_QuandoUsuarioEhClienteERotaUsaProjetoIdCombina_DeveRetornarSucesso()
    {
        var projetoId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim("user_type", "client"),
            new Claim("projeto_id", projetoId.ToString()),
            new Claim(ClaimTypes.Role, "Cliente")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContextMock = new Mock<HttpContext>();
        var routeData = new RouteData();
        routeData.Values.Add("projetoId", projetoId.ToString());

        httpContextMock.Setup(c => c.Features.Get<IRoutingFeature>())
            .Returns(new RoutingFeature { RouteData = routeData });

        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

        var requirement = new ProjetoOwnerRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, principal, null);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_QuandoUsuarioEhClienteEProjetoIdDiferente_NaoDeveRetornarSucesso()
    {
        var userProjetoId = Guid.NewGuid();
        var routeProjetoId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim("user_type", "client"),
            new Claim("projeto_id", userProjetoId.ToString()),
            new Claim(ClaimTypes.Role, "Cliente")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContextMock = new Mock<HttpContext>();
        var routeData = new RouteData();
        routeData.Values.Add("id", routeProjetoId.ToString());

        httpContextMock.Setup(c => c.Features.Get<IRoutingFeature>())
            .Returns(new RoutingFeature { RouteData = routeData });

        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

        var requirement = new ProjetoOwnerRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, principal, null);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_QuandoUsuarioNaoAutenticado_NaoDeveRetornarSucesso()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var requirement = new ProjetoOwnerRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, principal, null);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_QuandoHttpContextNulo_NaoDeveRetornarSucesso()
    {
        var claims = new[]
        {
            new Claim("user_type", "client"),
            new Claim(ClaimTypes.Role, "Cliente")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var requirement = new ProjetoOwnerRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, principal, null);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_QuandoRotaNaoContemId_NaoDeveRetornarSucesso()
    {
        var claims = new[]
        {
            new Claim("user_type", "client"),
            new Claim(ClaimTypes.Role, "Cliente")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContextMock = new Mock<HttpContext>();
        var routeData = new RouteData();

        httpContextMock.Setup(c => c.Features.Get<IRoutingFeature>())
            .Returns(new RoutingFeature { RouteData = routeData });

        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

        var requirement = new ProjetoOwnerRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, principal, null);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_QuandoRotaIdInvalido_NaoDeveRetornarSucesso()
    {
        var claims = new[]
        {
            new Claim("user_type", "client"),
            new Claim("projeto_id", Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Cliente")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContextMock = new Mock<HttpContext>();
        var routeData = new RouteData();
        routeData.Values.Add("id", "invalid-guid-format");

        httpContextMock.Setup(c => c.Features.Get<IRoutingFeature>())
            .Returns(new RoutingFeature { RouteData = routeData });

        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

        var requirement = new ProjetoOwnerRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, principal, null);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_QuandoClienteSemClaimProjetoId_NaoDeveRetornarSucesso()
    {
        var claims = new[]
        {
            new Claim("user_type", "client"),
            new Claim(ClaimTypes.Role, "Cliente")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContextMock = new Mock<HttpContext>();
        var routeData = new RouteData();
        routeData.Values.Add("id", Guid.NewGuid().ToString());

        httpContextMock.Setup(c => c.Features.Get<IRoutingFeature>())
            .Returns(new RoutingFeature { RouteData = routeData });

        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

        var requirement = new ProjetoOwnerRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, principal, null);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_QuandoClaimProjetoIdInvalido_NaoDeveRetornarSucesso()
    {
        var claims = new[]
        {
            new Claim("user_type", "client"),
            new Claim("projeto_id", "invalid-guid-claim"),
            new Claim(ClaimTypes.Role, "Cliente")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContextMock = new Mock<HttpContext>();
        var routeData = new RouteData();
        routeData.Values.Add("id", Guid.NewGuid().ToString());

        httpContextMock.Setup(c => c.Features.Get<IRoutingFeature>())
            .Returns(new RoutingFeature { RouteData = routeData });

        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

        var requirement = new ProjetoOwnerRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, principal, null);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }
}

#pragma warning disable CS8767
public class RoutingFeature : IRoutingFeature
{
    public RouteData RouteData { get; set; } = new RouteData();
}
#pragma warning restore CS8767
