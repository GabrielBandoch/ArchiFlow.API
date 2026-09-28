using ArchiFlow.Domain.Financeiro;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Repositories.Financeiro;

public class ContratoFinanceiroRepository : Repository<ContratoFinanceiro>, IContratoFinanceiroRepository
{
    public ContratoFinanceiroRepository(ArchiFlowDbContext context) : base(context) { }

    public async Task<ContratoFinanceiro?> ObterPorProjetoIdAsync(Guid projetoId)
    {
        return await _dbSet
            .Include(c => c.Projeto)
            .Include(c => c.Parcelas.OrderBy(p => p.NumeroParcela))
            .FirstOrDefaultAsync(c => c.ProjetoId == projetoId);
    }
}
