using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Projetos.Enum;

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
