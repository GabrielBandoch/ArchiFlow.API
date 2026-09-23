using System;

namespace ArchiFlow.Domain.Honorarios;

public class ItemPropostaEtapa
{
    public Guid Id { get; set; }
    public Guid PropostaId { get; set; }
    public string NomeEtapa { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Incluso { get; set; }
    public decimal Percentual { get; set; }
    public decimal Valor { get; set; }
    public decimal HorasEstimadas { get; set; }
    public int Ordem { get; set; }

    public PropostaHonorario? Proposta { get; set; }
}
