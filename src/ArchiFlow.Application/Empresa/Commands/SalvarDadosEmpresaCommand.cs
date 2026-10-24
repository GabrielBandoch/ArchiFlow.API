using System.ComponentModel.DataAnnotations;

namespace ArchiFlow.Application.Empresa.Commands;

public record SalvarDadosEmpresaCommand(
    [Required(ErrorMessage = "Nome da empresa/escritório é obrigatório.")]
    [StringLength(200, ErrorMessage = "Nome não pode ultrapassar 200 caracteres.")]
    string NomeEmpresa,

    [StringLength(200)]
    string? RazaoSocial = null,

    [StringLength(30)]
    string? Cnpj = null,

    [StringLength(150)]
    string? RegistroProfissional = null,

    [StringLength(300)]
    string? Slogan = null,

    [EmailAddress(ErrorMessage = "E-mail em formato inválido.")]
    [StringLength(150)]
    string? Email = null,

    [StringLength(50)]
    string? Telefone = null,

    [StringLength(300)]
    string? Endereco = null,

    [StringLength(100)]
    string? Cidade = null,

    [StringLength(50)]
    string? Estado = null,

    [StringLength(20)]
    string? Cep = null,

    [StringLength(200)]
    string? Website = null,

    string? LogoUrl = null,

    [StringLength(20)]
    string? CorPrimaria = null
);

