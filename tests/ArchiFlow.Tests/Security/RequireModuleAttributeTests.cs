using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ArchiFlow.API.Security;
using ArchiFlow.Domain.Usuarios;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace ArchiFlow.Tests.Security;

public class RequireModuleAttributeTests
{
    private static (ActionExecutingContext Context, ActionExecutionDelegate Next, bool Executed) CreateFilterContext(ClaimsPrincipal user)
    {
        var httpContext = new DefaultHttpContext { User = user };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());

        var executed = false;
        ActionExecutionDelegate next = () =>
        {
            executed = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object()));
        };

        return (context, next, executed);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenUserNotAuthenticated_ReturnsUnauthorized()
    {
        var attribute = new RequireModuleAttribute(ModulosApp.Projetos);
        var unauthUser = new ClaimsPrincipal(new ClaimsIdentity());
        var (context, next, _) = CreateFilterContext(unauthUser);

        await attribute.OnActionExecutionAsync(context, next);

        context.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenClientUserTriesToAccess_ReturnsForbidden()
    {
        var attribute = new RequireModuleAttribute(ModulosApp.Projetos);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Cliente"),
            new Claim("user_type", "client")
        };
        var clientUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var (context, next, _) = CreateFilterContext(clientUser);

        await attribute.OnActionExecutionAsync(context, next);

        var result = context.Result as ObjectResult;
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenAdminUser_AllowsExecution()
    {
        var attribute = new RequireModuleAttribute(ModulosApp.Financeiro);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, Roles.Administrador),
            new Claim("user_type", "staff")
        };
        var adminUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var executed = false;
        var httpContext = new DefaultHttpContext { User = adminUser };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());

        ActionExecutionDelegate next = () =>
        {
            executed = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object()));
        };

        await attribute.OnActionExecutionAsync(context, next);

        context.Result.Should().BeNull();
        executed.Should().BeTrue();
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenStaffHasModulePermission_AllowsExecution()
    {
        var attribute = new RequireModuleAttribute(ModulosApp.Projetos);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, Roles.Colaborador),
            new Claim("user_type", "staff"),
            new Claim("modulos", "projetos,agenda")
        };
        var staffUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var executed = false;
        var httpContext = new DefaultHttpContext { User = staffUser };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());

        ActionExecutionDelegate next = () =>
        {
            executed = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object()));
        };

        await attribute.OnActionExecutionAsync(context, next);

        context.Result.Should().BeNull();
        executed.Should().BeTrue();
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenStaffLacksModulePermission_ReturnsForbidden()
    {
        var attribute = new RequireModuleAttribute(ModulosApp.Financeiro);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, Roles.Colaborador),
            new Claim("user_type", "staff"),
            new Claim("modulos", "projetos,agenda")
        };
        var staffUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var (context, next, _) = CreateFilterContext(staffUser);

        await attribute.OnActionExecutionAsync(context, next);

        var result = context.Result as ObjectResult;
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(403);
    }
}

