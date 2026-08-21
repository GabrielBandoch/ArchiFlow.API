using ArchiFlow.Domain.Projetos.Enum;
using System;
using System.Collections.Generic;

namespace ArchiFlow.Application.Projetos.Commands;

public record CriarProjetoCommand(
    string Nome,
    string Descricao,
    TipoProjeto Tipo,
    DateTime DataInicio,
    DateTime? DataPrevistaEntrega,
    decimal MetragemTotal,
    Guid ClienteId
);

public record AtualizarProjetoCommand(
    Guid Id,
    string Nome,
    string Descricao,
    TipoProjeto Tipo,
    StatusProjeto Status,
    DateTime DataInicio,
    DateTime? DataPrevistaEntrega,
    decimal MetragemTotal
);

public record AtualizarStatusProjetoCommand(
    Guid Id,
    StatusProjeto Status
);

public record CriarEtapaCommand(
    Guid ProjetoId,
    string Nome,
    string Descricao,
    int Ordem
);

public record AtualizarStatusEtapaCommand(
    Guid EtapaId,
    StatusEtapa Status
);

public record AdicionarTarefaCommand(
    Guid EtapaId,
    string Titulo
);

public record AlternarTarefaCommand(
    Guid TarefaId
);

public record CriarTemplateProjetoCommand(
    string Codigo,
    string Nome,
    string? Descricao,
    string? Icone,
    List<CriarTemplateEtapaItemCommand>? Etapas = null
);

public record CriarTemplateEtapaItemCommand(
    string Nome,
    string? Descricao,
    int Ordem,
    List<string>? Tarefas = null
);
