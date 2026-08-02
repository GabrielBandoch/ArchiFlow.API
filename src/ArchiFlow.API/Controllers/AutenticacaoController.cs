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
}
