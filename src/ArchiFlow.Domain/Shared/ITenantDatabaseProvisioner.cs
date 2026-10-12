using System.Threading.Tasks;

namespace ArchiFlow.Domain.Shared;

/// <summary>
/// Contrato responsável por provisionar, criar banco de dados e aplicar migrações/seeds
/// de forma dinâmica por tenant/escritório (Estratégia 1 banco de dados por cliente).
/// </summary>
public interface ITenantDatabaseProvisioner
{
    /// <summary>
    /// Garante que o banco de dados do tenant especificado existe no PostgreSQL e está atualizado
    /// com as últimas migrações e seeds estruturais.
    /// </summary>
    Task EnsureTenantDatabaseAsync(string tenantId);
}
