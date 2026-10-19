using ArchiFlow.API.Security;
using ArchiFlow.Application.Clientes.Commands;
using ArchiFlow.Application.Clientes.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Application.Projetos.Commands;
using ArchiFlow.Application.Projetos.DTOs;
using ArchiFlow.Domain.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/projetos")]
[Authorize]
public class ProjetosController : ControllerBase
{
    private const string IdInconsistenteMsg = "ID inconsistente.";
    private readonly IProjetoFacade _facade;
    private readonly IRepresentanteClienteFacade _representanteFacade;
    private readonly IAppUrlService _appUrlService;

    public ProjetosController(
        IProjetoFacade facade,
        IRepresentanteClienteFacade representanteFacade,
        IAppUrlService appUrlService)
    {
        _facade = facade;
        _representanteFacade = representanteFacade;
        _appUrlService = appUrlService;
    }

    [HttpGet("meus-projetos")]
    public async Task<IActionResult> ObterMeusProjetos()
    {
        var userType = User.FindFirst("user_type")?.Value;
        if (userType == "client" || User.IsInRole("Cliente"))
        {
            var clienteIdClaim = User.FindFirst("cliente_id")?.Value;
            if (Guid.TryParse(clienteIdClaim, out var clienteId))
            {
                var projetos = await _facade.GetByClienteId(clienteId);
                return Ok(projetos);
            }
            return Ok(Array.Empty<ProjetoDto>());
        }

        return Ok(await _facade.GetAll());
    }

    [HttpGet]
    [Authorize(Policy = "AcessoArquiteto")]
    [RequireModule(ModulosApp.Projetos)]
    public async Task<IActionResult> GetAll() =>
        Ok(await _facade.GetAll());

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "ProjetoOwner")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var projeto = await _facade.GetById(id);
        return projeto is null ? NotFound() : Ok(projeto);
    }

    [HttpPost]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    [RequireModule(ModulosApp.Projetos)]
    public async Task<IActionResult> Create([FromBody] CriarProjetoCommand command)
    {
        var result = await _facade.Create(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    [RequireModule(ModulosApp.Projetos)]
    public async Task<IActionResult> Update(Guid id, [FromBody] AtualizarProjetoCommand command)
    {
        if (id != command.Id) 
            return BadRequest(IdInconsistenteMsg);
        return Ok(await _facade.Update(command));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "AcessoArquiteto")]
    [RequireModule(ModulosApp.Projetos)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] AtualizarStatusProjetoCommand command)
    {
        if (id != command.Id) 
            return BadRequest(IdInconsistenteMsg);
        return Ok(await _facade.UpdateStatus(command));
    }

    [HttpPost("{id:guid}/etapas")]
    [Authorize(Policy = "AcessoArquiteto")]
    [RequireModule(ModulosApp.Projetos)]
    public async Task<IActionResult> CreateEtapa(Guid id, [FromBody] CriarEtapaCommand command)
    {
        if (id != command.ProjetoId) 
            return BadRequest(IdInconsistenteMsg);
        return Ok(await _facade.CreateEtapa(command));
    }

    [HttpPatch("etapas/{etapaId:guid}/status")]
    [Authorize(Policy = "AcessoArquiteto")]
    [RequireModule(ModulosApp.Projetos)]
    public async Task<IActionResult> UpdateStatusEtapa(Guid etapaId, [FromBody] AtualizarStatusEtapaCommand command)
    {
        if (etapaId != command.EtapaId) 
            return BadRequest(IdInconsistenteMsg);
        return Ok(await _facade.UpdateStatusEtapa(command));
    }

    [HttpPost("etapas/{etapaId:guid}/tarefas")]
    [Authorize(Policy = "AcessoArquiteto")]
    [RequireModule(ModulosApp.Projetos)]
    public async Task<IActionResult> AdicionarTarefa(Guid etapaId, [FromBody] AdicionarTarefaCommand command)
    {
        if (etapaId != command.EtapaId)
            return BadRequest(IdInconsistenteMsg);
        return Ok(await _facade.AdicionarTarefa(command));
    }

    [HttpPatch("etapas/tarefas/{tarefaId:guid}/toggle")]
    [Authorize(Policy = "AcessoArquiteto")]
    [RequireModule(ModulosApp.Projetos)]
    public async Task<IActionResult> AlternarTarefa(Guid tarefaId)
    {
        return Ok(await _facade.AlternarTarefa(tarefaId));
    }

    [HttpDelete("etapas/tarefas/{tarefaId:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    [RequireModule(ModulosApp.Projetos)]
    public async Task<IActionResult> RemoverTarefa(Guid tarefaId)
    {
        await _facade.RemoverTarefa(tarefaId);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ApenasAdmin")]
    [RequireModule(ModulosApp.Projetos)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _facade.Delete(id);
        return NoContent();
    }

    [HttpGet("{id:guid}/representantes")]
    [Authorize(Policy = "ProjetoOwner")]
    public async Task<IActionResult> ObterRepresentantes(Guid id)
    {
        var projeto = await _facade.GetById(id);
        if (projeto is null) return NotFound();
        if (projeto.ClienteId == Guid.Empty) return Ok(Array.Empty<RepresentanteClienteDto>());
        return Ok(await _representanteFacade.ObterPorClienteIdAsync(projeto.ClienteId));
    }

    [HttpPost("{id:guid}/representantes")]
    [Authorize(Policy = "ProjetoOwner")]
    public async Task<IActionResult> ConvidarRepresentante(Guid id, [FromBody] ConvidarRepresentanteCommand command)
    {
        var projeto = await _facade.GetById(id);
        if (projeto is null) return NotFound();
        if (projeto.ClienteId == Guid.Empty) return BadRequest("O projeto não possui um cliente vinculado.");

        var rep = await _representanteFacade.ConvidarRepresentanteAsync(projeto.ClienteId, command, ObterBaseUrl());
        return Ok(rep);
    }

    [HttpPost("{id:guid}/representantes/{repId:guid}/reenviar-convite")]
    [Authorize(Policy = "ProjetoOwner")]
    public async Task<IActionResult> ReenviarConvite(Guid id, Guid repId)
    {
        await _representanteFacade.ReenviarConviteAsync(repId, ObterBaseUrl());
        return NoContent();
    }

    private string ObterBaseUrl() =>
        _appUrlService.ObterUrlBaseConfiavel(Request?.Headers.Origin.FirstOrDefault());
}
