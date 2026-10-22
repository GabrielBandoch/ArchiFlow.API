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

public class AtualizarTemplatePorCodigoRequest
{
    public object? Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Descricao { get; set; }
    public string? Icone { get; set; }
    public List<CriarTemplateEtapaItemCommand>? Etapas { get; set; }
}

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
    public async Task<IActionResult> AtualizarPorCodigo(string codigo, [FromBody] AtualizarTemplatePorCodigoRequest request)
    {
        var template = await _facade.ObterTemplatePorCodigo(codigo);
        if (template is null)
        {
            var criarCommand = new CriarTemplateProjetoCommand(
                Codigo: codigo,
                Nome: request.Nome,
                Descricao: request.Descricao,
                Icone: request.Icone,
                Etapas: request.Etapas
            );
            var criado = await _facade.CriarTemplate(criarCommand);
            return Ok(criado);
        }

        var command = new AtualizarTemplateProjetoCommand(
            Id: template.Id,
            Nome: request.Nome,
            Descricao: request.Descricao,
            Icone: request.Icone,
            Etapas: request.Etapas
        );
        var result = await _facade.AtualizarTemplate(command);
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
        var template = await _facade.ObterTemplatePorCodigo(codigo);
        if (template is not null)
        {
            await _facade.ExcluirTemplate(template.Id);
        }
        return NoContent();
    }
}
