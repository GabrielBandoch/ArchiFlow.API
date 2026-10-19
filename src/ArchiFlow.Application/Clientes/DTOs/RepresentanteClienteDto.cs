using System;

namespace ArchiFlow.Application.Clientes.DTOs;

public record RepresentanteClienteDto(
    Guid Id,
    Guid ClienteId,
    string Nome,
    string Email,
    string? Telefone,
    string? Cargo,
    bool Ativo,
    string StatusConvite, // "Pendente", "Ativo", "Expirado"
    DateTime CriadoEm,
    DateTime? AtualizadoEm
);
