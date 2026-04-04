using ArchiFlow.Domain.Projetos.Enum;

namespace ArchiFlow.Application.Projetos.Commands;
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