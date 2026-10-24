using ArchiFlow.Domain.Usuarios;
using ArchiFlow.Domain.Clientes;
using System;

namespace ArchiFlow.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateToken(Usuario usuario);
    string GenerateToken(Cliente cliente, Guid? projectId);
    string GenerateToken(RepresentanteCliente representante, Guid? projectId);
}

