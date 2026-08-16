using ArchiFlow.Application.Clientes.Commands;
using ArchiFlow.Application.Interfaces.Facades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IClienteFacade _facade;

    public ClientesController(IClienteFacade facade) 
        => _facade = facade;

    [HttpGet]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> GetAll() =>
        Ok(await _facade.GetAll());

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var cliente = await _facade.GetById(id);
        return cliente is null ? NotFound() : Ok(cliente);
    }

    [HttpPost("convert")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ConvertLead([FromBody] ConvertLeadToClienteCommand command) =>
        Ok(await _facade.ConvertLead(command));

    [HttpPut]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> Update([FromBody] AtualizarClienteCommand command) =>
        Ok(await _facade.Update(command));

    [HttpPatch("portal-access")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> UpdatePortalAccess([FromBody] AtualizarPortalAccessCommand command) =>
        Ok(await _facade.UpdatePortalAccess(command));
}
