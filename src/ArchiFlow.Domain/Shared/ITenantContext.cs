namespace ArchiFlow.Domain.Shared;

/// <summary>
/// Contrato do contexto de multi-tenancy para identificação do escritório cliente em runtime.
/// </summary>
public interface ITenantContext
{
    string TenantId { get; }
    string Host { get; }
    bool IsResolved { get; }
    string DatabaseName { get; }
    string BuildConnectionString(string baseConnectionString);
    void SetTenant(string tenantId, string host);
}
