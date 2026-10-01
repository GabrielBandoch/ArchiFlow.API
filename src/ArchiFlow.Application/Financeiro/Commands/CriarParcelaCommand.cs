using System;

namespace ArchiFlow.Application.Financeiro.Commands;

public record CriarParcelaCommand(
    Guid ProjetoId,
    Guid? ContratoFinanceiroId,
    int NumeroParcela,
    int TotalParcelas,
    string Descricao,
    decimal Valor,
    DateTime DataVencimento,
    string? Observacoes
);
