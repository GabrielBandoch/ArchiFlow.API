using ArchiFlow.Domain.Shared;

namespace ArchiFlow.Domain.Fornecedores;

public interface IFornecedorRepository : IRepository<Fornecedor>
{
    Task<IEnumerable<Fornecedor>> ObterTodosComRelacionamentosAsync();
    Task<Fornecedor?> ObterPorIdComRelacionamentosAsync(Guid id);
    Task<IEnumerable<Fornecedor>> ObterPorEspecialidadeAsync(string especialidade);
    Task<IEnumerable<ProjetoFornecedor>> ObterFornecedoresDoProjetoAsync(Guid projetoId);
    Task AdicionarAvaliacaoAsync(AvaliacaoFornecedor avaliacao);
    Task AdicionarVinculoProjetoAsync(ProjetoFornecedor vinculo);
    Task RemoverVinculoProjetoAsync(Guid vinculoId);
}
