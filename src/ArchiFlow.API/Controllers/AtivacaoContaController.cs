using ArchiFlow.Application.Clientes.Commands;
using ArchiFlow.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AtivacaoContaController : ControllerBase
{
    private readonly IRepresentanteClienteService _representanteClienteService;

    public AtivacaoContaController(IRepresentanteClienteService representanteClienteService)
    {
        _representanteClienteService = representanteClienteService;
    }

    [HttpGet("convite/{token}")]
    public async Task<IActionResult> ValidarTokenConvite(string token) =>
        Ok(await _representanteClienteService.ValidarTokenConviteAsync(token));

    [HttpPost("ativar-conta")]
    public async Task<IActionResult> AtivarConta([FromBody] AtivarContaRepresentanteCommand command)
    {
        try
        {
            await _representanteClienteService.AtivarContaAsync(command);
            return Ok(new { message = "Conta ativada com sucesso! Você já pode acessar o Portal do Cliente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
