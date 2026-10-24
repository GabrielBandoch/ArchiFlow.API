using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using ArchiFlow.API.Middleware;
using ArchiFlow.Infrastructure.MultiTenancy;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
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
    public async Task InvokeAsync_RequisicaoNaoAutenticadaComHostValido_DeveResolverTenantEAdicionarHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("duna.cotu.com.br");

        var proximoChamado = false;
        RequestDelegate next = (ctx) =>
        {
            proximoChamado = true;
            return Task.CompletedTask;
        };

        var tenantContext = new TenantContext("cotu");
        var middleware = new TenantResolutionMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context, tenantContext);

        proximoChamado.Should().BeTrue();
        tenantContext.TenantId.Should().Be("duna");
        tenantContext.IsResolved.Should().BeTrue();
        context.Response.Headers[TenantResolutionMiddleware.TenantResolvedHeaderName].ToString().Should().Be("duna");
    }

    [Fact]
    public async Task InvokeAsync_RequisicaoNaoAutenticadaComHostStaging_DeveResolverStaging()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("staging.cotu.com.br");

        var proximoChamado = false;
        RequestDelegate next = (ctx) =>
        {
            proximoChamado = true;
            return Task.CompletedTask;
        };

        var tenantContext = new TenantContext("cotu");
        var middleware = new TenantResolutionMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context, tenantContext);

        proximoChamado.Should().BeTrue();
        tenantContext.TenantId.Should().Be("staging");
        tenantContext.IsResolved.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_RequisicaoNaoAutenticadaComHostLocalhost_DeveResolverDefault()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("localhost:5000");

        var proximoChamado = false;
        RequestDelegate next = (ctx) =>
        {
            proximoChamado = true;
            return Task.CompletedTask;
        };

        var tenantContext = new TenantContext("cotu");
        var middleware = new TenantResolutionMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context, tenantContext);

        proximoChamado.Should().BeTrue();
        tenantContext.TenantId.Should().Be("default");
        tenantContext.IsResolved.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ComUsuarioAutenticadoComEscritorioIdGuidETenantValido_DeveResolverTenantCorretamente()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("duna.cotu.com.br");
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("escritorio_id", Guid.NewGuid().ToString()),
            new Claim("tenant", "duna")
        }, "TestAuth"));

        var proximoChamado = false;
        RequestDelegate next = (ctx) =>
        {
            proximoChamado = true;
            return Task.CompletedTask;
        };

        var tenantContext = new TenantContext("cotu");
        var middleware = new TenantResolutionMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context, tenantContext);

        proximoChamado.Should().BeTrue();
        tenantContext.TenantId.Should().Be("duna");
        tenantContext.IsResolved.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ComUsuarioAutenticadoComClaimTenantValida_DeveResolverTenantCorretamente()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("staging.cotu.com.br");
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("tenant", "staging")
        }, "TestAuth"));

        var proximoChamado = false;
        RequestDelegate next = (ctx) =>
        {
            proximoChamado = true;
            return Task.CompletedTask;
        };

        var tenantContext = new TenantContext("cotu");
        var middleware = new TenantResolutionMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context, tenantContext);

        proximoChamado.Should().BeTrue();
        tenantContext.TenantId.Should().Be("staging");
        tenantContext.IsResolved.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ComUsuarioAutenticadoComClaimLegadaSlug_DeveResolverTenantCorretamente()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("localhost:5000");
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("escritorio_id", "escritorio-ouro")
        }, "TestAuth"));

        var proximoChamado = false;
        RequestDelegate next = (ctx) =>
        {
            proximoChamado = true;
            return Task.CompletedTask;
        };

        var tenantContext = new TenantContext("cotu");
        var middleware = new TenantResolutionMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context, tenantContext);

        proximoChamado.Should().BeTrue();
        tenantContext.TenantId.Should().Be("escritorio-ouro");
        tenantContext.IsResolved.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ComUsuarioAutenticadoComApenasGuidSemTenantClaim_DeveRetornar401Unauthorized()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("duna.cotu.com.br");
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("escritorio_id", Guid.NewGuid().ToString())
        }, "TestAuth"));

        var proximoChamado = false;
        RequestDelegate next = (ctx) =>
        {
            proximoChamado = true;
            return Task.CompletedTask;
        };

        var tenantContext = new TenantContext("cotu");
        var middleware = new TenantResolutionMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context, tenantContext);

        proximoChamado.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task InvokeAsync_ComUsuarioAutenticadoComClaimTenantVazia_DeveRetornar401Unauthorized()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("duna.cotu.com.br");
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("tenant", "   ")
        }, "TestAuth"));

        var proximoChamado = false;
        RequestDelegate next = (ctx) =>
        {
            proximoChamado = true;
            return Task.CompletedTask;
        };

        var tenantContext = new TenantContext("cotu");
        var middleware = new TenantResolutionMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context, tenantContext);

        proximoChamado.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task InvokeAsync_ComUsuarioAutenticadoComClaimTenantFormatoInvalido_DeveRetornar401Unauthorized()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("duna.cotu.com.br");
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("tenant", "tenant!@#$%invalid")
        }, "TestAuth"));

        var proximoChamado = false;
        RequestDelegate next = (ctx) =>
        {
            proximoChamado = true;
            return Task.CompletedTask;
        };

        var tenantContext = new TenantContext("cotu");
        var middleware = new TenantResolutionMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context, tenantContext);

        proximoChamado.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task InvokeAsync_ComUsuarioAutenticadoETentativaSpoofingXTenantId_DeveRetornar403Forbidden()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("duna.cotu.com.br");
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("tenant", "duna")
        }, "TestAuth"));
        context.Request.Headers[TenantResolutionMiddleware.TenantHeaderName] = "outro-escritorio";

        var proximoChamado = false;
        RequestDelegate next = (ctx) =>
        {
            proximoChamado = true;
            return Task.CompletedTask;
        };

        var tenantContext = new TenantContext("cotu");
        var middleware = new TenantResolutionMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context, tenantContext);

        proximoChamado.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_ComDivergenciaEntreTokenEHost_DeveRetornar403Forbidden()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("staging.cotu.com.br");
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("tenant", "duna")
        }, "TestAuth"));

        var proximoChamado = false;
        RequestDelegate next = (ctx) =>
        {
            proximoChamado = true;
            return Task.CompletedTask;
        };

        var tenantContext = new TenantContext("cotu");
        var middleware = new TenantResolutionMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context, tenantContext);

        proximoChamado.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_ComUsuarioAutenticadoEHeaderConsistente_DevePermitirAcesso()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("duna.cotu.com.br");
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("tenant", "duna")
        }, "TestAuth"));
        context.Request.Headers[TenantResolutionMiddleware.TenantHeaderName] = "duna";

        var proximoChamado = false;
        RequestDelegate next = (ctx) =>
        {
            proximoChamado = true;
            return Task.CompletedTask;
        };

        var tenantContext = new TenantContext("cotu");
        var middleware = new TenantResolutionMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context, tenantContext);

        proximoChamado.Should().BeTrue();
        tenantContext.TenantId.Should().Be("duna");
    }

    [Fact]
    public async Task InvokeAsync_EmProducaoNaoAutenticadoComHeader_NaoDeveUsarHeaderDev()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("duna.cotu.com.br");
        context.Request.Headers[TenantResolutionMiddleware.TenantHeaderName] = "outro-tenant";

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Production");

        var proximoChamado = false;
        RequestDelegate next = (ctx) =>
        {
            proximoChamado = true;
            return Task.CompletedTask;
        };

        var tenantContext = new TenantContext("cotu");
        var middleware = new TenantResolutionMiddleware(next, _loggerMock.Object, envMock.Object);

        await middleware.InvokeAsync(context, tenantContext);

        proximoChamado.Should().BeTrue();
        tenantContext.TenantId.Should().Be("duna");
    }

    [Fact]
    public async Task InvokeAsync_EmDevNaoAutenticadoComHeaderInvalido_DeveRetornar400BadRequest()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("localhost:5000");
        context.Request.Headers[TenantResolutionMiddleware.TenantHeaderName] = "invalid!@#header";

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Development");

        var proximoChamado = false;
        RequestDelegate next = (ctx) =>
        {
            proximoChamado = true;
            return Task.CompletedTask;
        };

        var tenantContext = new TenantContext("cotu");
        var middleware = new TenantResolutionMiddleware(next, _loggerMock.Object, envMock.Object);

        await middleware.InvokeAsync(context, tenantContext);

        proximoChamado.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void ResolveTenant_ComSubdominioSimples_DeveExtrairTenant()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("duna.cotu.com.br");

        var tenant = TenantResolutionMiddleware.ResolveTenant(context);

        tenant.Should().Be("duna");
    }

    [Fact]
    public void ResolveTenant_ComSubdominioApi_DeveExtrairSegundoSegmento()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("api.duna.cotu.com.br");

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
        var tenantContext = new TenantContext("cotu");
        tenantContext.SetTenant("duna-arquitetura", "duna.cotu.com.br");

        tenantContext.DatabaseName.Should().Be("cotu_duna_arquitetura");

        var baseConn = "Host=vps.hostinger.internal;Port=5432;Database=cotu;Username=cotu_user;Password=secure_pass";
        var tenantConn = tenantContext.BuildConnectionString(baseConn);

        tenantConn.Should().Contain("Database=cotu_duna_arquitetura");
        tenantConn.Should().Contain("Host=vps.hostinger.internal");
    }

    [Fact]
    public void TenantContext_QuandoDefault_DeveUtilizarDatabasePadrao()
    {
        var tenantContext = new TenantContext("cotu");
        tenantContext.SetTenant("default", "localhost");

        tenantContext.DatabaseName.Should().Be("cotu");

        var baseConn = "Host=localhost;Port=5432;Database=cotu;Username=postgres;Password=123";
        var tenantConn = tenantContext.BuildConnectionString(baseConn);

        tenantConn.Should().Contain("Database=cotu");
    }
}

