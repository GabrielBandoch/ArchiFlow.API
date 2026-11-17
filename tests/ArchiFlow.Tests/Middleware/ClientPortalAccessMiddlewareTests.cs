using System;
using System.IO;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using ArchiFlow.API.Middleware;
using ArchiFlow.Domain.Clientes;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Middleware;

public class ClientPortalAccessMiddlewareTests
{
    private static DefaultHttpContext CreateContext(ClaimsPrincipal user, IServiceProvider services)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = services
        };
        context.Response.Body = new MemoryStream();
        context.User = user;
        return context;
    }

    private static ClaimsPrincipal CreateClientUser(Guid clienteId, Guid? repId = null)
    {
        var claims = new System.Collections.Generic.List<Claim>
        {
            new(ClaimTypes.NameIdentifier, (repId ?? clienteId).ToString()),
            new(ClaimTypes.Role, "Cliente"),
            new("user_type", "client"),
            new("cliente_id", clienteId.ToString())
        };
        if (repId.HasValue)
        {
            claims.Add(new("representative_id", repId.Value.ToString()));
        }
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
    }

    [Fact]
    public async Task InvokeAsync_QuandoClientePortalSuspenso_DeveRetornar401()
    {
        var clienteId = Guid.NewGuid();
        var clienteRepoMock = new Mock<IClienteRepository>();
        clienteRepoMock.Setup(r => r.GetById(clienteId))
            .ReturnsAsync(new Cliente { Id = clienteId, Ativo = false });

        var services = new ServiceCollection()
            .AddSingleton(clienteRepoMock.Object)
            .BuildServiceProvider();

        var nextCalled = false;
        var middleware = new ClientPortalAccessMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = CreateContext(CreateClientUser(clienteId), services);
        await middleware.InvokeAsync(context);

        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(401);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("error").GetString().Should().Contain("suspenso");
    }

    [Fact]
    public async Task InvokeAsync_QuandoRepresentanteInativo_DeveRetornar401()
    {
        var clienteId = Guid.NewGuid();
        var repId = Guid.NewGuid();

        var clienteRepoMock = new Mock<IClienteRepository>();
        clienteRepoMock.Setup(r => r.GetById(clienteId))
            .ReturnsAsync(new Cliente { Id = clienteId, Ativo = true });

        var repRepoMock = new Mock<IRepresentanteClienteRepository>();
        repRepoMock.Setup(r => r.ObterPorIdAsync(repId))
            .ReturnsAsync(new RepresentanteCliente { Id = repId, ClienteId = clienteId, Ativo = false });

        var services = new ServiceCollection()
            .AddSingleton(clienteRepoMock.Object)
            .AddSingleton(repRepoMock.Object)
            .BuildServiceProvider();

        var nextCalled = false;
        var middleware = new ClientPortalAccessMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = CreateContext(CreateClientUser(clienteId, repId), services);
        await middleware.InvokeAsync(context);

        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_QuandoClienteERepresentanteAtivos_DeveContinuarPipeline()
    {
        var clienteId = Guid.NewGuid();
        var repId = Guid.NewGuid();

        var clienteRepoMock = new Mock<IClienteRepository>();
        clienteRepoMock.Setup(r => r.GetById(clienteId))
            .ReturnsAsync(new Cliente { Id = clienteId, Ativo = true });

        var repRepoMock = new Mock<IRepresentanteClienteRepository>();
        repRepoMock.Setup(r => r.ObterPorIdAsync(repId))
            .ReturnsAsync(new RepresentanteCliente { Id = repId, ClienteId = clienteId, Ativo = true });

        var services = new ServiceCollection()
            .AddSingleton(clienteRepoMock.Object)
            .AddSingleton(repRepoMock.Object)
            .BuildServiceProvider();

        var nextCalled = false;
        var middleware = new ClientPortalAccessMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = CreateContext(CreateClientUser(clienteId, repId), services);
        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_QuandoStaffAutenticado_DeveContinuarPipelineSemBloquear()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "ArquitetoAdmin"),
            new("user_type", "staff")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        var services = new ServiceCollection().BuildServiceProvider();

        var nextCalled = false;
        var middleware = new ClientPortalAccessMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = CreateContext(user, services);
        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }
}
