using ArchiFlow.API.Security;
using ArchiFlow.Application.Configuracoes.DTOs;
using ArchiFlow.Application.Configuracoes.Services;
using ArchiFlow.Domain.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/configuracoes")]
[Authorize]
[RequireModule(ModulosApp.Configuracoes)]
public class ConfiguracoesController : ControllerBase
{
    private readonly IConfiguracaoSistemaService _service;

    public ConfiguracoesController(IConfiguracaoSistemaService service)
    {
        _service = service;
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
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    public async Task<IActionResult> SalvarOpcao([FromBody] SalvarOpcaoConfiguracaoCommand command) =>
        Ok(await _service.SalvarOpcaoAsync(command));
}
