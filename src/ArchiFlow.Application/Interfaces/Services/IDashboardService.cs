using ArchiFlow.Application.Dashboard.DTOs;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardMetricasDto> ObterMetricasAsync();
    Task<PreferenciaDashboardDto?> ObterPreferenciasAsync();
    Task<PreferenciaDashboardDto> SalvarPreferenciasAsync(SalvarPreferenciaDashboardCommand command);
}
