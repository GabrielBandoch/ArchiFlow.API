using System.ComponentModel.DataAnnotations;

namespace ArchiFlow.Application.Usuarios.Commands;

public record AtualizarMeuPerfilCommand(
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 200 caracteres.")]
    string Nome,

    [StringLength(30, ErrorMessage = "O telefone deve ter no máximo 30 caracteres.")]
    string? Telefone,

    [StringLength(100, ErrorMessage = "O cargo deve ter no máximo 100 caracteres.")]
    string? Cargo
);
