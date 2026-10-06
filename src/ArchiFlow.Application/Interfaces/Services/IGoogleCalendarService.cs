using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.Application.Agenda.DTOs;
using ArchiFlow.Domain.Agenda;

namespace ArchiFlow.Application.Interfaces.Services;

public interface IGoogleCalendarService
{
    string GerarLinkWebAdicionarEvento(Compromisso compromisso, string? nomeProjeto = null, string? nomeCliente = null, string? nomeLead = null, string? emailAgendaEmpresa = null);
    string ExportarIcs(IEnumerable<Compromisso> compromissos, string nomeCalendario = "ArchiFlow - Agenda");
    string GerarUrlAutorizacaoOAuth(string clientId, string redirectUri, string state);

    Task<(string? refreshToken, string? email)> TrocarCodigoPorRefreshTokenAsync(
        string code,
        string clientId,
        string clientSecret,
        string redirectUri);

    Task<GoogleCalendarSyncResult> CriarEventoDiretoNoGoogleCalendarAsync(
        GoogleCalendarEventRequest request,
        string chaveServiceAccountJson);

    Task<GoogleCalendarSyncResult> CriarEventoViaOAuthAsync(
        GoogleCalendarEventRequest request,
        string refreshToken,
        string clientId,
        string clientSecret);

    Task<bool> AtualizarEventoDiretoAsync(
        GoogleCalendarEventRequest request,
        string chaveServiceAccountJson);

    Task<bool> AtualizarEventoViaOAuthAsync(
        GoogleCalendarEventRequest request,
        string refreshToken,
        string clientId,
        string clientSecret);

    Task<bool> ExcluirEventoDiretoAsync(
        string googleEventId,
        string calendarId,
        string chaveServiceAccountJson);

    Task<bool> ExcluirEventoViaOAuthAsync(
        string googleEventId,
        string calendarId,
        string refreshToken,
        string clientId,
        string clientSecret);
}
