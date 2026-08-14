using ArchiFlow.Domain.Leads.Enum;

namespace ArchiFlow.Application.Leads.Commands;

public record CriarLeadCommand(
    string Nome,
    string Email,
    string? Telefone,
    Guid? OrigemId
);

public record AtualizarLeadCommand(
    Guid Id,
    string Nome,
    string Email,
    string? Telefone,
    Guid? OrigemId
);

public record AtualizarStatusLeadCommand(
    Guid Id,
    StatusLead Status,
    string? MotivoPerda
);

public record RegistrarContatoLeadCommand(
    Guid LeadId,
    string Canal,
    string Resumo
);
