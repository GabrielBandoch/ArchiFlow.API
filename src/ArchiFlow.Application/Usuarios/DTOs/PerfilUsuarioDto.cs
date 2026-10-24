using System;
using System.Collections.Generic;

namespace ArchiFlow.Application.Usuarios.DTOs;

public record PerfilUsuarioDto(
    Guid Id,
    Guid? EscritorioId,
    string Nome,
    string Email,
    string Role,
    string? Cargo,
    string? Telefone,
    string? FotoUrl,
    List<string> ModulosPermitidos,
    DateTime CriadoEm
);

