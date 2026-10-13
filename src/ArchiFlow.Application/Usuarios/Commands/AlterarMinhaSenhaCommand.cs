using System.ComponentModel.DataAnnotations;

namespace ArchiFlow.Application.Usuarios.Commands;

public record AlterarMinhaSenhaCommand(
    [Required(ErrorMessage = "A senha atual é obrigatória.")]
    string SenhaAtual,

    [Required(ErrorMessage = "A nova senha é obrigatória.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A nova senha deve ter no mínimo 6 caracteres.")]
    string NovaSenha
);
