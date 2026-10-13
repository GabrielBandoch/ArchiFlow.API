using System;

namespace ArchiFlow.Domain.Assinaturas;

/// <summary>
/// Representa o registro mestre de assinatura de um escritório/cliente SaaS da COTU.
/// Permite o controle financeiro (adimplência, débitos, suspensões) e provisionamento de tenants.
/// </summary>
public class TenantAssinatura
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identificador do subdomínio (ex: 'duna', 'alfa-arquitetura', 'staging').
    /// Mapeia para o banco de dados cotu_{subdominio}.
    /// </summary>
    public string Subdominio { get; set; } = string.Empty;

    public string NomeEscritorio { get; set; } = string.Empty;
    public string? RazaoSocial { get; set; }
    public string? CnpjCpf { get; set; }

    public string NomeResponsavel { get; set; } = string.Empty;
    public string EmailResponsavel { get; set; } = string.Empty;
    public string? TelefoneResponsavel { get; set; }

    /// <summary>
    /// Plano da assinatura: 'Starter', 'Professional', 'Studio Scale', 'Enterprise'
    /// </summary>
    public string Plano { get; set; } = "Professional";

    /// <summary>
    /// Ciclo de faturamento: 'Mensal', 'Anual'
    /// </summary>
    public string CicloFaturamento { get; set; } = "Mensal";

    public decimal ValorMensalidade { get; set; } = 349.00m;

    /// <summary>
    /// Status financeiro da assinatura: 'Adimplente', 'EmDebito', 'Suspenso', 'Trial'
    /// </summary>
    public string StatusFinanceiro { get; set; } = "Adimplente";

    /// <summary>
    /// Data de vencimento da mensalidade atual
    /// </summary>
    public DateTime DataVencimento { get; set; } = DateTime.UtcNow.AddDays(30);

    /// <summary>
    /// Data do último pagamento confirmado
    /// </summary>
    public DateTime? UltimoPagamento { get; set; }

    /// <summary>
    /// Se verdadeiro, os membros deste escritório têm o acesso bloqueado até a regularização financeira
    /// </summary>
    public bool BloqueadoPorInadimplencia { get; set; } = false;

    public string? MotivoBloqueio { get; set; }

    /// <summary>
    /// Anotações internas da equipe COTU sobre negociações e cobrança
    /// </summary>
    public string? NotasInternas { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }

    public int CalcularDiasEmAtraso()
    {
        if (StatusFinanceiro == "Adimplente" || StatusFinanceiro == "Trial")
        {
            return 0;
        }

        if (DateTime.UtcNow > DataVencimento)
        {
            return (int)Math.Max(1, (DateTime.UtcNow - DataVencimento).TotalDays);
        }

        return 0;
    }
}
