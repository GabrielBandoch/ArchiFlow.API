namespace ArchiFlow.Domain.Shared;




public interface ITenantContext
{
    string TenantId { get; }
    string Host { get; }
    bool IsResolved { get; }
    string DatabaseName { get; }
    string BuildConnectionString(string baseConnectionString);
    void SetTenant(string tenantId, string host);
}

