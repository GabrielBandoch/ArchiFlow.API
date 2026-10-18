using System;
using System.Linq;
using ArchiFlow.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace ArchiFlow.Infrastructure.Services;

public class AppUrlService : IAppUrlService
{
    private readonly string[] _allowedOrigins;
    private readonly string _defaultUrl;

    public AppUrlService(IConfiguration configuration)
    {
        var rawAllowed = configuration.GetValue<string>("AllowedOrigins")
            ?? configuration.GetValue<string>("CORS:AllowedOrigins")
            ?? "http://localhost:4200,http://localhost:3000";

        _allowedOrigins = rawAllowed
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(o => o.TrimEnd('/').ToLowerInvariant())
            .ToArray();

        var configuredAppUrl = configuration["AppUrl"]
            ?? configuration["FrontendUrl"]
            ?? configuration["APP_URL"]
            ?? _allowedOrigins.FirstOrDefault()
            ?? "https://cotu.com.br";

        _defaultUrl = configuredAppUrl.TrimEnd('/');
    }

    public string ObterUrlBaseConfiavel(string? requestedOrigin = null)
    {
        if (!string.IsNullOrWhiteSpace(requestedOrigin) &&
            Uri.TryCreate(requestedOrigin.Trim(), UriKind.Absolute, out var uri))
        {
            var normalizedOrigin = $"{uri.Scheme}://{uri.Authority}".ToLowerInvariant().TrimEnd('/');
            if (_allowedOrigins.Contains(normalizedOrigin))
            {
                return normalizedOrigin;
            }
        }

        return _defaultUrl;
    }
}
