using ArchiFlow.Application.Fornecedores.Commands;
using ArchiFlow.Application.Fornecedores.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/fornecedores")]
[Authorize]
public class FornecedoresController : ControllerBase
{
    private const string IdInconsistenteMsg = "O ID informado na rota diverge do corpo da requisição.";
    private readonly IFornecedorService _service;

    public FornecedoresController(IFornecedorService service) =>
        _service = service;

    [HttpGet]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterTodos([FromQuery] string? especialidade) =>
        Ok(await _service.ObterTodosAsync(especialidade));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var result = await _service.ObterPorIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> Criar([FromBody] CriarFornecedorCommand command)
    {
        var result = await _service.CriarAsync(command);
        return CreatedAtAction(nameof(ObterPorId), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarFornecedorCommand command)
    {
        if (id != command.Id)
            return BadRequest(IdInconsistenteMsg);

        var result = await _service.AtualizarAsync(command);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var removido = await _service.ExcluirAsync(id);
        return removido ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/avaliacoes")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> AdicionarAvaliacao(Guid id, [FromBody] AdicionarAvaliacaoCommand command)
    {
        command.FornecedorId = id;
        var result = await _service.AdicionarAvaliacaoAsync(command);
        return Created(string.Empty, result);
    }

    [HttpPost("{id:guid}/projetos")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> VincularProjeto(Guid id, [FromBody] VincularProjetoCommand command)
    {
        command.FornecedorId = id;
        var result = await _service.VincularProjetoAsync(command);
        return Created(string.Empty, result);
    }

    [HttpDelete("projetos/{vinculoId:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> DesvincularProjeto(Guid vinculoId)
    {
        var result = await _service.DesvincularProjetoAsync(vinculoId);
        return result ? NoContent() : NotFound();
    }

    [HttpGet("projeto/{projetoId:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterFornecedoresDoProjeto(Guid projetoId) =>
        Ok(await _service.ObterFornecedoresDoProjetoAsync(projetoId));
}
