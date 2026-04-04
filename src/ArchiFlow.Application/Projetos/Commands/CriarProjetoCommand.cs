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
