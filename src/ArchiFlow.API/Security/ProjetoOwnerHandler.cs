using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArchiFlow.API.Security;

public class ProjetoOwnerRequirement : IAuthorizationRequirement { }

public class ProjetoOwnerHandler : AuthorizationHandler<ProjetoOwnerRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProjetoOwnerHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ProjetoOwnerRequirement requirement)
    {
        var user = context.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        var userType = user.FindFirst("user_type")?.Value;
        if (userType == "staff")
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if ((userType == "client" || user.IsInRole("Cliente")) && HasAccessToProject(user))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private bool HasAccessToProject(ClaimsPrincipal user)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return false;
        }

        var routeData = httpContext.GetRouteData();
        if (!routeData.Values.TryGetValue("projetoId", out var routeIdObj) && !routeData.Values.TryGetValue("id", out routeIdObj))
        {
            return false;
        }

        if (routeIdObj == null || !Guid.TryParse(routeIdObj.ToString(), out var routeId))
        {
            return false;
        }

        var claimProjetoId = user.FindFirst("projeto_id")?.Value;
        return Guid.TryParse(claimProjetoId, out var userProjetoId) && userProjetoId == routeId;
    }
}
