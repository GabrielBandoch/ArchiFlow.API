using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchiFlow.Domain.MultiTenancy;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArchiFlow.Infrastructure.Repositories.MultiTenancy;

public class TenantAssinaturaRepository : ITenantAssinaturaRepository
{
    private readonly ArchiFlowDbContext _context;

    public TenantAssinaturaRepository(ArchiFlowDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TenantAssinatura>> ListarTodosAsync(string? busca = null, string? status = null)
    {
        var query = _context.TenantsAssinaturas.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToLower();
            query = query.Where(t => t.NomeEscritorio.ToLower().Contains(termo)
                                  || t.Subdominio.ToLower().Contains(termo)
                                  || t.EmailResponsavel.ToLower().Contains(termo));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(t => t.StatusFinanceiro.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        return await query.OrderByDescending(t => t.CriadoEm).ToListAsync();
    }

    public async Task<TenantAssinatura?> ObterPorIdAsync(Guid id)
    {
        return await _context.TenantsAssinaturas.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TenantAssinatura?> ObterPorSubdominioAsync(string subdominio)
    {
        return await _context.TenantsAssinaturas.FirstOrDefaultAsync(t => t.Subdominio.ToLower() == subdominio.Trim().ToLower());
    }

    public async Task AdicionarAsync(TenantAssinatura tenantAssinatura)
    {
        await _context.TenantsAssinaturas.AddAsync(tenantAssinatura);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(TenantAssinatura tenantAssinatura)
    {
        _context.TenantsAssinaturas.Update(tenantAssinatura);
        await _context.SaveChangesAsync();
    }
}
