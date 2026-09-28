using ArchiFlow.Domain.Financeiro;
using System;

namespace ArchiFlow.Application.Financeiro.Commands;

public record AtualizarDespesaCommand(
    string Descricao,
    decimal Valor,
    DateTime DataDespesa,
    CategoriaDespesa Categoria,
    string? Observacoes,
    string? ComprovanteUrl
);
