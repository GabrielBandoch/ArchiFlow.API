using ArchiFlow.Application.Dashboard.DTOs;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Interfaces.Facades;

public interface IDashboardFacade
{
    Task<DashboardMetricasDto> ObterMetricasAsync();
    Task<PreferenciaDashboardDto?> ObterPreferenciasAsync();
    Task<PreferenciaDashboardDto?> ObterPreferenciasAsync(Guid usuarioId);
    Task<PreferenciaDashboardDto> SalvarPreferenciasAsync(SalvarPreferenciaDashboardCommand command);
    Task<PreferenciaDashboardDto> SalvarPreferenciasAsync(Guid usuarioId, SalvarPreferenciaDashboardCommand command);
}
