using System.ComponentModel.DataAnnotations;

namespace ArchiFlow.Application.Usuarios.Commands;

public record AtualizarPerfilCommand(
    [Required(ErrorMessage = "Nome é obrigatório.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Nome deve ter entre 2 e 200 caracteres.")]
    string Nome,

    [StringLength(100, ErrorMessage = "Cargo não pode ultrapassar 100 caracteres.")]
    string? Cargo,

    [StringLength(30, ErrorMessage = "Telefone não pode ultrapassar 30 caracteres.")]
    string? Telefone
);
