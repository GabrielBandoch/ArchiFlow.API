using ArchiFlow.Domain.Financeiro;
using System;

namespace ArchiFlow.Application.Financeiro.DTOs;

public record ParcelaFinanceiraDto(
    Guid Id,
    Guid ProjetoId,
    string ProjetoNome,
    Guid? ClienteId,
    string? ClienteNome,
    Guid? ContratoFinanceiroId,
    int NumeroParcela,
    int TotalParcelas,
    string Descricao,
    decimal Valor,
    DateTime DataVencimento,
    DateTime? DataPagamento,
    StatusParcela Status,
    string StatusNome,
    FormaPagamento? FormaPagamento,
    string? FormaPagamentoNome,
    string? Observacoes,
    string? ComprovanteUrl,
    DateTime CriadoEm
);
