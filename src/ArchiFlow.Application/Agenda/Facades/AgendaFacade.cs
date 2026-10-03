using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.Application.Agenda.Commands;
using ArchiFlow.Application.Agenda.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Interfaces.Services;

namespace ArchiFlow.Application.Agenda.Facades;

public class AgendaFacade : IAgendaFacade
{
    private readonly IAgendaService _agendaService;

    public AgendaFacade(IAgendaService agendaService)
    {
        _agendaService = agendaService;
    }

    public Task<IEnumerable<CompromissoDto>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim, Guid? usuarioId = null, Guid? projetoId = null) =>
        _agendaService.ObterPorPeriodoAsync(inicio, fim, usuarioId, projetoId);

    public Task<IEnumerable<CompromissoDto>> ObterProximosAsync(int quantidade = 10, Guid? usuarioId = null) =>
        _agendaService.ObterProximosAsync(quantidade, usuarioId);

    public Task<CompromissoDto?> ObterPorIdAsync(Guid id) =>
        _agendaService.ObterPorIdAsync(id);

    public Task<CompromissoDto> CriarCompromissoAsync(CriarCompromissoCommand command) =>
        _agendaService.CriarCompromissoAsync(command);

    public Task<CompromissoDto> AtualizarCompromissoAsync(Guid id, AtualizarCompromissoCommand command) =>
        _agendaService.AtualizarCompromissoAsync(id, command);

    public Task<CompromissoDto> AlterarStatusCompromissoAsync(Guid id, AlterarStatusCompromissoCommand command) =>
        _agendaService.AlterarStatusCompromissoAsync(id, command);

    public Task ExcluirCompromissoAsync(Guid id) =>
        _agendaService.ExcluirCompromissoAsync(id);

    public Task<string> ExportarIcsAsync(DateTime? inicio = null, DateTime? fim = null) =>
        _agendaService.ExportarIcsAsync(inicio, fim);

    public Task<ConfiguracaoAgendaEscritorioDto?> ObterConfiguracaoAgendaEscritorioAsync() =>
        _agendaService.ObterConfiguracaoAgendaEscritorioAsync();

    public Task<ConfiguracaoAgendaEscritorioDto> SalvarConfiguracaoAgendaEscritorioAsync(SalvarConfiguracaoAgendaEscritorioCommand command) =>
        _agendaService.SalvarConfiguracaoAgendaEscritorioAsync(command);

    public Task<string> ObterLinkCompartilhadoGoogleAgendaAsync() =>
        _agendaService.ObterLinkCompartilhadoGoogleAgendaAsync();

    public Task<string> ObterUrlGoogleOAuthAsync(string redirectUri) =>
        _agendaService.ObterUrlGoogleOAuthAsync(redirectUri);

    public Task<ConfiguracaoAgendaEscritorioDto> ConectarGoogleOAuthAsync(ConectarGoogleOAuthCommand command) =>
        _agendaService.ConectarGoogleOAuthAsync(command);

    public Task DesconectarGoogleOAuthAsync() =>
        _agendaService.DesconectarGoogleOAuthAsync();
}
