using ArchiFlow.Domain.Shared;
using Npgsql;

namespace ArchiFlow.Infrastructure.MultiTenancy;

/// <summary>
/// Implementação escopada por requisição do contexto de tenant.
/// Garante isolamento seguro de dados conectando a API ao banco de dados específico do escritório.
/// </summary>
public class TenantContext : ITenantContext
{
    private string _tenantId = "default";
    private string _host = "localhost";
    private bool _isResolved = false;

    public string TenantId => _tenantId;
    public string Host => _host;
    public bool IsResolved => _isResolved;

    public string DatabaseName => string.IsNullOrWhiteSpace(_tenantId) || _tenantId.Equals("default", StringComparison.OrdinalIgnoreCase)
        ? "archiflow"
        : $"archiflow_{_tenantId.ToLowerInvariant().Replace("-", "_")}";

    public void SetTenant(string tenantId, string host)
    {
        _tenantId = string.IsNullOrWhiteSpace(tenantId) ? "default" : tenantId.Trim().ToLowerInvariant();
        _host = host ?? string.Empty;
        _isResolved = true;
    }

    public string BuildConnectionString(string baseConnectionString)
    {
        if (string.IsNullOrWhiteSpace(baseConnectionString))
            return string.Empty;

        var builder = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Database = DatabaseName
        };

        return builder.ConnectionString;
    }
}
