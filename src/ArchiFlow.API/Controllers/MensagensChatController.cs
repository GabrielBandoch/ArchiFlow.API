using ArchiFlow.Application.Chat.Commands;
using ArchiFlow.Application.Chat.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/mensagens")]
[Authorize]
public class MensagensChatController : ControllerBase
{
    private readonly IMensagemChatFacade _mensagemFacade;

    public MensagensChatController(IMensagemChatFacade mensagemFacade)
    {
        _mensagemFacade = mensagemFacade;
    }

    [HttpGet("projeto/{projetoId:guid}")]
    [Authorize(Policy = "ProjetoOwner")]
    public async Task<ActionResult<IEnumerable<MensagemChatDto>>> GetByProjeto(Guid projetoId, [FromQuery] int take = 50)
    {
        var mensagens = await _mensagemFacade.GetByProjetoId(projetoId, take);

        var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("nameid")?.Value
                        ?? User.FindFirst("sub")?.Value;

        if (Guid.TryParse(usuarioIdStr, out var usuarioId))
        {
            await _mensagemFacade.MarcarComoLidas(projetoId, usuarioId);
        }

        return Ok(mensagens);
    }

    [HttpPost("projeto/{projetoId:guid}")]
    [Authorize(Policy = "ProjetoOwner")]
    public async Task<ActionResult<MensagemChatDto>> EnviarMensagem(Guid projetoId, [FromBody] EnviarMensagemCommand command)
    {
        var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("nameid")?.Value
                        ?? User.FindFirst("sub")?.Value;

        var usuarioId = Guid.TryParse(usuarioIdStr, out var id) ? id : Guid.NewGuid();

        var usuarioNome = User.FindFirst(ClaimTypes.Name)?.Value
                       ?? User.FindFirst("name")?.Value
                       ?? User.FindFirst("unique_name")?.Value
                       ?? User.FindFirst(ClaimTypes.Email)?.Value
                       ?? "Usuário";

        var role = User.FindFirst(ClaimTypes.Role)?.Value
                ?? User.FindFirst("role")?.Value;

        var userType = User.FindFirst("user_type")?.Value;
        var perfil = (userType == "client" || role == "Cliente") ? "Cliente" : (role ?? "Arquiteto");

        var msg = await _mensagemFacade.EnviarMensagem(
            projetoId,
            usuarioId,
            usuarioNome,
            perfil,
            command.Conteudo
        );

        return CreatedAtAction(nameof(GetByProjeto), new { projetoId }, msg);
    }
}
