using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ArchiFlow.API.Security;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Usuarios.Commands;
using ArchiFlow.Domain.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioFacade _facade;

    public UsuariosController(IUsuarioFacade facade)
    {
        _facade = facade;
    }

    [HttpGet("me")]
    public async Task<IActionResult> ObterMeuPerfil()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var perfil = await _facade.ObterPerfilAsync(userId);
        return perfil is null ? NotFound() : Ok(perfil);
    }

    [HttpPut("me")]
    public async Task<IActionResult> AtualizarMeuPerfil([FromBody] AtualizarPerfilCommand command)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var perfil = await _facade.AtualizarPerfilAsync(userId, command);
        return Ok(perfil);
    }

    [HttpPost("me/foto")]
    public async Task<IActionResult> UploadFotoPerfil([FromForm] IFormFile? file, [FromForm] IFormFile? arquivo)
    {
        var targetFile = file ?? arquivo ?? (Request.HasFormContentType ? Request.Form.Files.FirstOrDefault() : null);
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        if (targetFile == null || targetFile.Length == 0)
            return BadRequest("Nenhum arquivo enviado.");

        if (targetFile.Length > 5 * 1024 * 1024)
            return BadRequest("O tamanho da imagem não deve exceder 5MB.");

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedTypes.Contains(targetFile.ContentType.ToLower()))
            return BadRequest("Formato de imagem inválido. Use JPEG, PNG ou WEBP.");

        using var stream = targetFile.OpenReadStream();
        var fotoUrl = await _facade.AtualizarFotoPerfilAsync(userId, stream, targetFile.FileName, targetFile.ContentType);
        return Ok(new { fotoUrl });
    }

    [HttpDelete("me/foto")]
    public async Task<IActionResult> RemoverFotoPerfil()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        await _facade.RemoverFotoPerfilAsync(userId);
        return NoContent();
    }

    [HttpPut("me/senha")]
    public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaCommand command)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        await _facade.AlterarSenhaAsync(userId, command);
        return NoContent();
    }

    [HttpGet("equipe")]
    [Authorize(Policy = "AcessoArquiteto")]
    [RequireModule(ModulosApp.Configuracoes)]
    public async Task<IActionResult> ObterEquipe() =>
        Ok(await _facade.ObterEquipeAsync());

    [HttpGet("equipe/{id:guid}")]
    [Authorize(Policy = "AcessoArquiteto")]
    [RequireModule(ModulosApp.Configuracoes)]
    public async Task<IActionResult> ObterMembroPorId(Guid id)
    {
        var membro = await _facade.ObterMembroPorIdAsync(id);
        return membro is null ? NotFound() : Ok(membro);
    }

    [HttpPost("equipe")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    [RequireModule(ModulosApp.Configuracoes)]
    public async Task<IActionResult> ConvidarMembro([FromBody] ConvidarMembroEquipeCommand command)
    {
        var membro = await _facade.ConvidarMembroAsync(command);
        return CreatedAtAction(nameof(ObterMembroPorId), new { id = membro.Id }, membro);
    }

    [HttpPut("equipe/{id:guid}")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    [RequireModule(ModulosApp.Configuracoes)]
    public async Task<IActionResult> AtualizarMembro(Guid id, [FromBody] AtualizarMembroEquipeCommand command) =>
        Ok(await _facade.AtualizarMembroAsync(id, command));

    [HttpPatch("equipe/{id:guid}/status")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    [RequireModule(ModulosApp.Configuracoes)]
    public async Task<IActionResult> AlterarStatusMembro(Guid id, [FromBody] AlterarStatusMembroCommand command) =>
        Ok(await _facade.AlterarStatusMembroAsync(id, command));

    [HttpPost("equipe/{id:guid}/redefinir-senha")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    [RequireModule(ModulosApp.Configuracoes)]
    public async Task<IActionResult> RedefinirSenhaMembro(Guid id, [FromBody] RedefinirSenhaMembroCommand command)
    {
        await _facade.RedefinirSenhaMembroAsync(id, command);
        return NoContent();
    }

    [HttpDelete("equipe/{id:guid}")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    [RequireModule(ModulosApp.Configuracoes)]
    public async Task<IActionResult> ExcluirMembro(Guid id)
    {
        await _facade.ExcluirMembroAsync(id);
        return NoContent();
    }
}
