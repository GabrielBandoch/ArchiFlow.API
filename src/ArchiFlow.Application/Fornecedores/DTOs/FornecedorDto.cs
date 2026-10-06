namespace ArchiFlow.Application.Fornecedores.DTOs;

public class FornecedorDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Especialidade { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Descricao { get; set; }
    public decimal AvaliacaoMedia { get; set; }
    public int TotalAvaliacoes { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public int TotalProjetosAtivos { get; set; }

    public List<AvaliacaoFornecedorDto> Avaliacoes { get; set; } = new();
    public List<ProjetoFornecedorDto> ProjetosVinculados { get; set; } = new();
}

public class AvaliacaoFornecedorDto
{
    public Guid Id { get; set; }
    public Guid FornecedorId { get; set; }
    public Guid? ProjetoId { get; set; }
    public int Nota { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public string AutorNome { get; set; } = string.Empty;
    public DateTime DataAvaliacao { get; set; }
}

public class ProjetoFornecedorDto
{
    public Guid Id { get; set; }
    public Guid ProjetoId { get; set; }
    public string? ProjetoNome { get; set; }
    public Guid FornecedorId { get; set; }
    public string? FornecedorNome { get; set; }
    public string FuncaoNoProjeto { get; set; } = string.Empty;
    public DateTime DataVinculo { get; set; }
}
