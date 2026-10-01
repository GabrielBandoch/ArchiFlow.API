using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.Domain.Shared;

namespace ArchiFlow.Domain.Usuarios;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> GetByEmail(string email);
    Task<IEnumerable<Usuario>> ObterPorEscritorioIdAsync(Guid escritorioId);
}
