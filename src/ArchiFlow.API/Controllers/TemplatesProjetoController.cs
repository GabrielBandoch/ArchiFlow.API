using ArchiFlow.Application.Projetos.Commands;
using ArchiFlow.Application.Interfaces.Facades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/templates-projeto")]
[Authorize]
public class TemplatesProjetoController : ControllerBase
{
    private readonly IProjetoFacade _facade;

    public TemplatesProjetoController(IProjetoFacade facade)
    {
        _facade = facade;
    }

    [HttpGet]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterTemplates()
    {
        return Ok(await _facade.ObterTemplates());
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var template = await _facade.ObterTemplatePorId(id);
        return template is null ? NotFound() : Ok(template);
    }

    [HttpPost]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    public async Task<IActionResult> Criar([FromBody] CriarTemplateProjetoCommand command)
    {
        var result = await _facade.CriarTemplate(command);
        return CreatedAtAction(nameof(ObterPorId), new { id = result.Id }, result);
    }
}
