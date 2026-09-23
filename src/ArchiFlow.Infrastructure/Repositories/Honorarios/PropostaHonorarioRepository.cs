using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Repositories.Honorarios;

public class PropostaHonorarioRepository : Repository<PropostaHonorario>, IPropostaHonorarioRepository
{
    public PropostaHonorarioRepository(ArchiFlowDbContext context) : base(context) { }

    public async Task<PropostaHonorario?> GetByIdWithItens(Guid id)
    {
        return await _dbSet
            .Include(p => p.ItensEtapa.OrderBy(i => i.Ordem))
            .Include(p => p.Cliente)
            .Include(p => p.Lead)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<PropostaHonorario>> GetAllWithItens()
    {
        return await _dbSet
            .Include(p => p.ItensEtapa.OrderBy(i => i.Ordem))
            .Include(p => p.Cliente)
            .Include(p => p.Lead)
            .OrderByDescending(p => p.CriadoEm)
            .ToListAsync();
    }

    public async Task<IEnumerable<PropostaHonorario>> GetByClienteId(Guid clienteId)
    {
        return await _dbSet
            .Include(p => p.ItensEtapa.OrderBy(i => i.Ordem))
            .Where(p => p.ClienteId == clienteId)
            .OrderByDescending(p => p.CriadoEm)
            .ToListAsync();
    }

    public async Task<IEnumerable<PropostaHonorario>> GetByLeadId(Guid leadId)
    {
        return await _dbSet
            .Include(p => p.ItensEtapa.OrderBy(i => i.Ordem))
            .Where(p => p.LeadId == leadId)
            .OrderByDescending(p => p.CriadoEm)
            .ToListAsync();
    }

    public async Task<string> GerarProximoCodigo()
    {
        var ano = DateTime.UtcNow.Year;
        var prefixo = $"PROP-{ano}-";

        var total = await _dbSet
            .Where(p => p.Codigo.StartsWith(prefixo))
            .CountAsync();

        return $"{prefixo}{(total + 1):D4}";
    }
}
