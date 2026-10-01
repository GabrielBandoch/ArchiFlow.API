using ArchiFlow.Application.Financeiro.Commands;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Domain.Financeiro;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/financeiro")]
[Authorize]
public class FinanceiroController : ControllerBase
{
    private readonly IFinanceiroFacade _facade;

    public FinanceiroController(IFinanceiroFacade facade) =>
        _facade = facade;

    [HttpGet("painel")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterPainel() =>
        Ok(await _facade.ObterPainelConsolidadoAsync());

    [HttpGet("parcelas")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterParcelas(
        [FromQuery] Guid? projetoId,
        [FromQuery] StatusParcela? status,
        [FromQuery] DateTime? inicio,
        [FromQuery] DateTime? fim) =>
        Ok(await _facade.ObterParcelasAsync(projetoId, status, inicio, fim));

    [HttpGet("parcelas/{id:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterParcelaPorId(Guid id)
    {
        var parcela = await _facade.ObterParcelaPorIdAsync(id);
        return parcela is null ? NotFound() : Ok(parcela);
    }

    [HttpPost("contratos")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> CriarContrato([FromBody] CriarContratoCommand command) =>
        Ok(await _facade.CriarContratoAsync(command));

    [HttpGet("contratos/projeto/{projetoId:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterContratoPorProjeto(Guid projetoId)
    {
        var contrato = await _facade.ObterContratoPorProjetoIdAsync(projetoId);
        return contrato is null ? NotFound() : Ok(contrato);
    }

    [HttpPost("parcelas")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> RegistrarParcela([FromBody] CriarParcelaCommand command) =>
        Ok(await _facade.RegistrarParcelaAsync(command));

    [HttpPut("parcelas/{id:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> AtualizarParcela(Guid id, [FromBody] AtualizarParcelaCommand command) =>
        Ok(await _facade.AtualizarParcelaAsync(id, command));

    [HttpPatch("parcelas/{id:guid}/baixa")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> DarBaixaParcela(Guid id, [FromBody] DarBaixaParcelaCommand command) =>
        Ok(await _facade.DarBaixaParcelaAsync(id, command));

    [HttpDelete("parcelas/{id:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ExcluirParcela(Guid id)
    {
        await _facade.ExcluirParcelaAsync(id);
        return NoContent();
    }

    [HttpGet("despesas")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterDespesas(
        [FromQuery] Guid? projetoId,
        [FromQuery] CategoriaDespesa? categoria,
        [FromQuery] DateTime? inicio,
        [FromQuery] DateTime? fim) =>
        Ok(await _facade.ObterDespesasAsync(projetoId, categoria, inicio, fim));

    [HttpGet("despesas/{id:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterDespesaPorId(Guid id)
    {
        var despesa = await _facade.ObterDespesaPorIdAsync(id);
        return despesa is null ? NotFound() : Ok(despesa);
    }

    [HttpPost("despesas")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> CriarDespesa([FromBody] CriarDespesaCommand command) =>
        Ok(await _facade.CriarDespesaAsync(command));

    [HttpPut("despesas/{id:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> AtualizarDespesa(Guid id, [FromBody] AtualizarDespesaCommand command) =>
        Ok(await _facade.AtualizarDespesaAsync(id, command));

    [HttpDelete("despesas/{id:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ExcluirDespesa(Guid id)
    {
        await _facade.ExcluirDespesaAsync(id);
        return NoContent();
    }

    [HttpGet("alertas")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterAlertas() =>
        Ok(await _facade.ObterAlertasAsync());

    [HttpPost("upload-comprovante")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> UploadComprovante([FromForm] UploadComprovanteCommand command) =>
        Ok(await _facade.UploadComprovanteAsync(command));

    [HttpDelete("comprovante")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ExcluirComprovante([FromQuery] string url)
    {
        await _facade.ExcluirComprovanteAsync(url);
        return NoContent();
    }
}
