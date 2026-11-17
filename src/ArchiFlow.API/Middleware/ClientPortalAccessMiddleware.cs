using System;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using ArchiFlow.Domain.Clientes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiFlow.API.Middleware;

public class ClientPortalAccessMiddleware
{
    private readonly RequestDelegate _next;

    public ClientPortalAccessMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var userType = user.FindFirst("user_type")?.Value;
            if (string.Equals(userType, "client", StringComparison.OrdinalIgnoreCase) || user.IsInRole("Cliente"))
            {
                var clienteIdClaim = user.FindFirst("cliente_id")?.Value;
                if (Guid.TryParse(clienteIdClaim, out var clienteId))
                {
                    var clienteRepo = context.RequestServices.GetService<IClienteRepository>();
                    if (clienteRepo != null)
                    {
                        var cliente = await clienteRepo.GetById(clienteId);
                        if (cliente == null || !cliente.Ativo)
                        {
                            await WriteUnauthorizedAsync(context, "O acesso ao portal está suspenso. Entre em contato com o escritório.");
                            return;
                        }
                    }
                }

                var repIdClaim = user.FindFirst("representative_id")?.Value;
                if (Guid.TryParse(repIdClaim, out var repId))
                {
                    var repRepo = context.RequestServices.GetService<IRepresentanteClienteRepository>();
                    if (repRepo != null)
                    {
                        var rep = await repRepo.ObterPorIdAsync(repId);
                        if (rep == null || !rep.Ativo)
                        {
                            await WriteUnauthorizedAsync(context, "Sua conta de acesso está inativa. Entre em contato com o escritório.");
                            return;
                        }
                    }
                }
            }
        }

        await _next(context);
    }

    private static async Task WriteUnauthorizedAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }), context.RequestAborted);
    }
}
