using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Agenda;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;

namespace ArchiFlow.Infrastructure.Services;

public class GoogleCalendarService : IGoogleCalendarService
{
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

    public string GerarLinkGoogleMeet(string identificador)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(identificador));
        static char ToLetter(byte b) => (char)('a' + (b % 26));

        var part1 = new string(new[] { ToLetter(bytes[0]), ToLetter(bytes[1]), ToLetter(bytes[2]) });
        var part2 = new string(new[] { ToLetter(bytes[3]), ToLetter(bytes[4]), ToLetter(bytes[5]), ToLetter(bytes[6]) });
        var part3 = new string(new[] { ToLetter(bytes[7]), ToLetter(bytes[8]), ToLetter(bytes[9]) });

        return $"https://meet.google.com/{part1}-{part2}-{part3}";
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

    public async Task<string?> CriarEventoDiretoNoGoogleCalendarAsync(
        Compromisso compromisso,
        string calendarId,
        string chaveServiceAccountJson,
        string? nomeProjeto = null,
        string? nomeCliente = null,
        string? nomeLead = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(chaveServiceAccountJson) || string.IsNullOrWhiteSpace(calendarId))
                return null;

#pragma warning disable CS0618
            var credential = GoogleCredential.FromJson(chaveServiceAccountJson)
                .CreateScoped(CalendarService.Scope.Calendar);
#pragma warning restore CS0618

            using var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "ArchiFlow"
            });

            var sbDescricao = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(compromisso.Descricao))
            {
                sbDescricao.AppendLine(compromisso.Descricao);
                sbDescricao.AppendLine();
            }
            if (!string.IsNullOrWhiteSpace(nomeProjeto)) sbDescricao.AppendLine($"Projeto: {nomeProjeto}");
            if (!string.IsNullOrWhiteSpace(nomeCliente)) sbDescricao.AppendLine($"Cliente: {nomeCliente}");
            if (!string.IsNullOrWhiteSpace(nomeLead)) sbDescricao.AppendLine($"Lead: {nomeLead}");
            if (!string.IsNullOrWhiteSpace(compromisso.LinkGoogleMeet)) sbDescricao.AppendLine($"Google Meet: {compromisso.LinkGoogleMeet}");

            var targetCalendar = calendarId.Trim();
            var novoEvento = MontarEvento(compromisso, sbDescricao.ToString().Trim());
            var insertRequest = service.Events.Insert(novoEvento, targetCalendar);
            if (!string.IsNullOrWhiteSpace(compromisso.LinkGoogleMeet))
            {
                insertRequest.ConferenceDataVersion = 1;
            }
            var eventoCriado = await insertRequest.ExecuteAsync();

            var meetUrlReal = eventoCriado?.HangoutLink
                ?? eventoCriado?.ConferenceData?.EntryPoints?.FirstOrDefault(e => e.EntryPointType == "video")?.Uri;
            if (!string.IsNullOrWhiteSpace(meetUrlReal))
            {
                compromisso.LinkGoogleMeet = meetUrlReal;
            }

            return eventoCriado?.Id;
        }
        catch
        {
            return null;
        }
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
            using var httpClient = new System.Net.Http.HttpClient();
            var payload = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" }
            };

            using var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", new System.Net.Http.FormUrlEncodedContent(payload));
            if (!response.IsSuccessStatusCode)
            {
                return (null, null);
            }

            using var doc = await System.Text.Json.JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var root = doc.RootElement;
            var refreshToken = root.TryGetProperty("refresh_token", out var rtProp) ? rtProp.GetString() : null;
            var accessToken = root.TryGetProperty("access_token", out var atProp) ? atProp.GetString() : null;

            string? email = null;
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                try
                {
                    using var userInfoReq = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
                    userInfoReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    using var userInfoResp = await httpClient.SendAsync(userInfoReq);
                    if (userInfoResp.IsSuccessStatusCode)
                    {
                        using var userDoc = await System.Text.Json.JsonDocument.ParseAsync(await userInfoResp.Content.ReadAsStreamAsync());
                        if (userDoc.RootElement.TryGetProperty("email", out var emailProp))
                        {
                            email = emailProp.GetString();
                        }
                    }
                }
                catch
                {
                    // Falha silenciosa na leitura do e-mail
                }
            }

            return (refreshToken, email);
        }
        catch
        {
            return (null, null);
        }
    }

    public async Task<string?> CriarEventoViaOAuthAsync(
        Compromisso compromisso,
        string calendarId,
        string refreshToken,
        string clientId,
        string clientSecret,
        string? nomeProjeto = null,
        string? nomeCliente = null,
        string? nomeLead = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refreshToken) || string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
                return null;

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

            using var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "ArchiFlow"
            });

            var sbDescricao = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(compromisso.Descricao))
            {
                sbDescricao.AppendLine(compromisso.Descricao);
                sbDescricao.AppendLine();
            }
            if (!string.IsNullOrWhiteSpace(nomeProjeto)) sbDescricao.AppendLine($"Projeto: {nomeProjeto}");
            if (!string.IsNullOrWhiteSpace(nomeCliente)) sbDescricao.AppendLine($"Cliente: {nomeCliente}");
            if (!string.IsNullOrWhiteSpace(nomeLead)) sbDescricao.AppendLine($"Lead: {nomeLead}");
            if (!string.IsNullOrWhiteSpace(compromisso.LinkGoogleMeet)) sbDescricao.AppendLine($"Google Meet: {compromisso.LinkGoogleMeet}");

            var targetCalendar = string.IsNullOrWhiteSpace(calendarId) ? "primary" : calendarId.Trim();
            var novoEvento = MontarEvento(compromisso, sbDescricao.ToString().Trim());
            var insertRequest = service.Events.Insert(novoEvento, targetCalendar);
            if (!string.IsNullOrWhiteSpace(compromisso.LinkGoogleMeet))
            {
                insertRequest.ConferenceDataVersion = 1;
            }
            var eventoCriado = await insertRequest.ExecuteAsync();

            var meetUrlReal = eventoCriado?.HangoutLink
                ?? eventoCriado?.ConferenceData?.EntryPoints?.FirstOrDefault(e => e.EntryPointType == "video")?.Uri;
            if (!string.IsNullOrWhiteSpace(meetUrlReal))
            {
                compromisso.LinkGoogleMeet = meetUrlReal;
            }

            return eventoCriado?.Id;
        }
        catch
        {
            return null;
        }
    }

    private static Event MontarEvento(Compromisso compromisso, string descricao)
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

        if (!string.IsNullOrWhiteSpace(compromisso.LinkGoogleMeet))
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
}
