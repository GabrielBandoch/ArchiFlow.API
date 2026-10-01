using ArchiFlow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Domain.Financeiro;

public interface IDespesaProjetoRepository : IRepository<DespesaProjeto>
{
    Task<IEnumerable<DespesaProjeto>> ObterPorProjetoIdAsync(Guid projetoId);
    Task<IEnumerable<DespesaProjeto>> ObterComFiltroAsync(Guid? projetoId, CategoriaDespesa? categoria, DateTime? dataInicio, DateTime? dataFim);
    Task<IEnumerable<DespesaProjeto>> ObterTodasComProjetoAsync();
}
