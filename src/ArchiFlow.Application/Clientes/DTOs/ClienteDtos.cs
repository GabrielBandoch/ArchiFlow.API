using System;

namespace ArchiFlow.Application.Clientes.DTOs;

public record ClienteDto(
    Guid Id,
    Guid? LeadId,
    string Nome,
    string Email,
    string? Telefone,
    string? CpfCnpj,
    string? Endereco,
    bool Ativo,
    int ProjetosAtivosCount = 0,
    string? FotoUrl = null
);

public record ConversaoClienteResponseDto(
    ClienteDto Cliente,
    string SenhaTemporaria
);
