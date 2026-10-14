using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Domain.MultiTenancy;

public interface ITenantAssinaturaRepository
{
    Task<IEnumerable<TenantAssinatura>> ListarTodosAsync(string? busca = null, string? status = null);
    Task<TenantAssinatura?> ObterPorIdAsync(Guid id);
    Task<TenantAssinatura?> ObterPorSubdominioAsync(string subdominio);
    Task AdicionarAsync(TenantAssinatura tenantAssinatura);
    Task AtualizarAsync(TenantAssinatura tenantAssinatura);
}
