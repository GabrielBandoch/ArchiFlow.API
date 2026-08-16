using ArchiFlow.Application.Arquivos.Commands;
using ArchiFlow.Application.Interfaces.Facades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    {
        _facade = facade;
    }

    [HttpGet("projeto/{projetoId:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> GetByProjeto(Guid projetoId) =>
        Ok(await _facade.GetByProjetoId(projetoId));

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] Guid projetoId,
        [FromForm] bool visivelCliente)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Nenhum arquivo enviado.");
        }

        using var stream = file.OpenReadStream();
        var command = new UploadArquivoCommand(
            projetoId,
            file.FileName,
            file.ContentType,
            file.Length,
            stream,
            visivelCliente
        );

        var result = await _facade.Upload(command);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _facade.Delete(id);
        return NoContent();
    }
}
