using ArchiFlow.Domain.Usuarios;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Repositories.Usuarios;

public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(ArchiFlowDbContext context) : base(context) { }

    public async Task<Usuario?> GetByEmail(string email)
    {
        var normalizedEmail = email.Trim();
        return await _dbSet.FirstOrDefaultAsync(u => string.Equals(u.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<Usuario>> ObterPorEscritorioIdAsync(Guid escritorioId)
    {
        return await _dbSet
            .Where(u => u.EscritorioId == escritorioId || u.Id == escritorioId)
            .OrderBy(u => u.Nome)
            .ToListAsync();
    }
}

