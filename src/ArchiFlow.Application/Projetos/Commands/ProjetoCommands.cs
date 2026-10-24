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
    int Ordem,
    DateTime? DataFimPrevista = null
);

public record AtualizarStatusEtapaCommand(
    Guid EtapaId,
    StatusEtapa Status,
    DateTime? DataConclusao = null
);

public record AtualizarDatasEtapaCommand(
    Guid EtapaId,
    DateTime? DataFimPrevista = null,
    DateTime? DataConclusao = null,
    bool LimparDataFimPrevista = false,
    bool LimparDataConclusao = false
);

public record AdicionarTarefaCommand(
    Guid EtapaId,
    string Titulo,
    DateTime? DataLimite = null
);

public record AlternarTarefaCommand(
    Guid TarefaId
);

public record AtualizarPrazoTarefaCommand(
    Guid TarefaId,
    DateTime? DataLimite = null
);

public record CriarTemplateProjetoCommand(
    string Codigo,
    string Nome,
    string? Descricao,
    string? Icone,
    List<CriarTemplateEtapaItemCommand>? Etapas = null
);

public record AtualizarTemplateProjetoCommand(
    Guid Id,
    string Nome,
    string? Descricao,
    string? Icone,
    List<CriarTemplateEtapaItemCommand>? Etapas = null
);

public record AtualizarTemplatePorCodigoCommand(
    string Codigo = "",
    string Nome = "",
    string? Descricao = null,
    string? Icone = null,
    List<CriarTemplateEtapaItemCommand>? Etapas = null
);

public record CriarTemplateEtapaItemCommand(
    string Nome,
    string? Descricao,
    int Ordem,
    List<string>? Tarefas = null
);
