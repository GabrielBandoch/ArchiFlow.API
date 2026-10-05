using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ArchiFlow.Domain.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ArchiFlow.API.Middleware;

/// <summary>
/// Middleware responsável por interceptar requisições HTTP e resolver o Tenant seguro
/// a partir da identidade autenticada do usuário (EscritorioId) ou DNS/Host HTTP válido.
/// Impede que clientes autenticados manipulem o cabeçalho X-Tenant-Id para trocar de tenant.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;
    private readonly IWebHostEnvironment? _environment;
    public const string TenantHeaderName = "X-Tenant-Id";
    public const string TenantResolvedHeaderName = "X-Tenant-Resolved";
    public const string DefaultTenant = "default";

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger,
        IWebHostEnvironment? environment = null)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // 1. Usuário autenticado: a fonte de verdade confiável é a claim do escritório do usuário
        if (await ProcessarUsuarioAutenticadoAsync(context, tenantContext))
        {
            return;
        }

        // 2. Requisição não autenticada: em ambiente de desenvolvimento/teste, aceita o cabeçalho para testes locais
        if (await ProcessarDevHeaderAsync(context, tenantContext))
        {
            return;
        }

        // 3. Resolução segura via Host / Subdomínio DNS
        AplicarTenant(context, tenantContext, ResolveTenantFromHost(context.Request.Host.Host));
        await _next(context);
    }

    private async Task<bool> ProcessarUsuarioAutenticadoAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var claimEscritorioId = context.User.FindFirst("escritorio_id")?.Value
                             ?? context.User.FindFirst("EscritorioId")?.Value;

        if (string.IsNullOrWhiteSpace(claimEscritorioId))
        {
            return false;
        }

        var authenticatedTenant = SanitizeTenant(claimEscritorioId);

        if (context.Request.Headers.TryGetValue(TenantHeaderName, out var headerValues))
        {
            var requestedTenant = SanitizeTenant(headerValues.FirstOrDefault() ?? string.Empty);
            if (!string.Equals(requestedTenant, authenticatedTenant, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Tentativa de violação de tenant rejeitada. Usuário autenticado pertence ao escritório '{AuthTenant}', mas enviou X-Tenant-Id '{ReqTenant}'",
                    authenticatedTenant, requestedTenant);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\":\"Acesso negado: o tenant informado não corresponde ao escritório do usuário autenticado.\"}", context.RequestAborted);
                return true;
            }
        }

        AplicarTenant(context, tenantContext, authenticatedTenant);
        await _next(context);
        return true;
    }

    private async Task<bool> ProcessarDevHeaderAsync(HttpContext context, ITenantContext tenantContext)
    {
        var isDevOrTest = _environment == null || _environment.IsDevelopment() || _environment.EnvironmentName == "Testing";
        if (isDevOrTest && context.Request.Headers.TryGetValue(TenantHeaderName, out var devHeaderValues))
        {
            var devTenant = SanitizeTenant(devHeaderValues.FirstOrDefault() ?? string.Empty);
            if (!string.IsNullOrWhiteSpace(devTenant) && !devTenant.Equals(DefaultTenant, StringComparison.OrdinalIgnoreCase))
            {
                AplicarTenant(context, tenantContext, devTenant);
                await _next(context);
                return true;
            }
        }

        return false;
    }

    private static void AplicarTenant(HttpContext context, ITenantContext tenantContext, string tenant)
    {
        tenantContext.SetTenant(tenant, context.Request.Host.Host);
        context.Response.Headers[TenantResolvedHeaderName] = tenantContext.TenantId;
    }

    public static string ResolveTenant(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var claimEscritorioId = context.User.FindFirst("escritorio_id")?.Value
                                 ?? context.User.FindFirst("EscritorioId")?.Value;
            if (!string.IsNullOrWhiteSpace(claimEscritorioId))
            {
                return SanitizeTenant(claimEscritorioId);
            }
        }

        if (context.Request.Headers.TryGetValue(TenantHeaderName, out var headerValues))
        {
            var headerTenant = headerValues.FirstOrDefault()?.Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(headerTenant))
            {
                return SanitizeTenant(headerTenant);
            }
        }

        return ResolveTenantFromHost(context.Request.Host.Host);
    }

    public static string ResolveTenantFromHost(string? host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return ObterDefaultTenant();

        var hostSemPorta = host.Split(':')[0].Trim();

        if (IsLocalHost(hostSemPorta))
            return ObterDefaultTenant();

        var parts = hostSemPorta.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 2)
        {
            var subdominio = parts[0];
            if (!IsPrefixoInfra(subdominio))
                return SanitizeTenant(subdominio);

            if (parts.Length > 3 && (subdominio.Equals("api", StringComparison.OrdinalIgnoreCase) || subdominio.Equals("app", StringComparison.OrdinalIgnoreCase)))
            {
                var candidate = parts[1];
                if (!candidate.Equals("archiflow", StringComparison.OrdinalIgnoreCase))
                    return SanitizeTenant(candidate);
            }
        }
        else if (parts.Length == 2)
        {
            return SanitizeTenant(parts[0]);
        }

        return DefaultTenant;
    }

    private static bool IsLocalHost(string host) =>
        host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
        host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);

    private static bool IsPrefixoInfra(string subdominio) =>
        subdominio.Equals("www", StringComparison.OrdinalIgnoreCase) ||
        subdominio.Equals("api", StringComparison.OrdinalIgnoreCase) ||
        subdominio.Equals("app", StringComparison.OrdinalIgnoreCase);

    private static string ObterDefaultTenant()
    {
        var envDefault = Environment.GetEnvironmentVariable("DEFAULT_TENANT");
        return !string.IsNullOrWhiteSpace(envDefault) ? SanitizeTenant(envDefault) : DefaultTenant;
    }

    public static string SanitizeTenant(string input)
    {
        var sanitized = Regex.Replace(input, @"[^a-zA-Z0-9_-]", "", RegexOptions.None, TimeSpan.FromSeconds(1));
        return string.IsNullOrWhiteSpace(sanitized) ? DefaultTenant : sanitized.ToLowerInvariant();
    }
}
