using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.Application.Agenda.Commands;
using ArchiFlow.Application.Agenda.DTOs;

namespace ArchiFlow.Application.Interfaces.Facades;

public interface IAgendaFacade
{
    Task<IEnumerable<CompromissoDto>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim, Guid? usuarioId = null, Guid? projetoId = null);
    Task<IEnumerable<CompromissoDto>> ObterProximosAsync(int quantidade = 10, Guid? usuarioId = null);
    Task<CompromissoDto?> ObterPorIdAsync(Guid id);
    Task<CompromissoDto> CriarCompromissoAsync(CriarCompromissoCommand command);
    Task<CompromissoDto> AtualizarCompromissoAsync(Guid id, AtualizarCompromissoCommand command);
    Task<CompromissoDto> AlterarStatusCompromissoAsync(Guid id, AlterarStatusCompromissoCommand command);
    Task ExcluirCompromissoAsync(Guid id);
    Task<string> ExportarIcsAsync(DateTime? inicio = null, DateTime? fim = null);
    Task<ConfiguracaoAgendaEscritorioDto?> ObterConfiguracaoAgendaEscritorioAsync();
    Task<ConfiguracaoAgendaEscritorioDto> SalvarConfiguracaoAgendaEscritorioAsync(SalvarConfiguracaoAgendaEscritorioCommand command);
    Task<string> ObterLinkCompartilhadoGoogleAgendaAsync();
    Task<string> ObterUrlGoogleOAuthAsync(string redirectUri);
    Task<ConfiguracaoAgendaEscritorioDto> ConectarGoogleOAuthAsync(ConectarGoogleOAuthCommand command);
    Task DesconectarGoogleOAuthAsync();
}
