using System;

namespace ArchiFlow.Application.SuperAdmin.DTOs;

public class TenantAssinaturaDto
{
    public Guid Id { get; set; }
    public string Subdominio { get; set; } = string.Empty;
    public string NomeEscritorio { get; set; } = string.Empty;
    public string? RazaoSocial { get; set; }
    public string? CnpjCpf { get; set; }
    public string NomeResponsavel { get; set; } = string.Empty;
    public string EmailResponsavel { get; set; } = string.Empty;
    public string? TelefoneResponsavel { get; set; }
    public string Plano { get; set; } = string.Empty;
    public string CicloFaturamento { get; set; } = string.Empty;
    public decimal ValorMensalidade { get; set; }
    public string StatusFinanceiro { get; set; } = string.Empty;
    public DateTime DataVencimento { get; set; }
    public DateTime? UltimoPagamento { get; set; }
    public bool BloqueadoPorInadimplencia { get; set; }
    public string? MotivoBloqueio { get; set; }
    public string? NotasInternas { get; set; }
    public int DiasEmAtraso { get; set; }
    public string LinkAcesso { get; set; } = string.Empty;
    public string LinkWhatsAppCobranca { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}
