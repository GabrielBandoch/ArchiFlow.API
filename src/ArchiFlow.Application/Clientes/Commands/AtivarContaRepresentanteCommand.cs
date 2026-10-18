using System.ComponentModel.DataAnnotations;

namespace ArchiFlow.Application.Clientes.Commands;

public record AtivarContaRepresentanteCommand(
    [Required(ErrorMessage = "O token de ativação é obrigatório.")]
    string Token,

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
    string Senha,

    string? ConfirmacaoSenha = null
);
