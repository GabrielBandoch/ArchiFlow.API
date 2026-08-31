using System;

namespace ArchiFlow.Application.Clientes.Commands;

public record ConvertLeadToClienteCommand(
    Guid LeadId,
    string? CpfCnpj = null,
    string? Telefone = null,
    string? Endereco = null,
    string? FotoUrl = null
);

public record AtualizarClienteCommand(
    Guid Id,
    string Nome,
    string Email,
    string? Telefone,
    string? CpfCnpj,
    string? Endereco,
    string? FotoUrl = null
);

public record AtualizarPortalAccessCommand(
    Guid Id,
    bool Ativo
);
