using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Projetos.Enum;
using System.Collections.Generic;

namespace ArchiFlow.Application.Honorarios.DTOs;

public record SimulacaoResultadoDto(
    decimal MetragemQuadrada,
    TipoProjeto TipoProjeto,
    string TipoProjetoNome,
    PadraoImovel PadraoImovel,
    string PadraoImovelNome,
    decimal ValorTotalSugerido,
    decimal ValorMetroQuadrado,
    decimal HorasEstimadasTotal,
    List<ItemEtapaSimulacaoDto> Etapas,
    MemoriaCalculoDto MemoriaCalculo
);
