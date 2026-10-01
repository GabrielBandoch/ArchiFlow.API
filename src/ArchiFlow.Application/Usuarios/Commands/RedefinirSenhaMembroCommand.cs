using System.ComponentModel.DataAnnotations;

namespace ArchiFlow.Application.Usuarios.Commands;

public record RedefinirSenhaMembroCommand(
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
    string? NovaSenha
);
