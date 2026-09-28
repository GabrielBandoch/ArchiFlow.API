using ArchiFlow.Domain.Financeiro;
using System;

namespace ArchiFlow.Application.Financeiro.Commands;

public record CriarDespesaCommand(
    Guid ProjetoId,
    string Descricao,
    decimal Valor,
    DateTime DataDespesa,
    CategoriaDespesa Categoria,
    string? Observacoes,
    string? ComprovanteUrl
);
