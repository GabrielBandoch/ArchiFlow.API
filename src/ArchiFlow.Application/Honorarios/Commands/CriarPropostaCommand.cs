using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Projetos.Enum;
using System;
using System.Collections.Generic;

namespace ArchiFlow.Application.Honorarios.Commands;

public record CriarPropostaCommand(
    string Titulo,
    Guid? ClienteId,
    string? ClienteNome,
    Guid? LeadId,
    string? LeadNome,
    TipoProjeto TipoProjeto,
    PadraoImovel PadraoImovel,
    decimal MetragemQuadrada,
    decimal? ValorHoraBase,
    decimal? ValorMetroQuadradoBase,
    decimal? ValorFinalAjustado,
    List<string>? EtapasInclusas,
    string? Observacoes
);
