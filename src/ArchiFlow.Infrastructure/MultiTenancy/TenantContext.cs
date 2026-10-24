using ArchiFlow.Domain.Shared;
using Npgsql;

namespace ArchiFlow.Infrastructure.MultiTenancy;

public class TenantContext : ITenantContext
{
    private const string DefaultTenant = "default";

    private string _tenantId = DefaultTenant;
    private string _host = "localhost";
    private bool _isResolved = false;
    private readonly string? _configuredBaseDb;

    public TenantContext(string? configuredBaseDb = null)
    {
        _configuredBaseDb = configuredBaseDb;
    }

    public string TenantId => _tenantId;
    public string Host => _host;
    public bool IsResolved => _isResolved;

    public string DatabaseName => string.IsNullOrWhiteSpace(_tenantId) || _tenantId.Equals(DefaultTenant, StringComparison.OrdinalIgnoreCase)
        ? ObterBaseDatabaseName()
        : $"{ObterBaseDatabaseName()}_{_tenantId.ToLowerInvariant().Replace("-", "_")}";

    private string ObterBaseDatabaseName()
    {
        if (!string.IsNullOrWhiteSpace(_configuredBaseDb))
        {
            return _configuredBaseDb.Trim().ToLowerInvariant();
        }

        var envDb = Environment.GetEnvironmentVariable("DB_NAME");
        return !string.IsNullOrWhiteSpace(envDb) ? envDb.Trim().ToLowerInvariant() : "cotu";
    }

    public void SetTenant(string tenantId, string host)
    {
        _tenantId = string.IsNullOrWhiteSpace(tenantId) ? DefaultTenant : tenantId.Trim().ToLowerInvariant();
        _host = host ?? string.Empty;
        _isResolved = true;
    }

    public string BuildConnectionString(string baseConnectionString)
    {
        if (string.IsNullOrWhiteSpace(baseConnectionString))
        {
            return string.Empty;
        }

        var builder = new NpgsqlConnectionStringBuilder(baseConnectionString);
        var baseDb = !string.IsNullOrWhiteSpace(builder.Database)
            ? builder.Database.Trim().ToLowerInvariant()
            : ObterBaseDatabaseName();

        builder.Database = string.IsNullOrWhiteSpace(_tenantId) || _tenantId.Equals(DefaultTenant, StringComparison.OrdinalIgnoreCase)
            ? baseDb
            : $"{baseDb}_{_tenantId.ToLowerInvariant().Replace("-", "_")}";

        return builder.ConnectionString;
    }
}

