using System;
using System.Threading.Tasks;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Usuarios.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioFacade _facade;

    public UsuariosController(IUsuarioFacade facade)
    {
        _facade = facade;
    }

    [HttpGet("equipe")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterEquipe() =>
        Ok(await _facade.ObterEquipeAsync());

    [HttpGet("equipe/{id:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterMembroPorId(Guid id)
    {
        var membro = await _facade.ObterMembroPorIdAsync(id);
        return membro is null ? NotFound() : Ok(membro);
    }

    [HttpPost("equipe")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    public async Task<IActionResult> ConvidarMembro([FromBody] ConvidarMembroEquipeCommand command)
    {
        var membro = await _facade.ConvidarMembroAsync(command);
        return CreatedAtAction(nameof(ObterMembroPorId), new { id = membro.Id }, membro);
    }

    [HttpPut("equipe/{id:guid}")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    public async Task<IActionResult> AtualizarMembro(Guid id, [FromBody] AtualizarMembroEquipeCommand command) =>
        Ok(await _facade.AtualizarMembroAsync(id, command));

    [HttpPatch("equipe/{id:guid}/status")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    public async Task<IActionResult> AlterarStatusMembro(Guid id, [FromBody] AlterarStatusMembroCommand command) =>
        Ok(await _facade.AlterarStatusMembroAsync(id, command));

    [HttpPost("equipe/{id:guid}/redefinir-senha")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    public async Task<IActionResult> RedefinirSenhaMembro(Guid id, [FromBody] RedefinirSenhaMembroCommand command)
    {
        await _facade.RedefinirSenhaMembroAsync(id, command);
        return NoContent();
    }

    [HttpDelete("equipe/{id:guid}")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    public async Task<IActionResult> ExcluirMembro(Guid id)
    {
        await _facade.ExcluirMembroAsync(id);
        return NoContent();
    }
}
