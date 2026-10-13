using System;
using System.ComponentModel.DataAnnotations;

namespace ArchiFlow.Application.SuperAdmin.DTOs;

public class CriarTenantAssinaturaRequestDto
{
    [Required(ErrorMessage = "O subdomínio é obrigatório.")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "O subdomínio deve conter apenas letras minúsculas, números e hífens.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "O subdomínio deve ter entre 3 e 50 caracteres.")]
    public string Subdominio { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome do escritório é obrigatório.")]
    [StringLength(200, ErrorMessage = "O nome deve ter até 200 caracteres.")]
    public string NomeEscritorio { get; set; } = string.Empty;

    public string? RazaoSocial { get; set; }
    public string? CnpjCpf { get; set; }

    [Required(ErrorMessage = "O nome do responsável é obrigatório.")]
    public string NomeResponsavel { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail do responsável é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string EmailResponsavel { get; set; } = string.Empty;

    public string? TelefoneResponsavel { get; set; }

    [Required(ErrorMessage = "O plano é obrigatório.")]
    public string Plano { get; set; } = "Professional";

    public string CicloFaturamento { get; set; } = "Mensal";

    [Range(0, 100000, ErrorMessage = "O valor deve ser maior ou igual a zero.")]
    public decimal ValorMensalidade { get; set; } = 349.00m;

    public string StatusFinanceiro { get; set; } = "Adimplente";

    public DateTime? DataVencimento { get; set; }
    public string? NotasInternas { get; set; }
}

public class AtualizarStatusFinanceiroDto
{
    [Required]
    public string StatusFinanceiro { get; set; } = "Adimplente"; // Adimplente, EmDebito, Suspenso, Trial

    public DateTime? NovaDataVencimento { get; set; }
    public bool RegistrarPagamentoHoje { get; set; } = false;
    public string? NotasInternas { get; set; }
}

public class BloquearTenantRequestDto
{
    public bool Bloquear { get; set; } = true;
    public string? Motivo { get; set; }
}

public class AtualizarNotasRequestDto
{
    public string? NotasInternas { get; set; }
}
