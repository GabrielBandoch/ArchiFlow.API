using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Projetos.Enum;
using System.Collections.Generic;

namespace ArchiFlow.Application.Honorarios.DTOs;

public record SimulacaoParametrosDto(
    decimal MetragemQuadrada,
    TipoProjeto TipoProjeto,
    PadraoImovel PadraoImovel,
    List<string>? EtapasInclusas,
    decimal? ValorHoraBase,
    decimal? ValorMetroQuadradoBase
);
