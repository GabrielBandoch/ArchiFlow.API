using ArchiFlow.Application.Arquivos.Commands;
using ArchiFlow.Application.Interfaces.Facades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/arquivos")]
[Authorize]
public class ArquivosController : ControllerBase
{
    private readonly IArquivoFacade _facade;

    public ArquivosController(IArquivoFacade facade) 
        => _facade = facade;

    [HttpGet("projeto/{projetoId:guid}")]
    [Authorize(Policy = "ProjetoOwner")]
    public async Task<IActionResult> GetByProjeto(Guid projetoId)
    {
        var isClient = User?.IsInRole("Cliente") == true || User?.FindFirst("user_type")?.Value == "client";
        return Ok(await _facade.GetByProjetoId(projetoId, isClient));
    }

    [HttpPost("upload")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> Upload([FromForm] UploadArquivoCommand command) =>
        Ok(await _facade.Upload(command));

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _facade.Delete(id);
        return NoContent();
    }
}
