using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ArchiFlow.Application.Agenda.DTOs;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Agenda;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;

namespace ArchiFlow.Infrastructure.Services;

public class GoogleCalendarService : IGoogleCalendarService
{
    private const string AppName = "ArchiFlow";
    private const string PrimaryCalendarId = "primary";

    private static readonly Uri GoogleTokenUri = new("https://oauth2.googleapis.com/token");
    private static readonly Uri GoogleUserInfoUri = new("https://www.googleapis.com/oauth2/v2/userinfo");

    private readonly ILogger<GoogleCalendarService>? _logger;
    private readonly HttpClient? _httpClient;

    public GoogleCalendarService(ILogger<GoogleCalendarService>? logger = null, HttpClient? httpClient = null)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public string GerarLinkWebAdicionarEvento(Compromisso compromisso, string? nomeProjeto = null, string? nomeCliente = null, string? nomeLead = null, string? emailAgendaEmpresa = null)
    {
        var sbDescricao = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(compromisso.Descricao))
        {
            sbDescricao.AppendLine(compromisso.Descricao);
            sbDescricao.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(nomeProjeto))
        {
            sbDescricao.AppendLine($"Projeto: {nomeProjeto}");
        }

        if (!string.IsNullOrWhiteSpace(nomeCliente))
        {
            sbDescricao.AppendLine($"Cliente: {nomeCliente}");
        }

        if (!string.IsNullOrWhiteSpace(nomeLead))
        {
            sbDescricao.AppendLine($"Lead: {nomeLead}");
        }

        if (!string.IsNullOrWhiteSpace(compromisso.LinkGoogleMeet))
        {
            sbDescricao.AppendLine($"Link Google Meet: {compromisso.LinkGoogleMeet}");
        }

        sbDescricao.AppendLine();
        sbDescricao.AppendLine("Criado via ArchiFlow — Gestão Inteligente para Arquitetura");

        var inicioIso = compromisso.DataHoraInicio.ToUniversalTime().ToString("yyyyMMdd\\THHmmss\\Z");
        var fimIso = compromisso.DataHoraFim.ToUniversalTime().ToString("yyyyMMdd\\THHmmss\\Z");

        var query = new StringBuilder("https://calendar.google.com/calendar/render?action=TEMPLATE");
        query.Append($"&text={Uri.EscapeDataString(compromisso.Titulo)}");
        query.Append($"&dates={inicioIso}/{fimIso}");
        query.Append($"&details={Uri.EscapeDataString(sbDescricao.ToString().Trim())}");

        if (!string.IsNullOrWhiteSpace(compromisso.Local))
        {
            query.Append($"&location={Uri.EscapeDataString(compromisso.Local)}");
        }
        else if (!string.IsNullOrWhiteSpace(compromisso.LinkGoogleMeet))
        {
            query.Append($"&location={Uri.EscapeDataString(compromisso.LinkGoogleMeet)}");
        }

        if (!string.IsNullOrWhiteSpace(emailAgendaEmpresa))
        {
            query.Append($"&add={Uri.EscapeDataString(emailAgendaEmpresa)}");
        }

        return query.ToString();
    }

    public string ExportarIcs(IEnumerable<Compromisso> compromissos, string nomeCalendario = "ArchiFlow - Agenda")
    {
        var sb = new StringBuilder();
        sb.AppendLine("BEGIN:VCALENDAR");
        sb.AppendLine("VERSION:2.0");
        sb.AppendLine("PRODID:-//ArchiFlow//Agenda//PT-BR");
        sb.AppendLine($"X-WR-CALNAME:{nomeCalendario}");
        sb.AppendLine("CALSCALE:GREGORIAN");
        sb.AppendLine("METHOD:PUBLISH");

        foreach (var c in compromissos)
        {
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{c.Id}@archiflow.com");
            sb.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMdd\\THHmmss\\Z}");
            sb.AppendLine($"DTSTART:{c.DataHoraInicio.ToUniversalTime():yyyyMMdd\\THHmmss\\Z}");
            sb.AppendLine($"DTEND:{c.DataHoraFim.ToUniversalTime():yyyyMMdd\\THHmmss\\Z}");
            sb.AppendLine($"SUMMARY:{SanitizeIcs(c.Titulo)}");

            if (!string.IsNullOrWhiteSpace(c.Descricao))
            {
                sb.AppendLine($"DESCRIPTION:{SanitizeIcs(c.Descricao)}");
            }

            var loc = !string.IsNullOrWhiteSpace(c.Local) ? c.Local : c.LinkGoogleMeet;
            if (!string.IsNullOrWhiteSpace(loc))
            {
                sb.AppendLine($"LOCATION:{SanitizeIcs(loc)}");
            }

            sb.AppendLine($"STATUS:{(c.Status == StatusCompromisso.Cancelado ? "CANCELLED" : "CONFIRMED")}");
            sb.AppendLine("END:VEVENT");
        }

        sb.AppendLine("END:VCALENDAR");
        return sb.ToString();
    }

    private static string SanitizeIcs(string text)
    {
        return text.Replace("\\", "\\\\")
                   .Replace(";", "\\;")
                   .Replace(",", "\\,")
                   .Replace("\r\n", "\\n")
                   .Replace("\n", "\\n");
    }

    public string GerarUrlAutorizacaoOAuth(string clientId, string redirectUri, string state)
    {
        var scopes = Uri.EscapeDataString("https://www.googleapis.com/auth/calendar.events https://www.googleapis.com/auth/userinfo.email");
        return $"https://accounts.google.com/o/oauth2/v2/auth?" +
               $"client_id={Uri.EscapeDataString(clientId)}" +
               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
               $"&response_type=code" +
               $"&scope={scopes}" +
               $"&access_type=offline" +
               $"&prompt=consent" +
               $"&state={Uri.EscapeDataString(state)}";
    }

    public async Task<(string? refreshToken, string? email)> TrocarCodigoPorRefreshTokenAsync(
        string code,
        string clientId,
        string clientSecret,
        string redirectUri)
    {
        try
        {
            using var localClient = _httpClient == null ? new HttpClient() : null;
            var client = _httpClient ?? localClient!;
            var payload = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" }
            };

            using var response = await client.PostAsync(GoogleTokenUri, new FormUrlEncodedContent(payload));
            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogWarning("Falha ao trocar código por refresh token: {StatusCode}", response.StatusCode);
                return (null, null);
            }

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var root = doc.RootElement;
            var refreshToken = root.TryGetProperty("refresh_token", out var rtProp) ? rtProp.GetString() : null;
            var accessToken = root.TryGetProperty("access_token", out var atProp) ? atProp.GetString() : null;

            string? email = null;
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                try
                {
                    using var userInfoReq = new HttpRequestMessage(HttpMethod.Get, GoogleUserInfoUri);
                    userInfoReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    using var userInfoResp = await client.SendAsync(userInfoReq);
                    if (userInfoResp.IsSuccessStatusCode)
                    {
                        using var userDoc = await JsonDocument.ParseAsync(await userInfoResp.Content.ReadAsStreamAsync());
                        if (userDoc.RootElement.TryGetProperty("email", out var emailProp))
                        {
                            email = emailProp.GetString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Falha na leitura do e-mail de perfil do usuário Google");
                }
            }

            return (refreshToken, email);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Erro na comunicação com os servidores do Google OAuth");
            return (null, null);
        }
    }

    public async Task<GoogleCalendarSyncResult> CriarEventoDiretoNoGoogleCalendarAsync(
        GoogleCalendarEventRequest request,
        string chaveServiceAccountJson)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(chaveServiceAccountJson) || string.IsNullOrWhiteSpace(request.CalendarId))
                return new GoogleCalendarSyncResult { Sucesso = false };

#pragma warning disable CS0618
            var credential = GoogleCredential.FromJson(chaveServiceAccountJson)
                .CreateScoped(CalendarService.Scope.Calendar);
#pragma warning restore CS0618

            using var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = AppName
            });

            var sbDescricao = MontarDescricao(request);
            var targetCalendar = request.CalendarId.Trim();
            var novoEvento = MontarEvento(request.Compromisso, sbDescricao, request.SolicitarGoogleMeet);
            var insertRequest = service.Events.Insert(novoEvento, targetCalendar);
            if (request.SolicitarGoogleMeet)
            {
                insertRequest.ConferenceDataVersion = 1;
            }

            var eventoCriado = await insertRequest.ExecuteAsync();
            var meetUrlReal = ExtrairMeetUrl(eventoCriado);

            return new GoogleCalendarSyncResult
            {
                Sucesso = true,
                GoogleEventId = eventoCriado?.Id,
                LinkGoogleMeet = meetUrlReal
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Erro ao criar evento via Service Account no Google Calendar");
            return new GoogleCalendarSyncResult { Sucesso = false };
        }
    }

    public async Task<GoogleCalendarSyncResult> CriarEventoViaOAuthAsync(
        GoogleCalendarEventRequest request,
        string refreshToken,
        string clientId,
        string clientSecret)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refreshToken) || string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
                return new GoogleCalendarSyncResult { Sucesso = false };

            using var service = CriarCalendarServiceComOAuth(refreshToken, clientId, clientSecret);
            var sbDescricao = MontarDescricao(request);
            var targetCalendar = string.IsNullOrWhiteSpace(request.CalendarId) ? PrimaryCalendarId : request.CalendarId.Trim();
            var novoEvento = MontarEvento(request.Compromisso, sbDescricao, request.SolicitarGoogleMeet);
            var insertRequest = service.Events.Insert(novoEvento, targetCalendar);
            if (request.SolicitarGoogleMeet)
            {
                insertRequest.ConferenceDataVersion = 1;
            }

            var eventoCriado = await insertRequest.ExecuteAsync();
            var meetUrlReal = ExtrairMeetUrl(eventoCriado);

            return new GoogleCalendarSyncResult
            {
                Sucesso = true,
                GoogleEventId = eventoCriado?.Id,
                LinkGoogleMeet = meetUrlReal
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Erro ao criar evento via OAuth no Google Calendar");
            return new GoogleCalendarSyncResult { Sucesso = false };
        }
    }

    public async Task<bool> AtualizarEventoDiretoAsync(
        GoogleCalendarEventRequest request,
        string chaveServiceAccountJson)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Compromisso.GoogleEventId) || string.IsNullOrWhiteSpace(chaveServiceAccountJson))
                return false;

#pragma warning disable CS0618
            var credential = GoogleCredential.FromJson(chaveServiceAccountJson)
                .CreateScoped(CalendarService.Scope.Calendar);
#pragma warning restore CS0618

            using var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = AppName
            });

            var targetCalendar = string.IsNullOrWhiteSpace(request.CalendarId) ? PrimaryCalendarId : request.CalendarId.Trim();
            var evt = MontarEvento(request.Compromisso, MontarDescricao(request), false);
            await service.Events.Update(evt, targetCalendar, request.Compromisso.GoogleEventId).ExecuteAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Falha ao atualizar evento no Google Calendar via Service Account");
            return false;
        }
    }

    public async Task<bool> AtualizarEventoViaOAuthAsync(
        GoogleCalendarEventRequest request,
        string refreshToken,
        string clientId,
        string clientSecret)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Compromisso.GoogleEventId) || string.IsNullOrWhiteSpace(refreshToken))
                return false;

            using var service = CriarCalendarServiceComOAuth(refreshToken, clientId, clientSecret);
            var targetCalendar = string.IsNullOrWhiteSpace(request.CalendarId) ? PrimaryCalendarId : request.CalendarId.Trim();
            var evt = MontarEvento(request.Compromisso, MontarDescricao(request), false);
            await service.Events.Update(evt, targetCalendar, request.Compromisso.GoogleEventId).ExecuteAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Falha ao atualizar evento no Google Calendar via OAuth");
            return false;
        }
    }

    public async Task<bool> ExcluirEventoDiretoAsync(
        string googleEventId,
        string calendarId,
        string chaveServiceAccountJson)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(googleEventId) || string.IsNullOrWhiteSpace(chaveServiceAccountJson))
                return false;

#pragma warning disable CS0618
            var credential = GoogleCredential.FromJson(chaveServiceAccountJson)
                .CreateScoped(CalendarService.Scope.Calendar);
#pragma warning restore CS0618

            using var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = AppName
            });

            var targetCalendar = string.IsNullOrWhiteSpace(calendarId) ? PrimaryCalendarId : calendarId.Trim();
            await service.Events.Delete(targetCalendar, googleEventId).ExecuteAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Falha ao excluir evento no Google Calendar via Service Account");
            return false;
        }
    }

    public async Task<bool> ExcluirEventoViaOAuthAsync(
        string googleEventId,
        string calendarId,
        string refreshToken,
        string clientId,
        string clientSecret)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(googleEventId) || string.IsNullOrWhiteSpace(refreshToken))
                return false;

            using var service = CriarCalendarServiceComOAuth(refreshToken, clientId, clientSecret);
            var targetCalendar = string.IsNullOrWhiteSpace(calendarId) ? PrimaryCalendarId : calendarId.Trim();
            await service.Events.Delete(targetCalendar, googleEventId).ExecuteAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Falha ao excluir evento no Google Calendar via OAuth");
            return false;
        }
    }

    private static CalendarService CriarCalendarServiceComOAuth(string refreshToken, string clientId, string clientSecret)
    {
        var flow = new Google.Apis.Auth.OAuth2.Flows.GoogleAuthorizationCodeFlow(
            new Google.Apis.Auth.OAuth2.Flows.GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                Scopes = new[] { CalendarService.Scope.CalendarEvents }
            });

        var token = new Google.Apis.Auth.OAuth2.Responses.TokenResponse
        {
            RefreshToken = refreshToken
        };

        var credential = new UserCredential(flow, "user", token);
        return new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = AppName
        });
    }

    private static string MontarDescricao(GoogleCalendarEventRequest request)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(request.Compromisso.Descricao))
        {
            sb.AppendLine(request.Compromisso.Descricao);
            sb.AppendLine();
        }
        if (!string.IsNullOrWhiteSpace(request.NomeProjeto)) sb.AppendLine($"Projeto: {request.NomeProjeto}");
        if (!string.IsNullOrWhiteSpace(request.NomeCliente)) sbDescricaoAdd(sb, "Cliente", request.NomeCliente);
        if (!string.IsNullOrWhiteSpace(request.NomeLead)) sbDescricaoAdd(sb, "Lead", request.NomeLead);
        if (!string.IsNullOrWhiteSpace(request.Compromisso.LinkGoogleMeet)) sbDescricaoAdd(sb, "Google Meet", request.Compromisso.LinkGoogleMeet);

        return sb.ToString().Trim();
    }

    private static void sbDescricaoAdd(StringBuilder sb, string label, string value)
    {
        sb.AppendLine($"{label}: {value}");
    }

    internal static Event MontarEvento(Compromisso compromisso, string descricao, bool gerarMeet)
    {
        var evt = new Event
        {
            Summary = compromisso.Titulo,
            Description = descricao,
            Location = compromisso.Local,
            Start = new EventDateTime
            {
                DateTimeDateTimeOffset = compromisso.DataHoraInicio.ToUniversalTime(),
                TimeZone = "America/Sao_Paulo"
            },
            End = new EventDateTime
            {
                DateTimeDateTimeOffset = compromisso.DataHoraFim.ToUniversalTime(),
                TimeZone = "America/Sao_Paulo"
            }
        };

        if (compromisso.Status == StatusCompromisso.Cancelado)
        {
            evt.Status = "cancelled";
        }

        if (gerarMeet)
        {
            evt.ConferenceData = new ConferenceData
            {
                CreateRequest = new CreateConferenceRequest
                {
                    RequestId = Guid.NewGuid().ToString("N"),
                    ConferenceSolutionKey = new ConferenceSolutionKey
                    {
                        Type = "hangoutsMeet"
                    }
                }
            };
        }

        return evt;
    }

    internal static string? ExtrairMeetUrl(Event? evt)
    {
        return evt?.HangoutLink
            ?? evt?.ConferenceData?.EntryPoints?.FirstOrDefault(e => e.EntryPointType == "video")?.Uri;
    }
}
