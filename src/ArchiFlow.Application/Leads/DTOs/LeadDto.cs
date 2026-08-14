using ArchiFlow.Domain.Leads.Enum;

namespace ArchiFlow.Application.Leads.DTOs;

public record LeadDto(
    Guid Id,
    string Nome,
    string Email,
    string? Telefone,
    Guid? OrigemId,
    string? Origem,
    string? MotivoPerda,
    StatusLead Status,
    string StatusLabel,
    DateTime CriadoEm,
    DateTime? AtualizadoEm,
    IEnumerable<HistoricoContatoLeadDto> HistoricoContatos
);

public record HistoricoContatoLeadDto(
    Guid Id,
    Guid LeadId,
    DateTime DataContato,
    string Canal,
    string Resumo
);
