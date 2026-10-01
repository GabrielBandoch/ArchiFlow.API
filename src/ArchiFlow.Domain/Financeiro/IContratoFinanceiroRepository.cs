using ArchiFlow.Domain.Shared;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.Domain.Financeiro;

public interface IContratoFinanceiroRepository : IRepository<ContratoFinanceiro>
{
    Task<ContratoFinanceiro?> ObterPorProjetoIdAsync(Guid projetoId);
}
