using System;
using System.Collections.Generic;

namespace ArchiFlow.Application.Usuarios.DTOs;

public record LoginResponseDto(
    string Token,
    string Perfil,
    string Nome,
    string Email,
    Guid Id,
    Guid? ProjetoId = null,
    List<string>? ModulosPermitidos = null,
    string? FotoUrl = null
);

