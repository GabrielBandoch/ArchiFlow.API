using System;
using System.ComponentModel.DataAnnotations;

namespace ArchiFlow.Application.Financeiro.Commands;

public record CriarContratoCommand(
    Guid ProjetoId,
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor total deve ser maior que zero.")] decimal ValorTotal,
    [Range(1, 120, ErrorMessage = "O número de parcelas deve ser entre 1 e 120.")] int NumeroParcelas,
    DateTime DataPrimeiroVencimento,
    [Range(1, 365, ErrorMessage = "O intervalo de dias deve ser entre 1 e 365.")] int IntervaloDias,
    string? CondicoesPagamento,
    string? Observacoes
);
