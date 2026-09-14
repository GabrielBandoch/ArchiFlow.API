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
    private readonly IMensagemChatFacade _facade;

    public MensagensChatController(IMensagemChatFacade facade) => _facade = facade;

    [HttpGet("projeto/{projetoId:guid}")]
    [Authorize(Policy = "ProjetoOwner")]
    public async Task<IActionResult> GetByProjeto(Guid projetoId, [FromQuery] int take = 50) =>
        Ok(await _facade.GetByProjetoId(projetoId, take));

    [HttpPost("projeto/{projetoId:guid}")]
    [Authorize(Policy = "ProjetoOwner")]
    public async Task<IActionResult> EnviarMensagem(Guid projetoId, [FromBody] EnviarMensagemCommand command)
    {
        var msg = await _facade.EnviarMensagem(projetoId, command);
        return CreatedAtAction(nameof(GetByProjeto), new { projetoId }, msg);
    }
}
