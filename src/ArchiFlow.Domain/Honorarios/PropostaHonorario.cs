using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Leads;
using ArchiFlow.Domain.Projetos.Enum;
using System;
using System.Collections.Generic;

namespace ArchiFlow.Domain.Honorarios;

public class PropostaHonorario
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public Guid? ClienteId { get; set; }
    public string? ClienteNome { get; set; }
    public Guid? LeadId { get; set; }
    public string? LeadNome { get; set; }
    public TipoProjeto TipoProjeto { get; set; }
    public PadraoImovel PadraoImovel { get; set; }
    public decimal MetragemQuadrada { get; set; }
    public decimal ValorHoraBase { get; set; }
    public decimal ValorMetroQuadradoBase { get; set; }
    public decimal HorasEstimadasTotal { get; set; }
    public decimal ValorBase { get; set; }
    public decimal ValorFatorPadrao { get; set; }
    public decimal ValorFatorTipologia { get; set; }
    public decimal ValorEscopo { get; set; }
    public decimal ValorTotalSugerido { get; set; }
    public decimal ValorFinalAjustado { get; set; }
    public StatusProposta Status { get; set; }
    public string? Observacoes { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public Cliente? Cliente { get; set; }
    public Lead? Lead { get; set; }
    public ICollection<ItemPropostaEtapa> ItensEtapa { get; set; } = new List<ItemPropostaEtapa>();
}
