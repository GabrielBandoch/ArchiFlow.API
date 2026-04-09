using ArchiFlow.Domain.Projetos.Enum;

namespace ArchiFlow.Application.Projetos.Commands;
public record AtualizarStatusEtapaCommand(
    Guid EtapaId,
    StatusEtapaEnum Status
);