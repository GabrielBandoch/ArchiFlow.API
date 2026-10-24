using ArchiFlow.Application.Autenticacao.Commands;
using ArchiFlow.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/auth")]
public class RecuperacaoSenhaController : ControllerBase
{
    private readonly IPasswordRecoveryService _passwordRecoveryService;
    private readonly IAppUrlService _appUrlService;
    private readonly ILogger<RecuperacaoSenhaController> _logger;

    public RecuperacaoSenhaController(
        IPasswordRecoveryService passwordRecoveryService,
        IAppUrlService appUrlService,
        ILogger<RecuperacaoSenhaController> logger)
    {
        _passwordRecoveryService = passwordRecoveryService;
        _appUrlService = appUrlService;
        _logger = logger;
    }

    [HttpPost("recuperar-senha")]
    public async Task<IActionResult> SolicitarRecuperacaoSenha([FromBody] SolicitarRecuperacaoCommand command)
    {
        try
        {
            var baseUrl = _appUrlService.ObterUrlBaseConfiavel(Request.Headers.Origin.FirstOrDefault());
            await _passwordRecoveryService.SolicitarRecuperacaoAsync(command.Email, baseUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao processar solicitação de recuperação de senha.");
        }

        return Ok(new { message = "Se o e-mail informado estiver cadastrado, as instruções para redefinição de senha foram enviadas." });
    }

    [HttpPost("validar-token")]
    public async Task<IActionResult> ValidarTokenRecuperacao([FromBody] ValidarTokenRecuperacaoCommand command) =>
        Ok(await _passwordRecoveryService.ValidarTokenAsync(command.Token));

    [HttpPost("redefinir-senha")]
    public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaCommand command)
    {
        try
        {
            await _passwordRecoveryService.RedefinirSenhaAsync(command.Token, command.NovaSenha);
            return Ok(new { message = "Senha redefinida com sucesso. Faça login com suas novas credenciais." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

