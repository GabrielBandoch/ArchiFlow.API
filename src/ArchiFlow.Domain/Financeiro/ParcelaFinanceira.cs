using ArchiFlow.Domain.Projetos;
using System;

namespace ArchiFlow.Domain.Financeiro;

public class ParcelaFinanceira
{
    public Guid Id { get; set; }
    public Guid ProjetoId { get; set; }
    public Guid? ContratoFinanceiroId { get; set; }
    public int NumeroParcela { get; set; }
    public int TotalParcelas { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public StatusParcela Status { get; set; } = StatusParcela.Pendente;
    public FormaPagamento? FormaPagamento { get; set; }
    public string? Observacoes { get; set; }
    public string? ComprovanteUrl { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }

    public Projeto? Projeto { get; set; }
    public ContratoFinanceiro? ContratoFinanceiro { get; set; }
}
