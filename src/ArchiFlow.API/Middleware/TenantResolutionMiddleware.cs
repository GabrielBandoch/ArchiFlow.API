using ArchiFlow.Domain.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace ArchiFlow.API.Middleware;

/// <summary>
/// Middleware responsável por interceptar as requisições HTTP e resolver o Tenant (escritório de arquitetura)
/// a partir do DNS (Host HTTP) ou do cabeçalho customizado X-Tenant-Id.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;
    public const string TenantHeaderName = "X-Tenant-Id";
    public const string TenantResolvedHeaderName = "X-Tenant-Resolved";

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var tenantId = ResolveTenant(context);
        var host = context.Request.Host.Host;

        tenantContext.SetTenant(tenantId, host);

        context.Response.Headers[TenantResolvedHeaderName] = tenantContext.TenantId;
        _logger.LogInformation("Tenant '{TenantId}' resolvido para requisição (Host: '{Host}', Banco: '{Database}')",
            tenantContext.TenantId, host, tenantContext.DatabaseName);

        await _next(context);
    }

    public static string ResolveTenant(HttpContext context)
    {
        // 1. Prioridade: cabeçalho explícito (útil para desenvolvimento, testes e integrações)
        if (context.Request.Headers.TryGetValue(TenantHeaderName, out var headerValues))
        {
            var headerTenant = headerValues.FirstOrDefault()?.Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(headerTenant))
            {
                return SanitizeTenant(headerTenant);
            }
        }

        // 2. DNS / Hostname da requisição
        var host = context.Request.Host.Host;
        if (string.IsNullOrWhiteSpace(host) || 
            host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || 
            host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase))
        {
            var envDefault = Environment.GetEnvironmentVariable("DEFAULT_TENANT");
            return !string.IsNullOrWhiteSpace(envDefault) ? SanitizeTenant(envDefault) : "default";
        }

        // Suporte a subdomínios (ex: duna.archiflow.com.br, alfa.escritorio.com.br)
        var parts = host.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 2)
        {
            var subdominio = parts[0];
            if (!subdominio.Equals("www", StringComparison.OrdinalIgnoreCase) && 
                !subdominio.Equals("api", StringComparison.OrdinalIgnoreCase) && 
                !subdominio.Equals("app", StringComparison.OrdinalIgnoreCase))
            {
                return SanitizeTenant(subdominio);
            }

            if (parts.Length > 3 && (subdominio.Equals("api", StringComparison.OrdinalIgnoreCase) || subdominio.Equals("app", StringComparison.OrdinalIgnoreCase)))
            {
                // Formato api.duna.archiflow.com.br
                return SanitizeTenant(parts[1]);
            }
        }
        else if (parts.Length == 2)
        {
            // Domínio customizado exclusivo do escritório (ex: dunaarquitetura.com -> dunaarquitetura)
            return SanitizeTenant(parts[0]);
        }

        return "default";
    }

    private static string SanitizeTenant(string input)
    {
        var sanitized = Regex.Replace(input, @"[^a-zA-Z0-9_-]", "");
        return string.IsNullOrWhiteSpace(sanitized) ? "default" : sanitized.ToLowerInvariant();
    }
}
