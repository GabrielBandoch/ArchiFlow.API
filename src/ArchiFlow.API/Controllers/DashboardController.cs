using ArchiFlow.Application.Dashboard.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardFacade _dashboardFacade;

    public DashboardController(IDashboardFacade dashboardFacade) =>
        _dashboardFacade = dashboardFacade;

    [HttpGet("metricas")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterMetricas() =>
        Ok(await _dashboardFacade.ObterMetricasAsync());

    [HttpGet("preferencias")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterPreferencias() =>
        Ok(await _dashboardFacade.ObterPreferenciasAsync());

    [HttpPut("preferencias")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> SalvarPreferencias([FromBody] SalvarPreferenciaDashboardCommand command) =>
        Ok(await _dashboardFacade.SalvarPreferenciasAsync(command));
}
