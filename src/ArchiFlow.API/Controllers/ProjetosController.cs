using ArchiFlow.Application.Projetos.Commands;
using ArchiFlow.Application.Interfaces.Facades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/projetos")]
[Authorize]
public class ProjetosController : ControllerBase
{
    private const string IdInconsistenteMsg = "ID inconsistente.";
    private readonly IProjetoFacade _facade;

    public ProjetosController(IProjetoFacade facade) 
        => _facade = facade;

    [HttpGet]
    [Authorize(Policy = "AcessoArquiteto")]
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
    public async Task<IActionResult> Create([FromBody] CriarProjetoCommand command)
    {
        var result = await _facade.Create(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] AtualizarProjetoCommand command)
    {
        if (id != command.Id) 
            return BadRequest(IdInconsistenteMsg);
        return Ok(await _facade.Update(command));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] AtualizarStatusProjetoCommand command)
    {
        if (id != command.Id) 
            return BadRequest(IdInconsistenteMsg);
        return Ok(await _facade.UpdateStatus(command));
    }

    [HttpPost("{id:guid}/etapas")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> CreateEtapa(Guid id, [FromBody] CriarEtapaCommand command)
    {
        if (id != command.ProjetoId) 
            return BadRequest(IdInconsistenteMsg);
        return Ok(await _facade.CreateEtapa(command));
    }

    [HttpPatch("etapas/{etapaId:guid}/status")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> UpdateStatusEtapa(Guid etapaId, [FromBody] AtualizarStatusEtapaCommand command)
    {
        if (etapaId != command.EtapaId) 
            return BadRequest(IdInconsistenteMsg);
        return Ok(await _facade.UpdateStatusEtapa(command));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ApenasAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _facade.Delete(id);
        return NoContent();
    }
}
