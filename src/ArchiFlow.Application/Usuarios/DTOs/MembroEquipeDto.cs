using System;
using System.Collections.Generic;

namespace ArchiFlow.Application.Usuarios.DTOs;

public record MembroEquipeDto(
    Guid Id,
    Guid? EscritorioId,
    string Nome,
    string Email,
    string Role,
    string? Cargo,
    string? Telefone,
    string? FotoUrl,
    List<string> ModulosPermitidos,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm
)
{
    public MembroEquipeDto(
        Guid id,
        Guid? escritorioId,
        string nome,
        string email,
        string role,
        string? cargo,
        string? telefone,
        bool ativo,
        DateTime criadoEm,
        DateTime? atualizadoEm)
        : this(id, escritorioId, nome, email, role, cargo, telefone, null, new List<string>(), ativo, criadoEm, atualizadoEm)
    {
    }
}
