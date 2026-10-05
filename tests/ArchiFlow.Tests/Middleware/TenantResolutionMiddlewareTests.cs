using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ArchiFlow.API.Extensions;
using ArchiFlow.API.Middleware;
using ArchiFlow.Domain.Shared;
using ArchiFlow.Infrastructure.Data;
using ArchiFlow.Infrastructure.MultiTenancy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Middleware;

public class TenantResolutionMiddlewareTests
{
    private readonly Mock<ILogger<TenantResolutionMiddleware>> _loggerMock = new();

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
    public async Task InvokeAsync_QuandoUsuarioAutenticadoTentaTrocarTenantComHeaderDiferente_DeveRetornar403Forbidden()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("duna.archiflow.com.br");

        var claims = new[]
        {
            new Claim("escritorio_id", "escritorio-autorizado-123"),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));

        // Cliente malicioso tenta injetar tenant diferente
        context.Request.Headers[TenantResolutionMiddleware.TenantHeaderName] = "escritorio-vitima-999";

        var proximoChamado = false;
        RequestDelegate next = (ctx) =>
        {
            proximoChamado = true;
            return Task.CompletedTask;
        };

        var tenantContext = new TenantContext();
        var middleware = new TenantResolutionMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context, tenantContext);

        proximoChamado.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_QuandoUsuarioAutenticado_DeveDefinirTenantDoUsuarioMesmoSemHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("api.archiflow.com.br");

        var claims = new[]
        {
            new Claim("escritorio_id", "escritorio-proprio"),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));

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
        tenantContext.TenantId.Should().Be("escritorio-proprio");
        tenantContext.IsResolved.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_QuandoUsuarioAutenticadoEnviaHeaderCorrespondente_DevePermitir()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("api.archiflow.com.br");

        var claims = new[]
        {
            new Claim("escritorio_id", "escritorio-proprio"),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        context.Request.Headers[TenantResolutionMiddleware.TenantHeaderName] = "escritorio-proprio";

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
        tenantContext.TenantId.Should().Be("escritorio-proprio");
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

    [Fact]
    public void ConfigureDatabase_QuandoTenantResolvido_DeveSelecionarConexaoDoTenant()
    {
        var services = new ServiceCollection();
        var baseConn = "Host=localhost;Port=5432;Database=archiflow;Username=postgres;Password=123";

        services.AddScoped<ITenantContext, TenantContext>();
        services.ConfigureDatabase(baseConn);

        var provider = services.BuildServiceProvider();

        // Scope 1: tenant "estudio-nobre"
        using (var scope = provider.CreateScope())
        {
            var tc = scope.ServiceProvider.GetRequiredService<ITenantContext>();
            tc.SetTenant("estudio-nobre", "estudio.archiflow.com");

            var db = scope.ServiceProvider.GetRequiredService<ArchiFlowDbContext>();
            db.Database.GetConnectionString().Should().Contain("Database=archiflow_estudio_nobre");
        }

        // Scope 2: default tenant
        using (var scope = provider.CreateScope())
        {
            var tc = scope.ServiceProvider.GetRequiredService<ITenantContext>();
            // Not resolved or default
            var db = scope.ServiceProvider.GetRequiredService<ArchiFlowDbContext>();
            db.Database.GetConnectionString().Should().Contain("Database=archiflow");
        }
    }

    [Fact]
    public async Task InvokeAsync_NaoAutenticado_EmAmbienteDev_DeveAceitarHeaderXTenantId()
    {
        var envMock = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Development");

        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("localhost");
        context.Request.Headers[TenantResolutionMiddleware.TenantHeaderName] = "meu-escritorio-dev";

        var proximoChamado = false;
        RequestDelegate next = (ctx) =>
        {
            proximoChamado = true;
            return Task.CompletedTask;
        };

        var tenantContext = new TenantContext();
        var middleware = new TenantResolutionMiddleware(next, _loggerMock.Object, envMock.Object);

        await middleware.InvokeAsync(context, tenantContext);

        proximoChamado.Should().BeTrue();
        tenantContext.TenantId.Should().Be("meu-escritorio-dev");
        context.Response.Headers[TenantResolutionMiddleware.TenantResolvedHeaderName].ToString().Should().Be("meu-escritorio-dev");
    }

    [Fact]
    public async Task InvokeAsync_NaoAutenticado_EmAmbienteProducao_DeveIgnorarHeaderXTenantIdEUsarHost()
    {
        var envMock = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Production");

        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("cliente.archiflow.com.br");
        context.Request.Headers[TenantResolutionMiddleware.TenantHeaderName] = "tentativa-spoofing";

        var proximoChamado = false;
        RequestDelegate next = (ctx) =>
        {
            proximoChamado = true;
            return Task.CompletedTask;
        };

        var tenantContext = new TenantContext();
        var middleware = new TenantResolutionMiddleware(next, _loggerMock.Object, envMock.Object);

        await middleware.InvokeAsync(context, tenantContext);

        proximoChamado.Should().BeTrue();
        tenantContext.TenantId.Should().Be("cliente");
    }

    [Fact]
    public async Task InvokeAsync_NaoAutenticado_ComHeaderDefault_DeveUsarHost()
    {
        var envMock = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Development");

        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("estudio.archiflow.com.br");
        context.Request.Headers[TenantResolutionMiddleware.TenantHeaderName] = "default";

        var proximoChamado = false;
        RequestDelegate next = (ctx) =>
        {
            proximoChamado = true;
            return Task.CompletedTask;
        };

        var tenantContext = new TenantContext();
        var middleware = new TenantResolutionMiddleware(next, _loggerMock.Object, envMock.Object);

        await middleware.InvokeAsync(context, tenantContext);

        proximoChamado.Should().BeTrue();
        tenantContext.TenantId.Should().Be("estudio");
    }

    [Theory]
    [InlineData("127.0.0.1", "default")]
    [InlineData("127.0.0.1:8080", "default")]
    [InlineData("www.archiflow.com.br", "default")]
    [InlineData("api.archiflow.com.br", "default")]
    [InlineData("app.archiflow.com.br", "default")]
    [InlineData("app.escritorio.archiflow.com.br", "escritorio")]
    public void ResolveTenantFromHost_ComDiferentesHosts_DeveResolverCorretamente(string host, string esperado)
    {
        var resultado = TenantResolutionMiddleware.ResolveTenantFromHost(host);
        resultado.Should().Be(esperado);
    }

    [Theory]
    [InlineData("!@#$%", "default")]
    [InlineData("ESCRITORIO_123", "escritorio_123")]
    [InlineData("tenant-nome.valido", "tenant-nomevalido")]
    [InlineData("", "default")]
    public void SanitizeTenant_ComCaracteresDiversos_DeveHigienizar(string input, string esperado)
    {
        var resultado = TenantResolutionMiddleware.SanitizeTenant(input);
        resultado.Should().Be(esperado);
    }
}
