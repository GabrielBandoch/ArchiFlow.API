namespace ArchiFlow.Application.Financeiro.DTOs;

public record ReceitaMesDto(
    string Mes,
    int MesNumero,
    int Ano,
    decimal ValorRecebido,
    decimal ValorPrevisto,
    decimal ValorDespesas
);
