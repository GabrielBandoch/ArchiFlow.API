using System;
using System.Threading.Tasks;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Application.SuperAdmin.DTOs;
using ArchiFlow.Domain.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/superadmin")]
[Authorize(Roles = $"{Roles.Administrador},{Roles.AdministradorGeral}")]
public class SuperAdminController : ControllerBase
{
    private readonly ISuperAdminService _superAdminService;

    public SuperAdminController(ISuperAdminService superAdminService)
    {
        _superAdminService = superAdminService;
    }

    [HttpGet("kpis")]
    public async Task<IActionResult> ObterKpis()
    {
        var result = await _superAdminService.ObterKpisAsync();
        return Ok(result);
    }

    [HttpGet("escritorios")]
    public async Task<IActionResult> ListarEscritorios([FromQuery] string? busca, [FromQuery] string? status)
    {
        var result = await _superAdminService.ListarEscritoriosAsync(busca, status);
        return Ok(result);
    }

    [HttpGet("escritorios/{id:guid}")]
    public async Task<IActionResult> ObterEscritorio(Guid id)
    {
        var result = await _superAdminService.ObterEscritorioPorIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("escritorios")]
    public async Task<IActionResult> CriarEscritorio([FromBody] CriarTenantRequestDto request)
    {
        try
        {
            var result = await _superAdminService.CriarEscritorioAsync(request);
            return CreatedAtAction(nameof(ObterEscritorio), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpPut("escritorios/{id:guid}/status-financeiro")]
    public async Task<IActionResult> AtualizarStatusFinanceiro(Guid id, [FromBody] AtualizarStatusFinanceiroRequestDto request)
    {
        var result = await _superAdminService.AtualizarStatusFinanceiroAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("escritorios/{id:guid}/bloquear")]
    public async Task<IActionResult> AlternarBloqueio(Guid id, [FromBody] BloquearTenantRequestDto request)
    {
        var result = await _superAdminService.AlternarBloqueioAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("escritorios/{id:guid}/notas")]
    public async Task<IActionResult> AtualizarNotas(Guid id, [FromBody] AtualizarNotasRequestDto request)
    {
        var result = await _superAdminService.AtualizarNotasAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("escritorios/{id:guid}/registrar-pagamento")]
    public async Task<IActionResult> RegistrarPagamento(Guid id)
    {
        var result = await _superAdminService.RegistrarPagamentoAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
}
