using ArchiFlow.API.Security;
using ArchiFlow.Application.Honorarios.Commands;
using ArchiFlow.Application.Honorarios.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Domain.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/propostas")]
[Authorize]
public class PropostasController : ControllerBase
{
    private readonly IPropostaHonorarioFacade _facade;

    public PropostasController(IPropostaHonorarioFacade facade)
    {
        _facade = facade;
    }

    [HttpGet]
    [Authorize(Policy = "AcessoArquiteto")]
    [RequireModule(ModulosApp.Simulador)]
    public async Task<IActionResult> GetAll() =>
        Ok(await _facade.GetAll());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var proposta = await _facade.GetById(id);
        return proposta is null ? NotFound() : Ok(proposta);
    }

    [HttpGet("cliente/{clienteId:guid}")]
    public async Task<IActionResult> GetByCliente(Guid clienteId)
    {
        var userType = User.FindFirst("user_type")?.Value;
        if (userType == "client" || User.IsInRole("Cliente"))
        {
            var claimClienteId = User.FindFirst("cliente_id")?.Value;
            if (claimClienteId != clienteId.ToString())
            {
                return Forbid();
            }
        }
        return Ok(await _facade.GetByClienteId(clienteId));
    }

    [HttpGet("lead/{leadId:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    [RequireModule(ModulosApp.Simulador)]
    public async Task<IActionResult> GetByLead(Guid leadId) =>
        Ok(await _facade.GetByLeadId(leadId));

    [HttpPost]
    [Authorize(Policy = "AcessoArquiteto")]
    [RequireModule(ModulosApp.Simulador)]
    public async Task<IActionResult> Criar([FromBody] CriarPropostaCommand command)
    {
        var proposta = await _facade.Criar(command);
        return CreatedAtAction(nameof(GetById), new { id = proposta.Id }, proposta);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = "AcessoArquiteto")]
    [RequireModule(ModulosApp.Simulador)]
    public async Task<IActionResult> AtualizarStatus(Guid id, [FromBody] AtualizarStatusPropostaCommand command) =>
        Ok(await _facade.AtualizarStatus(id, command));

    [HttpPut("{id:guid}/ajustar-valor")]
    [Authorize(Policy = "AcessoArquiteto")]
    [RequireModule(ModulosApp.Simulador)]
    public async Task<IActionResult> AjustarValor(Guid id, [FromBody] AjustarValorPropostaCommand command) =>
        Ok(await _facade.AjustarValor(id, command));

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ApenasAdmin")]
    [RequireModule(ModulosApp.Simulador)]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var sucesso = await _facade.Excluir(id);
        return !sucesso ? NotFound() : NoContent();
    }

    [HttpGet("configuracao")]
    [Authorize(Policy = "AcessoArquiteto")]
    [RequireModule(ModulosApp.Simulador)]
    public async Task<IActionResult> ObterConfiguracao() =>
        Ok(await _facade.ObterConfiguracaoAsync());

    [HttpPut("configuracao")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    [RequireModule(ModulosApp.Simulador)]
    public async Task<IActionResult> SalvarConfiguracao([FromBody] SalvarConfiguracaoPropostaCommand command) =>
        Ok(await _facade.SalvarConfiguracaoAsync(command));
}
