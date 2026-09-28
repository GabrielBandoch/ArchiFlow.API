using ArchiFlow.Domain.Financeiro;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Repositories.Financeiro;

public class ParcelaFinanceiraRepository : Repository<ParcelaFinanceira>, IParcelaFinanceiraRepository
{
    public ParcelaFinanceiraRepository(ArchiFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<ParcelaFinanceira>> ObterPorProjetoIdAsync(Guid projetoId)
    {
        return await _dbSet
            .Include(p => p.Projeto)
            .Where(p => p.ProjetoId == projetoId)
            .OrderBy(p => p.NumeroParcela)
            .ThenBy(p => p.DataVencimento)
            .ToListAsync();
    }

    public async Task<IEnumerable<ParcelaFinanceira>> ObterComFiltroAsync(Guid? projetoId, StatusParcela? status, DateTime? dataInicio, DateTime? dataFim)
    {
        var query = _dbSet.Include(p => p.Projeto).AsQueryable();

        if (projetoId.HasValue && projetoId.Value != Guid.Empty)
            query = query.Where(p => p.ProjetoId == projetoId.Value);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (dataInicio.HasValue)
            query = query.Where(p => p.DataVencimento >= dataInicio.Value);

        if (dataFim.HasValue)
            query = query.Where(p => p.DataVencimento <= dataFim.Value);

        return await query
            .OrderBy(p => p.DataVencimento)
            .ToListAsync();
    }

    public async Task<IEnumerable<ParcelaFinanceira>> ObterAlertasVencimentoAsync(int diasLimite = 7)
    {
        var hoje = DateTime.UtcNow.Date;
        var limite = hoje.AddDays(diasLimite);

        return await _dbSet
            .Include(p => p.Projeto)
            .Where(p => p.Status != StatusParcela.Pago && p.Status != StatusParcela.Cancelado &&
                        (p.DataVencimento < hoje || (p.DataVencimento >= hoje && p.DataVencimento <= limite)))
            .OrderBy(p => p.DataVencimento)
            .ToListAsync();
    }

    public async Task<IEnumerable<ParcelaFinanceira>> ObterTodasComProjetoAsync()
    {
        return await _dbSet
            .Include(p => p.Projeto)
            .OrderByDescending(p => p.DataVencimento)
            .ToListAsync();
    }
}
