using ArchiFlow.Application.Autenticacao.Commands;
using ArchiFlow.Application.Clientes.Commands;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Application.Usuarios.Commands;
using ArchiFlow.Application.Usuarios.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AutenticacaoController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IPasswordRecoveryService _passwordRecoveryService;
    private readonly IRepresentanteClienteService _representanteClienteService;

    public AutenticacaoController(IAuthService authService)
        : this(authService, null!, null!)
    {
    }

    public AutenticacaoController(
        IAuthService authService,
        IPasswordRecoveryService passwordRecoveryService,
        IRepresentanteClienteService representanteClienteService)
    {
        _authService = authService;
        _passwordRecoveryService = passwordRecoveryService;
        _representanteClienteService = representanteClienteService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var response = await _authService.Login(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var userId = await _authService.Registrar(request);
            return Created(string.Empty, new { id = userId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("recuperar-senha")]
    public async Task<IActionResult> SolicitarRecuperacaoSenha([FromBody] SolicitarRecuperacaoCommand command)
    {
        try
        {
            var baseUrl = ObterBaseUrl();
            await _passwordRecoveryService.SolicitarRecuperacaoAsync(command.Email, baseUrl);
        }
        catch
        {
        }

        return Ok(new { message = "Se o e-mail informado estiver cadastrado, as instruções para redefinição de senha foram enviadas." });
    }

    [HttpPost("validar-token")]
    public async Task<IActionResult> ValidarTokenRecuperacao([FromBody] ValidarTokenRecuperacaoCommand command)
    {
        var resultado = await _passwordRecoveryService.ValidarTokenAsync(command.Token);
        return Ok(resultado);
    }

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

    [HttpGet("convite/{token}")]
    public async Task<IActionResult> ValidarTokenConvite(string token)
    {
        var resultado = await _representanteClienteService.ValidarTokenConviteAsync(token);
        return Ok(resultado);
    }

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

    private string ObterBaseUrl()
    {
        var origin = Request.Headers["Origin"].ToString();
        return !string.IsNullOrWhiteSpace(origin) ? origin : $"{Request.Scheme}://{Request.Host}";
    }
}
