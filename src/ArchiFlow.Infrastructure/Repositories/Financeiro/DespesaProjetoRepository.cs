using ArchiFlow.Domain.Financeiro;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Repositories.Financeiro;

public class DespesaProjetoRepository : Repository<DespesaProjeto>, IDespesaProjetoRepository
{
    public DespesaProjetoRepository(ArchiFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<DespesaProjeto>> ObterPorProjetoIdAsync(Guid projetoId)
    {
        return await _dbSet
            .Include(d => d.Projeto)
            .Where(d => d.ProjetoId == projetoId)
            .OrderByDescending(d => d.DataDespesa)
            .ToListAsync();
    }

    public async Task<IEnumerable<DespesaProjeto>> ObterComFiltroAsync(Guid? projetoId, CategoriaDespesa? categoria, DateTime? dataInicio, DateTime? dataFim)
    {
        var query = _dbSet.Include(d => d.Projeto).AsQueryable();

        if (projetoId.HasValue && projetoId.Value != Guid.Empty)
            query = query.Where(d => d.ProjetoId == projetoId.Value);

        if (categoria.HasValue)
            query = query.Where(d => d.Categoria == categoria.Value);

        if (dataInicio.HasValue)
            query = query.Where(d => d.DataDespesa >= dataInicio.Value);

        if (dataFim.HasValue)
            query = query.Where(d => d.DataDespesa <= dataFim.Value);

        return await query
            .OrderByDescending(d => d.DataDespesa)
            .ToListAsync();
    }

    public async Task<IEnumerable<DespesaProjeto>> ObterTodasComProjetoAsync()
    {
        return await _dbSet
            .Include(d => d.Projeto)
            .OrderByDescending(d => d.DataDespesa)
            .ToListAsync();
    }
}
