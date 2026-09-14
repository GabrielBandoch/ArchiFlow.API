using ArchiFlow.Domain.Projetos;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Repositories.Projetos;

public class ArquivoRepository : Repository<Arquivo>, IArquivoRepository
{
    public ArquivoRepository(ArchiFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<Arquivo>> GetByProjetoId(Guid projetoId, bool apenasVisiveisCliente = false)
    {
        var query = _dbSet.Where(a => a.ProjetoId == projetoId);
        if (apenasVisiveisCliente)
        {
            query = query.Where(a => a.VisivelCliente);
        }
        return await query
            .OrderByDescending(a => a.CriadoEm)
            .ToListAsync();
    }
}
