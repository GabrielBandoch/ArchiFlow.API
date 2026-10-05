using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ArchiFlow.Application.Agenda.Commands;
using ArchiFlow.Application.Agenda.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgendaController : ControllerBase
{
    private readonly IAgendaFacade _agendaFacade;

    public AgendaController(IAgendaFacade agendaFacade)
    {
        _agendaFacade = agendaFacade;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CompromissoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPorPeriodo(
        [FromQuery] DateTime? inicio,
        [FromQuery] DateTime? fim,
        [FromQuery] Guid? usuarioId,
        [FromQuery] Guid? projetoId)
    {
        var dataInicio = inicio ?? DateTime.UtcNow.Date;
        var dataFim = fim ?? dataInicio.AddDays(30);

        var compromissos = await _agendaFacade.ObterPorPeriodoAsync(dataInicio, dataFim, usuarioId, projetoId);
        return Ok(compromissos);
    }

    [HttpGet("proximos")]
    [ProducesResponseType(typeof(IEnumerable<CompromissoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterProximos(
        [FromQuery] int quantidade = 10,
        [FromQuery] Guid? usuarioId = null)
    {
        var compromissos = await _agendaFacade.ObterProximosAsync(quantidade, usuarioId);
        return Ok(compromissos);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CompromissoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var compromisso = await _agendaFacade.ObterPorIdAsync(id);
        if (compromisso is null)
            return NotFound(new { mensagem = $"Compromisso com ID {id} não encontrado." });

        return Ok(compromisso);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CompromissoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarCompromissoCommand command)
    {
        var criado = await _agendaFacade.CriarCompromissoAsync(command);
        return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CompromissoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarCompromissoCommand command)
    {
        var atualizado = await _agendaFacade.AtualizarCompromissoAsync(id, command);
        return Ok(atualizado);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(CompromissoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarStatus(Guid id, [FromBody] AlterarStatusCompromissoCommand command)
    {
        var atualizado = await _agendaFacade.AlterarStatusCompromissoAsync(id, command);
        return Ok(atualizado);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(Guid id)
    {
        await _agendaFacade.ExcluirCompromissoAsync(id);
        return NoContent();
    }

    [HttpGet("exportar-ics")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarIcs([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
    {
        var icsContent = await _agendaFacade.ExportarIcsAsync(inicio, fim);
        var bytes = Encoding.UTF8.GetBytes(icsContent);
        return File(bytes, "text/calendar", "archiflow-agenda.ics");
    }

    [HttpGet("configuracao")]
    [ProducesResponseType(typeof(ConfiguracaoAgendaEscritorioDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterConfiguracao()
    {
        var config = await _agendaFacade.ObterConfiguracaoAgendaEscritorioAsync();
        return Ok(config);
    }

    [HttpPost("configuracao")]
    [HttpPut("configuracao")]
    [ProducesResponseType(typeof(ConfiguracaoAgendaEscritorioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SalvarConfiguracao([FromBody] SalvarConfiguracaoAgendaEscritorioCommand command)
    {
        var config = await _agendaFacade.SalvarConfiguracaoAgendaEscritorioAsync(command);
        return Ok(config);
    }

    [HttpGet("google/link-compartilhado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterLinkCompartilhado()
    {
        var link = await _agendaFacade.ObterLinkCompartilhadoGoogleAgendaAsync();
        return Ok(new { linkEmbed = link });
    }

    [HttpGet("oauth/url")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterUrlOAuth([FromQuery] string redirectUri)
    {
        if (string.IsNullOrWhiteSpace(redirectUri))
            return BadRequest(new { message = "A URL de redirecionamento é obrigatória." });

        var url = await _agendaFacade.ObterUrlGoogleOAuthAsync(redirectUri);
        var uri = new Uri(url);
        var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var state = queryParams["state"];
        return Ok(new { url, state });
    }

    [HttpPost("oauth/conectar")]
    [ProducesResponseType(typeof(ConfiguracaoAgendaEscritorioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConectarOAuth([FromBody] ConectarGoogleOAuthCommand command)
    {
        var config = await _agendaFacade.ConectarGoogleOAuthAsync(command);
        return Ok(config);
    }

    [HttpDelete("oauth/desconectar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DesconectarOAuth()
    {
        await _agendaFacade.DesconectarGoogleOAuthAsync();
        return NoContent();
    }
}
