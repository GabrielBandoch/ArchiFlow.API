using System.ComponentModel.DataAnnotations;

namespace ArchiFlow.Application.Autenticacao.Commands;

public record RedefinirSenhaCommand(
    [Required(ErrorMessage = "O token é obrigatório.")]
    string Token,

    [Required(ErrorMessage = "A nova senha é obrigatória.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A nova senha deve possuir no mínimo 6 caracteres.")]
    string NovaSenha,

    string? ConfirmacaoNovaSenha = null
);
