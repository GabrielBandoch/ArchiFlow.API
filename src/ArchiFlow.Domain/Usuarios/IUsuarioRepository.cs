using ArchiFlow.Domain.Shared;

namespace ArchiFlow.Domain.Usuarios;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> GetByEmail(string email);
}
