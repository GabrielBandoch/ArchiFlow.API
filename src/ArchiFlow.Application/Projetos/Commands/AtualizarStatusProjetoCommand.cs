using ArchiFlow.Domain.Projetos.Enum;

namespace ArchiFlow.Application.Projetos.Commands;

public record AtualizarStatusProjetoCommand(
    Guid Id,
    StatusProjetoEnum Status
);
