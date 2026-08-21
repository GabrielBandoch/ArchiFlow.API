using System;
using System.Collections.Generic;

namespace ArchiFlow.Application.Projetos.DTOs;

public record TemplateProjetoDto(
    Guid Id,
    string Codigo,
    string Nome,
    string? Descricao,
    string? Icone,
    bool Ativo,
    IEnumerable<TemplateEtapaDto> Etapas
);

public record TemplateEtapaDto(
    Guid Id,
    Guid TemplateProjetoId,
    string Nome,
    string? Descricao,
    int Ordem,
    IEnumerable<string> Tarefas
);
