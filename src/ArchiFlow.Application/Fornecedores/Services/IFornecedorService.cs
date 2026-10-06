using ArchiFlow.Application.Fornecedores.Commands;
using ArchiFlow.Application.Fornecedores.DTOs;

namespace ArchiFlow.Application.Fornecedores.Services;

public interface IFornecedorService
{
    Task<IEnumerable<FornecedorDto>> ObterTodosAsync(string? especialidade = null);
    Task<FornecedorDto?> ObterPorIdAsync(Guid id);
    Task<FornecedorDto> CriarAsync(CriarFornecedorCommand command);
    Task<FornecedorDto> AtualizarAsync(AtualizarFornecedorCommand command);
    Task<bool> ExcluirAsync(Guid id);
    Task<AvaliacaoFornecedorDto> AdicionarAvaliacaoAsync(AdicionarAvaliacaoCommand command);
    Task<ProjetoFornecedorDto> VincularProjetoAsync(VincularProjetoCommand command);
    Task<bool> DesvincularProjetoAsync(Guid vinculoId);
    Task<IEnumerable<ProjetoFornecedorDto>> ObterFornecedoresDoProjetoAsync(Guid projetoId);
}
