using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Projetos.Enum;

namespace ArchiFlow.Application.Projetos.Commands;

public record CriarProjetoCommand(
    string Nome,
    string Descricao,
    TipoProjetoEnum Tipo,
    DateTime DataInicio,
    DateTime? DataPrevistaEntrega,
    decimal MetragemTotal,
    Guid ClienteId
);

public record AtualizarProjetoCommand(
    Guid Id,
    string Nome,
    string Descricao,
    TipoProjetoEnum Tipo,
    StatusProjetoEnum Status,
    DateTime DataInicio,
    DateTime? DataPrevistaEntrega,
    decimal MetragemTotal
);

public record AtualizarStatusProjetoCommand(
    Guid Id,
    StatusProjetoEnum Status
);

public record CriarEtapaCommand(
    Guid ProjetoId,
    string Nome,
    string Descricao,
    int Ordem
);

public record AtualizarStatusEtapaCommand(
    Guid EtapaId,
    StatusEtapaEnum Status
);
