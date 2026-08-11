using System;

namespace ArchiFlow.Application.Leads.DTOs;

public record OrigemLeadDto(
    Guid Id,
    string Descricao,
    bool Ativo,
    DateTime CriadoEm
);
