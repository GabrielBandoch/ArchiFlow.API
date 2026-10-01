using ArchiFlow.Domain.Financeiro;
using System;

namespace ArchiFlow.Application.Financeiro.Commands;

public record AtualizarParcelaCommand(
    string Descricao,
    decimal Valor,
    DateTime DataVencimento,
    StatusParcela Status,
    DateTime? DataPagamento,
    FormaPagamento? FormaPagamento,
    string? Observacoes,
    string? ComprovanteUrl
);
