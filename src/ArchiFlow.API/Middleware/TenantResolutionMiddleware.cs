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
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userTenantResult = ResolverTenantUsuario(context);
            if (!userTenantResult.Sucesso)
            {
                context.Response.StatusCode = userTenantResult.StatusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync($"{{\"error\":\"{userTenantResult.MensagemErro}\"}}", context.RequestAborted);
                return;
            }

            var authTenant = userTenantResult.Tenant!;

            if (context.Request.Headers.TryGetValue(TenantHeaderName, out var headerValues))
            {
                var headerTenant = headerValues.FirstOrDefault()?.Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(headerTenant) && !string.Equals(headerTenant, authTenant, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Tentativa de spoofing de tenant por cabecalho rejeitada. Autenticado: {AuthTenant}, Solicitado: {ReqTenant}", authTenant, headerTenant);
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\":\"Acesso negado: o tenant informado no cabeçalho não corresponde ao usuário autenticado.\"}", context.RequestAborted);
                    return;
                }
            }

            var hostTenant = ResolveTenantFromHost(context.Request.Host.Host);
            if (hostTenant == null)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\":\"Tenant inválido solicitado pelo host.\"}", context.RequestAborted);
                return;
            }

            if (!hostTenant.Equals(DefaultTenant, StringComparison.OrdinalIgnoreCase) &&
                !hostTenant.Equals(authTenant, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Divergencia entre tenant do token e host da requisicao. Token: {TokenTenant}, HostTenant: {HostTenant}", authTenant, hostTenant);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync($"{{\"error\":\"Acesso negado: o token fornecido pertence ao tenant '{authTenant}' e não tem permissão para acessar '{hostTenant}'.\"}}", context.RequestAborted);
                return;
            }

            await AplicarTenantAsync(context, tenantContext, authTenant);
            await _next(context);
            return;
        }

        var isDevOrTest = _environment == null || _environment.IsDevelopment() || _environment.EnvironmentName == "Testing";
        if (isDevOrTest && context.Request.Headers.TryGetValue(TenantHeaderName, out var devHeaderValues))
        {
            var rawHeader = devHeaderValues.FirstOrDefault()?.Trim();
            if (!string.IsNullOrWhiteSpace(rawHeader))
            {
                if (!IsValidTenantIdentifier(rawHeader))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\":\"Tenant informado no cabeçalho possui formato inválido.\"}", context.RequestAborted);
                    return;
                }

                var devTenant = SanitizeTenant(rawHeader);
                await AplicarTenantAsync(context, tenantContext, devTenant);
                await _next(context);
                return;
            }
        }

        var requestedTenant = ResolveTenantFromHost(context.Request.Host.Host);
        if (requestedTenant == null)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\":\"Tenant inválido solicitado pelo host.\"}", context.RequestAborted);
            return;
        }

        await AplicarTenantAsync(context, tenantContext, requestedTenant);
        await _next(context);
    }

    private static (bool Sucesso, int StatusCode, string? MensagemErro, string? Tenant) ResolverTenantUsuario(HttpContext context)
    {
        var claimTenant = context.User.FindFirst("tenant")?.Value;
        if (!string.IsNullOrWhiteSpace(claimTenant))
        {
            if (!IsValidTenantIdentifier(claimTenant))
            {
                return (false, StatusCodes.Status401Unauthorized, "Token com claim de tenant inválida.", null);
            }
            return (true, 0, null, SanitizeTenant(claimTenant));
        }

        var claimEscritorioId = context.User.FindFirst("escritorio_id")?.Value
                             ?? context.User.FindFirst("EscritorioId")?.Value;

        if (!string.IsNullOrWhiteSpace(claimEscritorioId) && !Guid.TryParse(claimEscritorioId, out _) && IsValidTenantIdentifier(claimEscritorioId))
        {
            return (true, 0, null, SanitizeTenant(claimEscritorioId));
        }

        return (false, StatusCodes.Status401Unauthorized, "Token inválido: claim de tenant não encontrada.", null);
    }

    private static async Task AplicarTenantAsync(HttpContext context, ITenantContext tenantContext, string tenant)
    {
        tenantContext.SetTenant(tenant, context.Request.Host.Host);
        context.Response.Headers[TenantResolvedHeaderName] = tenantContext.TenantId;

        if (context.RequestServices != null)
        {
            var provisioner = context.RequestServices.GetService(typeof(ITenantDatabaseProvisioner)) as ITenantDatabaseProvisioner;
            if (provisioner != null)
            {
                await provisioner.EnsureTenantDatabaseAsync(tenantContext.TenantId);
            }
        }
    }

    public static string ResolveTenant(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userResult = ResolverTenantUsuario(context);
            if (userResult.Sucesso)
            {
                return userResult.Tenant!;
            }
        }

        if (context.Request.Headers.TryGetValue(TenantHeaderName, out var headerValues))
        {
            var headerTenant = headerValues.FirstOrDefault()?.Trim();
            if (!string.IsNullOrWhiteSpace(headerTenant) && IsValidTenantIdentifier(headerTenant))
            {
                return SanitizeTenant(headerTenant);
            }
        }

        return ResolveTenantFromHost(context.Request.Host.Host) ?? DefaultTenant;
    }

    public static string? ResolveTenantFromHost(string? host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return ObterDefaultTenant();
        }

        var hostSemPorta = host.Split(':')[0].Trim().ToLowerInvariant();

        if (IsLocalHost(hostSemPorta))
        {
            return ObterDefaultTenant();
        }

        var parts = hostSemPorta.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 2)
        {
            var subdominio = parts[0];
            if (!IsPrefixoInfra(subdominio))
            {
                return IsValidTenantIdentifier(subdominio) ? SanitizeTenant(subdominio) : null;
            }

            if (parts.Length > 3 && (subdominio.Equals("api", StringComparison.OrdinalIgnoreCase) || subdominio.Equals("app", StringComparison.OrdinalIgnoreCase)))
            {
                var candidate = parts[1];
                if (!candidate.Equals("archiflow", StringComparison.OrdinalIgnoreCase) && !candidate.Equals("cotu", StringComparison.OrdinalIgnoreCase))
                {
                    return IsValidTenantIdentifier(candidate) ? SanitizeTenant(candidate) : null;
                }
            }

            return DefaultTenant;
        }

        if (parts.Length == 2)
        {
            return IsValidTenantIdentifier(parts[0]) ? SanitizeTenant(parts[0]) : null;
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

    public static bool IsValidTenantIdentifier(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        if (Guid.TryParse(input, out _))
        {
            return false;
        }

        return Regex.IsMatch(input, @"^[a-zA-Z0-9][a-zA-Z0-9_-]{0,62}$", RegexOptions.None, TimeSpan.FromSeconds(1));
    }

    public static string SanitizeTenant(string input)
    {
        var sanitized = Regex.Replace(input, @"[^a-zA-Z0-9_-]", "", RegexOptions.None, TimeSpan.FromSeconds(1));
        return string.IsNullOrWhiteSpace(sanitized) ? DefaultTenant : sanitized.ToLowerInvariant();
    }
}
