using ArchiFlow.API.Security;
using ArchiFlow.Application.Clientes.Commands;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize]
[RequireModule(ModulosApp.Clientes)]
public class ClientesController : ControllerBase
{
    private readonly IClienteFacade _facade;
    private readonly IRepresentanteClienteFacade _representanteFacade;
    private readonly IAppUrlService _appUrlService;

    public ClientesController(
        IClienteFacade facade,
        IRepresentanteClienteFacade representanteFacade,
        IAppUrlService appUrlService)
    {
        _facade = facade;
        _representanteFacade = representanteFacade;
        _appUrlService = appUrlService;
    }

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

    [HttpGet("{id:guid}/representantes")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterRepresentantes(Guid id) =>
        Ok(await _representanteFacade.ObterPorClienteIdAsync(id));

    [HttpPost("{id:guid}/representantes")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ConvidarRepresentante(Guid id, [FromBody] ConvidarRepresentanteCommand command) =>
        Ok(await _representanteFacade.ConvidarRepresentanteAsync(id, command, ObterBaseUrl()));

    [HttpPost("representantes/{repId:guid}/reenviar-convite")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ReenviarConvite(Guid repId)
    {
        await _representanteFacade.ReenviarConviteAsync(repId, ObterBaseUrl());
        return NoContent();
    }

    [HttpPatch("representantes/{repId:guid}/status")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> AlterarStatusRepresentante(Guid repId, [FromBody] AlterarStatusRepresentanteCommand command) =>
        Ok(await _representanteFacade.AlterarStatusAsync(repId, command.Ativo));

    [HttpDelete("representantes/{repId:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> RemoverRepresentante(Guid repId)
    {
        await _representanteFacade.RemoverRepresentanteAsync(repId);
        return NoContent();
    }

    private string ObterBaseUrl() =>
        _appUrlService.ObterUrlBaseConfiavel(Request.Headers.Origin.FirstOrDefault());
}
