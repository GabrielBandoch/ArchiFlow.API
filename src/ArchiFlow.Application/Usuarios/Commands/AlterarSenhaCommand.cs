using System.ComponentModel.DataAnnotations;

namespace ArchiFlow.Application.Usuarios.Commands;

public record AlterarSenhaCommand(
    [Required(ErrorMessage = "Senha atual é obrigatória.")]
    string SenhaAtual,

    [Required(ErrorMessage = "Nova senha é obrigatória.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A nova senha deve ter no mínimo 6 caracteres.")]
    string NovaSenha,

    string? ConfirmacaoNovaSenha = null
);
