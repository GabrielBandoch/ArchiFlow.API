using System;

namespace ArchiFlow.Application.Usuarios.DTOs;

public record MembroEquipeDto(
    Guid Id,
    Guid? EscritorioId,
    string Nome,
    string Email,
    string Role,
    string? Cargo,
    string? Telefone,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm
);
