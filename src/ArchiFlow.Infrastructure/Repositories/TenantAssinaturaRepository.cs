using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchiFlow.Domain.Assinaturas;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArchiFlow.Infrastructure.Repositories;

public class TenantAssinaturaRepository : ITenantAssinaturaRepository
{
    private readonly ArchiFlowDbContext _context;

    public TenantAssinaturaRepository(ArchiFlowDbContext context)
    {
        _context = context;
    }

    public async Task<TenantAssinatura?> ObterPorIdAsync(Guid id)
    {
        return await _context.TenantsAssinaturas.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TenantAssinatura?> ObterPorSubdominioAsync(string subdominio)
    {
        var clean = subdominio.Trim().ToLowerInvariant();
        return await _context.TenantsAssinaturas
            .FirstOrDefaultAsync(t => t.Subdominio.ToLower() == clean);
    }

    public async Task<IEnumerable<TenantAssinatura>> ListarTodosAsync(string? termoBusca = null, string? statusFiltro = null)
    {
        var query = _context.TenantsAssinaturas.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(termoBusca))
        {
            var termo = termoBusca.Trim().ToLower();
            query = query.Where(t =>
                t.NomeEscritorio.ToLower().Contains(termo) ||
                t.Subdominio.ToLower().Contains(termo) ||
                t.NomeResponsavel.ToLower().Contains(termo) ||
                t.EmailResponsavel.ToLower().Contains(termo));
        }

        if (!string.IsNullOrWhiteSpace(statusFiltro) && !statusFiltro.Equals("Todos", StringComparison.OrdinalIgnoreCase))
        {
            var status = statusFiltro.Trim();
            query = query.Where(t => t.StatusFinanceiro.ToLower() == status.ToLower());
        }

        return await query.OrderByDescending(t => t.CriadoEm).ToListAsync();
    }

    public async Task AdicionarAsync(TenantAssinatura assinatura)
    {
        await _context.TenantsAssinaturas.AddAsync(assinatura);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(TenantAssinatura assinatura)
    {
        assinatura.AtualizadoEm = DateTime.UtcNow;
        _context.TenantsAssinaturas.Update(assinatura);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(TenantAssinatura assinatura)
    {
        _context.TenantsAssinaturas.Remove(assinatura);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> SubdominioExisteAsync(string subdominio, Guid? ignorarId = null)
    {
        var clean = subdominio.Trim().ToLowerInvariant();
        var query = _context.TenantsAssinaturas.Where(t => t.Subdominio.ToLower() == clean);
        if (ignorarId.HasValue)
        {
            query = query.Where(t => t.Id != ignorarId.Value);
        }
        return await query.AnyAsync();
    }
}
