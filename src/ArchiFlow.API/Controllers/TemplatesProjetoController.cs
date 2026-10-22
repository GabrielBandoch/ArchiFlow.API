using ArchiFlow.API.Security;
using ArchiFlow.Application.Projetos.Commands;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Domain.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/templates-projeto")]
[Authorize]
[RequireModule(ModulosApp.Projetos)]
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

    [HttpGet("{codigo}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterPorCodigo(string codigo)
    {
        var template = await _facade.ObterTemplatePorCodigo(codigo);
        return template is null ? NotFound() : Ok(template);
    }

    [HttpPost]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    public async Task<IActionResult> Criar([FromBody] CriarTemplateProjetoCommand command)
    {
        var result = await _facade.CriarTemplate(command);
        return CreatedAtAction(nameof(ObterPorId), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarTemplateProjetoCommand command)
    {
        if (id != command.Id)
            return BadRequest("O ID informado na rota diverge do corpo da requisição.");

        var result = await _facade.AtualizarTemplate(command);
        return Ok(result);
    }

    [HttpPut("{codigo}")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    public async Task<IActionResult> AtualizarPorCodigo(string codigo, [FromBody] AtualizarTemplatePorCodigoCommand command)
    {
        var result = await _facade.AtualizarTemplatePorCodigo(command with { Codigo = codigo });
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        await _facade.ExcluirTemplate(id);
        return NoContent();
    }

    [HttpDelete("{codigo}")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    public async Task<IActionResult> ExcluirPorCodigo(string codigo)
    {
        await _facade.ExcluirTemplatePorCodigo(codigo);
        return NoContent();
    }
}
