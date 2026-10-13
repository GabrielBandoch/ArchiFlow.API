using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Domain.Assinaturas;

public interface ITenantAssinaturaRepository
{
    Task<TenantAssinatura?> ObterPorIdAsync(Guid id);
    Task<TenantAssinatura?> ObterPorSubdominioAsync(string subdominio);
    Task<IEnumerable<TenantAssinatura>> ListarTodosAsync(string? termoBusca = null, string? statusFiltro = null);
    Task AdicionarAsync(TenantAssinatura assinatura);
    Task AtualizarAsync(TenantAssinatura assinatura);
    Task RemoverAsync(TenantAssinatura assinatura);
    Task<bool> SubdominioExisteAsync(string subdominio, Guid? ignorarId = null);
}
