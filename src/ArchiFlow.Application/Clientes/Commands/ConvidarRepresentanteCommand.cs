using System.ComponentModel.DataAnnotations;

namespace ArchiFlow.Application.Clientes.Commands;

public record ConvidarRepresentanteCommand(
    [Required(ErrorMessage = "Nome é obrigatório.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Nome deve ter entre 2 e 200 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "E-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail em formato inválido.")]
    [StringLength(256, ErrorMessage = "E-mail não pode ultrapassar 256 caracteres.")]
    string Email,

    [StringLength(30, ErrorMessage = "Telefone não pode ultrapassar 30 caracteres.")]
    string? Telefone = null,

    [StringLength(100, ErrorMessage = "Cargo não pode ultrapassar 100 caracteres.")]
    string? Cargo = null,

    bool EnviarEmailConvite = true
);
