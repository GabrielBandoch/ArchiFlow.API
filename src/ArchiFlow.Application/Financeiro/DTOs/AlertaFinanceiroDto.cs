using ArchiFlow.Domain.Financeiro;
using System;

namespace ArchiFlow.Application.Financeiro.DTOs;

public record AlertaFinanceiroDto(
    Guid ParcelaId,
    Guid ProjetoId,
    string Titulo,
    string Subtitulo,
    decimal Valor,
    DateTime DataVencimento,
    StatusParcela Status,
    int DiasDiferenca,
    bool EmAtraso
);
