using ArchiFlow.API.Middleware;
using ArchiFlow.Infrastructure.MultiTenancy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Middleware;

public class TenantResolutionMiddlewareTests
{
    private readonly Mock<ILogger<TenantResolutionMiddleware>> _loggerMock;

    public TenantResolutionMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<TenantResolutionMiddleware>>();
    }

    [Fact]
    public async Task InvokeAsync_QuandoChamado_DeveChamarProximoDelegateEAdicionarHeaderXTenantResolved()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("duna.archiflow.com.br");

        var proximoChamado = false;
        RequestDelegate next = (ctx) =>
        {
            proximoChamado = true;
            return Task.CompletedTask;
        };

        var tenantContext = new TenantContext();
        var middleware = new TenantResolutionMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context, tenantContext);

        proximoChamado.Should().BeTrue();
        tenantContext.TenantId.Should().Be("duna");
        tenantContext.IsResolved.Should().BeTrue();
        context.Response.Headers[TenantResolutionMiddleware.TenantResolvedHeaderName].ToString().Should().Be("duna");
    }

    [Fact]
    public void ResolveTenant_ComHeaderXTenantId_DevePriorizarHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("outro-escritorio.archiflow.com.br");
        context.Request.Headers[TenantResolutionMiddleware.TenantHeaderName] = "duna-arquitetura";

        var tenant = TenantResolutionMiddleware.ResolveTenant(context);

        tenant.Should().Be("duna-arquitetura");
    }

    [Fact]
    public void ResolveTenant_ComSubdominioSimples_DeveExtrairTenant()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("duna.archiflow.com.br");

        var tenant = TenantResolutionMiddleware.ResolveTenant(context);

        tenant.Should().Be("duna");
    }

    [Fact]
    public void ResolveTenant_ComSubdominioApi_DeveExtrairSegundoSegmento()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("api.duna.archiflow.com.br");

        var tenant = TenantResolutionMiddleware.ResolveTenant(context);

        tenant.Should().Be("duna");
    }

    [Fact]
    public void ResolveTenant_ComDominioProprioCustomizado_DeveExtrairNomeDominio()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("dunaarquitetura.com");

        var tenant = TenantResolutionMiddleware.ResolveTenant(context);

        tenant.Should().Be("dunaarquitetura");
    }

    [Fact]
    public void ResolveTenant_ComLocalhost_DeveRetornarDefault()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("localhost:5000");

        var tenant = TenantResolutionMiddleware.ResolveTenant(context);

        tenant.Should().Be("default");
    }

    [Fact]
    public void TenantContext_BuildConnectionString_DeveSubstituirDatabaseCorretamente()
    {
        var tenantContext = new TenantContext();
        tenantContext.SetTenant("duna-arquitetura", "duna.archiflow.com.br");

        tenantContext.DatabaseName.Should().Be("archiflow_duna_arquitetura");

        var baseConn = "Host=vps.hostinger.internal;Port=5432;Database=archiflow;Username=archiflow_user;Password=secure_pass";
        var tenantConn = tenantContext.BuildConnectionString(baseConn);

        tenantConn.Should().Contain("Database=archiflow_duna_arquitetura");
        tenantConn.Should().Contain("Host=vps.hostinger.internal");
    }

    [Fact]
    public void TenantContext_QuandoDefault_DeveUtilizarDatabasePadrao()
    {
        var tenantContext = new TenantContext();
        tenantContext.SetTenant("default", "localhost");

        tenantContext.DatabaseName.Should().Be("archiflow");

        var baseConn = "Host=localhost;Port=5432;Database=archiflow;Username=postgres;Password=123";
        var tenantConn = tenantContext.BuildConnectionString(baseConn);

        tenantConn.Should().Contain("Database=archiflow");
    }
}
