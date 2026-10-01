using ArchiFlow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Domain.Financeiro;

public interface IParcelaFinanceiraRepository : IRepository<ParcelaFinanceira>
{
    Task<IEnumerable<ParcelaFinanceira>> ObterPorProjetoIdAsync(Guid projetoId);
    Task<IEnumerable<ParcelaFinanceira>> ObterComFiltroAsync(Guid? projetoId, StatusParcela? status, DateTime? dataInicio, DateTime? dataFim);
    Task<IEnumerable<ParcelaFinanceira>> ObterAlertasVencimentoAsync(int diasLimite = 7);
    Task<IEnumerable<ParcelaFinanceira>> ObterTodasComProjetoAsync();
}
