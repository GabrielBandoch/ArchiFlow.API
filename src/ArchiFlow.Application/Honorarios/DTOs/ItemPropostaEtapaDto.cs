using System;

namespace ArchiFlow.Application.Honorarios.DTOs;

public record ItemPropostaEtapaDto(
    Guid Id,
    Guid PropostaId,
    string NomeEtapa,
    string? Descricao,
    bool Incluso,
    decimal Percentual,
    decimal Valor,
    decimal HorasEstimadas,
    int Ordem
);
