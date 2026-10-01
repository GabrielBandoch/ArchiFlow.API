using ArchiFlow.Domain.Financeiro;
using System;

namespace ArchiFlow.Application.Financeiro.Commands;

public record DarBaixaParcelaCommand(
    DateTime DataPagamento,
    FormaPagamento FormaPagamento,
    string? Observacoes,
    string? ComprovanteUrl
);
