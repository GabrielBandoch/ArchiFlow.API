using System.ComponentModel.DataAnnotations;

namespace ArchiFlow.Application.Usuarios.DTOs;

public record SolicitarRecuperacaoSenhaDto(
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail em formato inválido.")]
    string Email
);
