using ArchiFlow.Application.Leads.Commands;
using ArchiFlow.Application.Interfaces.Facades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/leads")]
[Authorize]
public class LeadsController : ControllerBase
{
    private const string IdInconsistenteMsg = "ID inconsistente.";
    private readonly ILeadFacade _facade;

    public LeadsController(ILeadFacade facade) 
        => _facade = facade;

    [HttpGet]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> GetAll() =>
        Ok(await _facade.GetAll());

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var lead = await _facade.GetById(id);
        return lead is null ? NotFound() : Ok(lead);
    }

    [HttpPost]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> Create([FromBody] CriarLeadCommand command)
    {
        var result = await _facade.Create(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> Update(Guid id, [FromBody] AtualizarLeadCommand command)
    {
        if (id != command.Id) 
            return BadRequest(IdInconsistenteMsg);
        return Ok(await _facade.Update(command));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] AtualizarStatusLeadCommand command)
    {
        if (id != command.Id) 
            return BadRequest(IdInconsistenteMsg);
        return Ok(await _facade.UpdateStatus(command));
    }

    [HttpPost("{id:guid}/historico")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> RegisterContact(Guid id, [FromBody] RegistrarContatoLeadCommand command)
    {
        if (id != command.LeadId) 
            return BadRequest(IdInconsistenteMsg);
        return Ok(await _facade.RegisterContact(command));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ApenasAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _facade.Delete(id);
        return NoContent();
    }
}
