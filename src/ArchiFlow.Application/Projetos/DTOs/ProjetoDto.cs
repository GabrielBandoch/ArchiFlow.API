using ArchiFlow.Domain.Projetos.Enum;

namespace ArchiFlow.Application.Projetos.DTOs;

public record ProjetoDto(
    Guid Id,
    string Nome,
    string Descricao,
    StatusProjetoEnum Status,
    string StatusLabel,
    TipoProjetoEnum Tipo,
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