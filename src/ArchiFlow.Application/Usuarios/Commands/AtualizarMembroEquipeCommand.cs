using System.ComponentModel.DataAnnotations;

namespace ArchiFlow.Application.Usuarios.Commands;

public record AtualizarMembroEquipeCommand(
    [Required(ErrorMessage = "Nome é obrigatório.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Nome deve ter entre 2 e 200 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "Role/Função é obrigatória.")]
    string Role,

    [StringLength(100, ErrorMessage = "Cargo não pode ultrapassar 100 caracteres.")]
    string? Cargo,

    [StringLength(30, ErrorMessage = "Telefone não pode ultrapassar 30 caracteres.")]
    string? Telefone,

    [EmailAddress(ErrorMessage = "E-mail em formato inválido.")]
    string? Email = null
);
