using System;
using System.Collections.Generic;
using ArchiFlow.Domain.Agenda;

namespace ArchiFlow.Application.Interfaces.Services;

public interface IGoogleCalendarService
{
    string GerarLinkWebAdicionarEvento(Compromisso compromisso, string? nomeProjeto = null, string? nomeCliente = null, string? nomeLead = null, string? emailAgendaEmpresa = null);
    string GerarLinkGoogleMeet(string identificador);
    string ExportarIcs(IEnumerable<Compromisso> compromissos, string nomeCalendario = "ArchiFlow - Agenda");
    Task<string?> CriarEventoDiretoNoGoogleCalendarAsync(
        Compromisso compromisso,
        string calendarId,
        string chaveServiceAccountJson,
        string? nomeProjeto = null,
        string? nomeCliente = null,
        string? nomeLead = null);

    string GerarUrlAutorizacaoOAuth(string clientId, string redirectUri, string state);

    Task<(string? refreshToken, string? email)> TrocarCodigoPorRefreshTokenAsync(
        string code,
        string clientId,
        string clientSecret,
        string redirectUri);

    Task<string?> CriarEventoViaOAuthAsync(
        Compromisso compromisso,
        string calendarId,
        string refreshToken,
        string clientId,
        string clientSecret,
        string? nomeProjeto = null,
        string? nomeCliente = null,
        string? nomeLead = null);
}
