using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.Application.SuperAdmin.DTOs;
using ArchiFlow.Application.SuperAdmin.Services;
using ArchiFlow.Domain.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/superadmin")]
public class SuperAdminController : ControllerBase
{
    private readonly ISuperAdminTenantService _service;

    public SuperAdminController(ISuperAdminTenantService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retorna os KPIs globais de assinaturas, MRR, débitos e adimplência da plataforma COTU.
    /// </summary>
    [HttpGet("kpis")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<SuperAdminDashboardKpisDto>> ObterKpis()
    {
        var kpis = await _service.ObterKpisAsync();
        return Ok(kpis);
    }

    /// <summary>
    /// Lista todos os escritórios assinantes da COTU, com filtros por busca e status financeiro.
    /// </summary>
    [HttpGet("escritorios")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<IEnumerable<TenantAssinaturaDto>>> ListarEscritorios(
        [FromQuery] string? busca = null,
        [FromQuery] string? status = null)
    {
        var escritorios = await _service.ListarEscritoriosAsync(busca, status);
        return Ok(escritorios);
    }

    /// <summary>
    /// Retorna os detalhes de um escritório assinante específico.
    /// </summary>
    [HttpGet("escritorios/{id:guid}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<TenantAssinaturaDto>> ObterEscritorio(Guid id)
    {
        var escritorio = await _service.ObterPorIdAsync(id);
        if (escritorio == null)
        {
            return NotFound(new { message = "Escritório não encontrado." });
        }
        return Ok(escritorio);
    }

    /// <summary>
    /// Cadastra e provisiona um novo escritório assinante com banco de dados isolado.
    /// </summary>
    [HttpPost("escritorios")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<TenantAssinaturaDto>> CriarEscritorio([FromBody] CriarTenantAssinaturaRequestDto dto)
    {
        try
        {
            var criado = await _service.CriarEscritorioAsync(dto);
            return CreatedAtAction(nameof(ObterEscritorio), new { id = criado.Id }, criado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza o status financeiro de uma assinatura (Adimplente, EmDébito, Suspenso, Trial).
    /// </summary>
    [HttpPut("escritorios/{id:guid}/status-financeiro")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<TenantAssinaturaDto>> AtualizarStatusFinanceiro(
        Guid id,
        [FromBody] AtualizarStatusFinanceiroDto dto)
    {
        try
        {
            var atualizado = await _service.AtualizarStatusFinanceiroAsync(id, dto);
            return Ok(atualizado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Bloqueia ou desbloqueia o acesso de um escritório à plataforma por inadimplência.
    /// </summary>
    [HttpPut("escritorios/{id:guid}/bloquear")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<TenantAssinaturaDto>> AlternarBloqueio(
        Guid id,
        [FromBody] BloquearTenantRequestDto dto)
    {
        try
        {
            var atualizado = await _service.AlternarBloqueioAcessoAsync(id, dto);
            return Ok(atualizado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza anotações internas da equipe COTU sobre o cliente.
    /// </summary>
    [HttpPut("escritorios/{id:guid}/notas")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<TenantAssinaturaDto>> AtualizarNotas(
        Guid id,
        [FromBody] AtualizarNotasRequestDto dto)
    {
        try
        {
            var atualizado = await _service.AtualizarNotasInternasAsync(id, dto);
            return Ok(atualizado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Ação rápida: registra que a mensalidade foi paga hoje, estende o vencimento e desbloqueia o acesso se estava suspenso.
    /// </summary>
    [HttpPost("escritorios/{id:guid}/registrar-pagamento")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<TenantAssinaturaDto>> RegistrarPagamento(Guid id)
    {
        try
        {
            var atualizado = await _service.RegistrarPagamentoAsync(id);
            return Ok(atualizado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Endpoint público para checagem rápida de status de acesso do tenant (usado no login/middleware).
    /// </summary>
    [HttpGet("status-acesso/{subdominio}")]
    [AllowAnonymous]
    public async Task<IActionResult> VerificarStatusAcesso(string subdominio)
    {
        var (bloqueado, motivo) = await _service.VerificarStatusAcessoAsync(subdominio);
        return Ok(new { bloqueado, motivo });
    }
}
