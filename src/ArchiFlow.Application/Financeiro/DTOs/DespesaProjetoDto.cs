using ArchiFlow.Domain.Financeiro;
using System;

namespace ArchiFlow.Application.Financeiro.DTOs;

public record DespesaProjetoDto(
    Guid Id,
    Guid ProjetoId,
    string ProjetoNome,
    string Descricao,
    decimal Valor,
    DateTime DataDespesa,
    CategoriaDespesa Categoria,
    string CategoriaNome,
    string? Observacoes,
    string? ComprovanteUrl,
    DateTime CriadoEm
);
