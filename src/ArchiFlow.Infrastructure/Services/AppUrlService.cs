using System;
using System.Collections.Generic;
using System.Linq;
using ArchiFlow.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace ArchiFlow.Infrastructure.Services;

public class AppUrlService : IAppUrlService
{
    private static readonly string FallbackScheme = Uri.UriSchemeHttps;
    private static readonly string FallbackHost = "cotu.com.br";
    private readonly HashSet<string> _allowedOrigins;
    private readonly string _defaultUrl;

    public AppUrlService(IConfiguration configuration)
    {
        var defaultLocalOrigins = $"{Uri.UriSchemeHttp}{Uri.SchemeDelimiter}localhost:4200,{Uri.UriSchemeHttp}{Uri.SchemeDelimiter}localhost:3000";
        var rawAllowed = configuration.GetValue<string>("AllowedOrigins")
            ?? configuration.GetValue<string>("CORS:AllowedOrigins")
            ?? defaultLocalOrigins;

        _allowedOrigins = new HashSet<string>(
            rawAllowed
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(o => o.TrimEnd('/')),
            StringComparer.OrdinalIgnoreCase);

        var configuredAppUrl = configuration["AppUrl"]
            ?? configuration["FrontendUrl"]
            ?? configuration["APP_URL"]
            ?? _allowedOrigins.FirstOrDefault()
            ?? $"{FallbackScheme}{Uri.SchemeDelimiter}{FallbackHost}";

        _defaultUrl = configuredAppUrl.TrimEnd('/');
    }

    public string ObterUrlBaseConfiavel(string? requestedOrigin = null)
    {
        if (!string.IsNullOrWhiteSpace(requestedOrigin) &&
            Uri.TryCreate(requestedOrigin.Trim(), UriKind.Absolute, out var uri))
        {
            var normalizedOrigin = $"{uri.Scheme}{Uri.SchemeDelimiter}{uri.Authority}".TrimEnd('/');
            if (_allowedOrigins.Contains(normalizedOrigin))
            {
                return normalizedOrigin;
            }
        }

        return _defaultUrl;
    }
}

