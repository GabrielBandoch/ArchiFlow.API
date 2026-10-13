using ArchiFlow.Application.Configuracoes.DTOs;
using ArchiFlow.Application.Configuracoes.Services;
using ArchiFlow.Application.Honorarios.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
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
    private readonly IPropostaHonorarioFacade _propostaFacade;

    public ConfiguracoesController(
        IConfiguracaoSistemaService service,
        IPropostaHonorarioFacade propostaFacade)
    {
        _service = service;
        _propostaFacade = propostaFacade;
    }

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

    [HttpGet("empresa")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterConfiguracaoEmpresa() =>
        Ok(await _propostaFacade.ObterConfiguracaoAsync());

    [HttpPut("empresa")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> SalvarConfiguracaoEmpresa([FromBody] SalvarConfiguracaoPropostaCommand command) =>
        Ok(await _propostaFacade.SalvarConfiguracaoAsync(command));
}
