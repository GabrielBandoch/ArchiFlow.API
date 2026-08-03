using ArchiFlow.Domain.Usuarios;
using ArchiFlow.Domain.Clientes;

namespace ArchiFlow.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateToken(Usuario usuario);
    string GenerateToken(Cliente cliente, Guid? projectId);
}
