using System;

namespace ArchiFlow.Application.Financeiro.Commands;

public record CriarContratoCommand(
    Guid ProjetoId,
    decimal ValorTotal,
    int NumeroParcelas,
    DateTime DataPrimeiroVencimento,
    int IntervaloDias,
    string? CondicoesPagamento,
    string? Observacoes
);
