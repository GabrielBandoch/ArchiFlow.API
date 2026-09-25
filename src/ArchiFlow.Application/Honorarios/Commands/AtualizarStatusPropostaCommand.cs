using ArchiFlow.Domain.Honorarios;

namespace ArchiFlow.Application.Honorarios.Commands;

public record AtualizarStatusPropostaCommand(
    StatusProposta Status
);
