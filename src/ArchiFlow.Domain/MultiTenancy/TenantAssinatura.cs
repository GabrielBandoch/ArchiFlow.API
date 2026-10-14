using System;

namespace ArchiFlow.Domain.MultiTenancy;

public class TenantAssinatura
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Subdominio { get; set; } = string.Empty;
    public string NomeEscritorio { get; set; } = string.Empty;
    public string? RazaoSocial { get; set; }
    public string? CnpjCpf { get; set; }
    public string NomeResponsavel { get; set; } = string.Empty;
    public string EmailResponsavel { get; set; } = string.Empty;
    public string? TelefoneResponsavel { get; set; }
    public string Plano { get; set; } = "Professional";
    public string CicloFaturamento { get; set; } = "Mensal";
    public decimal ValorMensalidade { get; set; }
    public string StatusFinanceiro { get; set; } = "Adimplente";
    public DateTime DataVencimento { get; set; } = DateTime.UtcNow.AddMonths(1);
    public DateTime? UltimoPagamento { get; set; }
    public bool BloqueadoInadimplencia { get; set; }
    public string? MotivoBloqueio { get; set; }
    public string? NotasInternas { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }
}
