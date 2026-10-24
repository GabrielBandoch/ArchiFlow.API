using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ArchiFlow.Domain.Usuarios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ArchiFlow.API.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class RequireModuleAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _modulo;

    public RequireModuleAttribute(string modulo)
    {
        _modulo = modulo;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userType = user.FindFirst("user_type")?.Value;
        if (userType == "client" || user.IsInRole("Cliente"))
        {
            context.Result = new ObjectResult(new { mensagem = "Acesso negado: o perfil do cliente não possui acesso a recursos internos da equipe." })
            {
                StatusCode = 403
            };
            return;
        }

        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role == Roles.Administrador || role == Roles.ArquitetoAdmin || role == "AdministradorGeral")
        {
            await next();
            return;
        }

        var modulosClaim = user.FindFirst("modulos")?.Value;
        if (string.IsNullOrWhiteSpace(modulosClaim))
        {
            context.Result = new ObjectResult(new { mensagem = $"Acesso negado ao módulo '{_modulo}'." })
            {
                StatusCode = 403
            };
            return;
        }

        var allowedModules = modulosClaim
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!allowedModules.Contains(_modulo))
        {
            context.Result = new ObjectResult(new { mensagem = $"Acesso negado ao módulo '{_modulo}'." })
            {
                StatusCode = 403
            };
            return;
        }

        await next();
    }
}

