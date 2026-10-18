using System.ComponentModel.DataAnnotations;

namespace ArchiFlow.Application.Autenticacao.Commands;

public record ValidarTokenRecuperacaoCommand(
    [Required(ErrorMessage = "O token é obrigatório.")]
    string Token
);
