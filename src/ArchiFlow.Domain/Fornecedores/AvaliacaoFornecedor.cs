namespace ArchiFlow.Domain.Fornecedores;

public class AvaliacaoFornecedor
{
    public Guid Id { get; set; }
    public Guid FornecedorId { get; set; }
    public Guid? ProjetoId { get; set; }
    public int Nota { get; set; } // 1 a 5
    public string Comentario { get; set; } = string.Empty;
    public string AutorNome { get; set; } = string.Empty;
    public DateTime DataAvaliacao { get; set; } = DateTime.UtcNow;

    public Fornecedor? Fornecedor { get; set; }
}
