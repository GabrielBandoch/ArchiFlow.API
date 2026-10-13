using System;

namespace ArchiFlow.Application.Usuarios.DTOs;

public record PerfilUsuarioDto(
    Guid Id,
    Guid? EscritorioId,
    string Nome,
    string Email,
    string Role,
    string? Cargo,
    string? Telefone,
    DateTime CriadoEm,
    DateTime? AtualizadoEm
);
