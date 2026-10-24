using System.ComponentModel.DataAnnotations;

namespace ArchiFlow.Application.Autenticacao.Commands;

public record SolicitarRecuperacaoCommand(
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail em formato inválido.")]
    string Email
);

