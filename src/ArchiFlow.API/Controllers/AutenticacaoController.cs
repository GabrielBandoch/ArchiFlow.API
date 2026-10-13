using ArchiFlow.Application.Interfaces.Services;
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

    public AutenticacaoController(IAuthService authService)
    {
        _authService = authService;
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

    [HttpPost("esqueci-senha")]
    public async Task<IActionResult> EsqueciSenha([FromBody] SolicitarRecuperacaoSenhaDto request)
    {
        try
        {
            await _authService.SolicitarRecuperacaoSenhaAsync(request);
            return Ok(new { message = "Se o e-mail informado estiver cadastrado, as instruções para redefinição de senha foram enviadas." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("redefinir-senha")]
    public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaDto request)
    {
        try
        {
            await _authService.RedefinirSenhaAsync(request);
            return Ok(new { message = "Senha redefinida com sucesso! Você já pode fazer login com sua nova senha." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
