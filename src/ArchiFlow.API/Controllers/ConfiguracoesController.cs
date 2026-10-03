using ArchiFlow.Application.Configuracoes.DTOs;
using ArchiFlow.Application.Configuracoes.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/configuracoes")]
[Authorize]
public class ConfiguracoesController : ControllerBase
{
    private readonly IConfiguracaoSistemaService _service;

    public ConfiguracoesController(IConfiguracaoSistemaService service) =>
        _service = service;

    [HttpGet("opcoes/{categoria}")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterPorCategoria(string categoria) =>
        Ok(await _service.ObterPorCategoriaAsync(categoria));

    [HttpGet("opcoes")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterTodas() =>
        Ok(await _service.ObterTodasAgrupadasAsync());

    [HttpPost("opcoes")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> SalvarOpcao([FromBody] SalvarOpcaoConfiguracaoCommand command) =>
        Ok(await _service.SalvarOpcaoAsync(command));
}
