using System.ComponentModel.DataAnnotations;

namespace ArchiFlow.Application.Fornecedores.Commands;

public class CriarFornecedorCommand
{
    [Required(ErrorMessage = "O nome do parceiro é obrigatório")]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A especialidade é obrigatória")]
    [MaxLength(100)]
    public string Especialidade { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? Telefone { get; set; }

    [MaxLength(100)]
    public string? Cidade { get; set; }

    [MaxLength(50)]
    public string? Estado { get; set; }

    [MaxLength(1000)]
    public string? Descricao { get; set; }
}

public class AtualizarFornecedorCommand
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Especialidade { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? Telefone { get; set; }

    [MaxLength(100)]
    public string? Cidade { get; set; }

    [MaxLength(50)]
    public string? Estado { get; set; }

    [MaxLength(1000)]
    public string? Descricao { get; set; }

    public bool Ativo { get; set; } = true;
}

public class AdicionarAvaliacaoCommand
{
    [Required]
    public Guid FornecedorId { get; set; }

    public Guid? ProjetoId { get; set; }

    [Range(1, 5, ErrorMessage = "A nota deve estar entre 1 e 5 estrelas")]
    public int Nota { get; set; }

    [Required(ErrorMessage = "O comentário é obrigatório")]
    [MaxLength(1000)]
    public string Comentario { get; set; } = string.Empty;

    [MaxLength(150)]
    public string AutorNome { get; set; } = "Arquiteto Titular";
}

public class VincularProjetoCommand
{
    [Required]
    public Guid FornecedorId { get; set; }

    [Required]
    public Guid ProjetoId { get; set; }

    [MaxLength(150)]
    public string FuncaoNoProjeto { get; set; } = "Fornecedor / Parceiro Especializado";
}
