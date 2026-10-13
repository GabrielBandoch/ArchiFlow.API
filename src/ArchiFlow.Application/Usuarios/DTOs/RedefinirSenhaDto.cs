using System.ComponentModel.DataAnnotations;

namespace ArchiFlow.Application.Usuarios.DTOs;

public record RedefinirSenhaDto(
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail em formato inválido.")]
    string Email,

    [Required(ErrorMessage = "O código ou token de verificação é obrigatório.")]
    string Token,

    [Required(ErrorMessage = "A nova senha é obrigatória.")]
    [MinLength(6, ErrorMessage = "A nova senha deve ter no mínimo 6 caracteres.")]
    string NovaSenha
);
