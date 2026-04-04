using ArchiFlow.Domain.Projetos.Enum;

namespace ArchiFlow.Application.Projetos.DTOs;

public record EtapaProjetoDto(
    Guid Id,
    Guid ProjetoId,
    string Nome,
    string Descricao,
    StatusEtapaEnum Status,
    string StatusLabel,
    int Ordem,
    DateTime? DataConclusao
);
