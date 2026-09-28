using System;
using System.Collections.Generic;

namespace ArchiFlow.Application.Financeiro.DTOs;

public record ContratoFinanceiroDto(
    Guid Id,
    Guid ProjetoId,
    string ProjetoNome,
    decimal ValorTotal,
    string? CondicoesPagamento,
    string? Observacoes,
    DateTime CriadoEm,
    List<ParcelaFinanceiraDto> Parcelas
);
