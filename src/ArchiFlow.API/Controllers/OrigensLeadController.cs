using ArchiFlow.Application.Leads.Commands;
using ArchiFlow.Application.Interfaces.Facades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/origens-lead")]
[Authorize]
public class OrigensLeadController : ControllerBase
{
    private const string IdInconsistenteMsg = "ID inconsistente.";
    private readonly IOrigemLeadFacade _facade;

    public OrigensLeadController(IOrigemLeadFacade facade)
        => _facade = facade;

    [HttpGet]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> GetAll() =>
        Ok(await _facade.GetAll());

    [HttpGet("ativas")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> GetAllActive() =>
        Ok(await _facade.GetAllActive());

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var origem = await _facade.GetById(id);
        return origem is null ? NotFound() : Ok(origem);
    }

    [HttpPost]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> Create([FromBody] CriarOrigemLeadCommand command)
    {
        var result = await _facade.Create(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> Update(Guid id, [FromBody] AtualizarOrigemLeadCommand command)
    {
        if (id != command.Id)
            return BadRequest(IdInconsistenteMsg);
        return Ok(await _facade.Update(command));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> Desativar(Guid id)
    {
        var result = await _facade.Desativar(id);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reativar")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> Reativar(Guid id)
    {
        var result = await _facade.Reativar(id);
        return Ok(result);
    }
}
