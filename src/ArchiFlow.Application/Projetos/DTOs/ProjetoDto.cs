using ArchiFlow.Domain.Projetos;
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
    int ProgressoPercentual
);

public record EtapaProjetoDto(
    Guid Id,
    Guid ProjetoId,
    string Nome,
    string Descricao,
    StatusEtapa Status,
    string StatusLabel,
    int Ordem,
    DateTime? DataConclusao
);
