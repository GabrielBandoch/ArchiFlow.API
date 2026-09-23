namespace ArchiFlow.Application.Honorarios.DTOs;

public record ItemEtapaSimulacaoDto(
    string Nome,
    string? Descricao,
    bool Incluso,
    decimal Percentual,
    decimal Valor,
    decimal HorasEstimadas,
    int Ordem
);
