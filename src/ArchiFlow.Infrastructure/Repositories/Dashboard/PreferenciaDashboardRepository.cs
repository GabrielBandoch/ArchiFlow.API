using ArchiFlow.Domain.Dashboard;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Repositories.Dashboard;

public class PreferenciaDashboardRepository : Repository<PreferenciaDashboard>, IPreferenciaDashboardRepository
{
    public PreferenciaDashboardRepository(ArchiFlowDbContext context) : base(context) { }

    public async Task<PreferenciaDashboard?> ObterPorUsuarioIdAsync(Guid usuarioId)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);
    }
}
