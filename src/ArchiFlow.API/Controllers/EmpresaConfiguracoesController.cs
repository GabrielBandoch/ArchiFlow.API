using ArchiFlow.API.Security;
using ArchiFlow.Application.Empresa.Commands;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Domain.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/configuracoes")]
[Authorize]
[RequireModule(ModulosApp.Configuracoes)]
public class EmpresaConfiguracoesController : ControllerBase
{
    private static readonly string[] AllowedLogoTypes = ["image/jpeg", "image/png", "image/webp", "image/svg+xml"];
    private readonly IEmpresaFacade _empresaFacade;

    public EmpresaConfiguracoesController(IEmpresaFacade empresaFacade)
    {
        _empresaFacade = empresaFacade;
    }

    [HttpGet("empresa")]
    [Authorize(Policy = "AcessoArquiteto")]
    public async Task<IActionResult> ObterDadosEmpresa() =>
        Ok(await _empresaFacade.ObterDadosEmpresaAsync());

    [HttpPut("empresa")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    public async Task<IActionResult> SalvarDadosEmpresa([FromBody] SalvarDadosEmpresaCommand command) =>
        Ok(await _empresaFacade.SalvarDadosEmpresaAsync(command));

    [HttpPatch("empresa/cor")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    public async Task<IActionResult> AtualizarCorPrimaria([FromBody] AtualizarCorPrimariaCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.CorPrimaria))
            return BadRequest("A cor primária é obrigatória.");

        var dados = await _empresaFacade.AtualizarCorPrimariaAsync(command.CorPrimaria);
        return Ok(dados);
    }

    [HttpPost("empresa/logo")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    public async Task<IActionResult> UploadLogo([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Nenhum arquivo enviado.");

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("A imagem deve ter no máximo 5MB.");

        if (!AllowedLogoTypes.Any(t => string.Equals(t, file.ContentType, StringComparison.OrdinalIgnoreCase)))
            return BadRequest("Formato de imagem inválido. Use JPEG, PNG, WEBP ou SVG.");

        using var stream = file.OpenReadStream();
        var logoUrl = await _empresaFacade.AtualizarLogotipoAsync(stream, file.FileName, file.ContentType);
        return Ok(new { logoUrl });
    }

    [HttpDelete("empresa/logo")]
    [Authorize(Policy = "ApenasGerenteOuAdmin")]
    public async Task<IActionResult> RemoverLogo()
    {
        await _empresaFacade.RemoverLogotipoAsync();
        return NoContent();
    }
}

