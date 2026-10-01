using ArchiFlow.Domain.Projetos;
using System;
using System.Collections.Generic;

namespace ArchiFlow.Domain.Financeiro;

public class ContratoFinanceiro
{
    public Guid Id { get; set; }
    public Guid ProjetoId { get; set; }
    public decimal ValorTotal { get; set; }
    public string? CondicoesPagamento { get; set; }
    public string? Observacoes { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }

    public Projeto? Projeto { get; set; }
    public ICollection<ParcelaFinanceira> Parcelas { get; set; } = new List<ParcelaFinanceira>();
}
