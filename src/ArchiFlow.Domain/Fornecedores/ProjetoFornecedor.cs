using ArchiFlow.Domain.Projetos;

namespace ArchiFlow.Domain.Fornecedores;

public class ProjetoFornecedor
{
    public Guid Id { get; set; }
    public Guid ProjetoId { get; set; }
    public Guid FornecedorId { get; set; }
    public string FuncaoNoProjeto { get; set; } = string.Empty;
    public DateTime DataVinculo { get; set; } = DateTime.UtcNow;

    public Fornecedor? Fornecedor { get; set; }
    public Projeto? Projeto { get; set; }
}
