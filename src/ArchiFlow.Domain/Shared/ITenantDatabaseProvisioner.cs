namespace ArchiFlow.Domain.Shared;

public interface ITenantDatabaseProvisioner
{
    Task EnsureTenantDatabaseAsync(string tenantId);
}
