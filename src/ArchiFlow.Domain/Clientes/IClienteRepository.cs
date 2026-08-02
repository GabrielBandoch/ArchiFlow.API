using ArchiFlow.Domain.Shared;

namespace ArchiFlow.Domain.Clientes;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<Cliente?> GetByEmail(string email);
}
