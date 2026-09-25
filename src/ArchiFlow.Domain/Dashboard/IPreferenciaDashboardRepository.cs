using ArchiFlow.Domain.Shared;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.Domain.Dashboard;

public interface IPreferenciaDashboardRepository : IRepository<PreferenciaDashboard>
{
    Task<PreferenciaDashboard?> ObterPorUsuarioIdAsync(Guid usuarioId);
}
