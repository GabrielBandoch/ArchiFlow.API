using ArchiFlow.Domain.Clientes;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Repositories.Clientes;

public class ClienteRepository : Repository<Cliente>, IClienteRepository
{
    public ClienteRepository(ArchiFlowDbContext context) : base(context) { }

    public async Task<Cliente?> GetByEmail(string email)
    {
        var normalizedEmail = email.Trim().ToLower();
        return await _dbSet.FirstOrDefaultAsync(c => c.Email.ToLower() == normalizedEmail);
    }
}
