using ArchiFlow.Application.Dashboard.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardFacade _dashboardFacade;

    public DashboardController(IDashboardFacade dashboardFacade)
    {
        _dashboardFacade = dashboardFacade;
    }

    [HttpGet("metricas")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<ActionResult<DashboardMetricasDto>> ObterMetricas()
    {
        var metricas = await _dashboardFacade.ObterMetricasAsync();
        return Ok(metricas);
    }

    [HttpGet("preferencias")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<ActionResult<PreferenciaDashboardDto>> ObterPreferencias()
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == Guid.Empty)
            return Unauthorized();

        var preferencias = await _dashboardFacade.ObterPreferenciasAsync(usuarioId);
        return Ok(preferencias);
    }

    [HttpPut("preferencias")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<ActionResult<PreferenciaDashboardDto>> SalvarPreferencias([FromBody] SalvarPreferenciaDashboardCommand command)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == Guid.Empty)
            return Unauthorized();

        var resultado = await _dashboardFacade.SalvarPreferenciasAsync(usuarioId, command);
        return Ok(resultado);
    }

    private Guid ObterUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
