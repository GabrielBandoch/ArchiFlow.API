using ArchiFlow.Domain.Projetos.Enum;

namespace ArchiFlow.Application.Projetos.DTOs;

public record ProjetoDto(
    Guid Id,
    string Nome,
    string Descricao,
    StatusProjeto Status,
    string StatusLabel,
    TipoProjeto Tipo,
    string TipoLabel,
    DateTime DataInicio,
    DateTime? DataPrevistaEntrega,
    decimal MetragemTotal,
    Guid ClienteId,
    DateTime CriadoEm,
    DateTime? AtualizadoEm,
    IEnumerable<EtapaProjetoDto> Etapas,
    int ProgressoPercentual,
    string? ClienteNome = null
);

public record EtapaProjetoDto(
    Guid Id,
    Guid ProjetoId,
    string Nome,
    string Descricao,
    StatusEtapa Status,
    string StatusLabel,
    int Ordem,
    DateTime? DataConclusao,
    IEnumerable<TarefaEtapaDto>? Tarefas = null
);

public record TarefaEtapaDto(
    Guid Id,
    Guid EtapaId,
    string Titulo,
    bool Concluida,
    DateTime? CriadoEm = null
);
