using ArchiFlow.Application.Dashboard.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Dashboard.Facades;

public class DashboardFacade : IDashboardFacade
{
    private readonly IDashboardService _dashboardService;

    public DashboardFacade(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public Task<DashboardMetricasDto> ObterMetricasAsync()
    {
        return _dashboardService.ObterMetricasAsync();
    }

    public Task<PreferenciaDashboardDto?> ObterPreferenciasAsync()
    {
        return _dashboardService.ObterPreferenciasAsync();
    }

    public Task<PreferenciaDashboardDto?> ObterPreferenciasAsync(Guid usuarioId)
    {
        return _dashboardService.ObterPreferenciasAsync(usuarioId);
    }

    public Task<PreferenciaDashboardDto> SalvarPreferenciasAsync(SalvarPreferenciaDashboardCommand command)
    {
        return _dashboardService.SalvarPreferenciasAsync(command);
    }

    public Task<PreferenciaDashboardDto> SalvarPreferenciasAsync(Guid usuarioId, SalvarPreferenciaDashboardCommand command)
    {
        return _dashboardService.SalvarPreferenciasAsync(usuarioId, command);
    }
}
