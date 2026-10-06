using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.Application.Agenda.DTOs;
using ArchiFlow.Domain.Agenda;
using ArchiFlow.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace ArchiFlow.Tests.Agenda;

public class GoogleCalendarServiceTests
{
    private readonly GoogleCalendarService _service = new();

    [Fact]
    public void GerarLinkWebAdicionarEvento_DeveGerarUrlValidaDoGoogleCalendar()
    {
        var compromisso = new Compromisso
        {
            Id = Guid.NewGuid(),
            Titulo = "Reunião de Briefing Residencial",
            Descricao = "Alinhamento de detalhes com cliente",
            DataHoraInicio = new DateTime(2026, 10, 15, 14, 0, 0, DateTimeKind.Utc),
            DataHoraFim = new DateTime(2026, 10, 15, 15, 30, 0, DateTimeKind.Utc),
            Local = "Av. Paulista, 1000 - SP",
            LinkGoogleMeet = "https://meet.google.com/abc-defg-hij"
        };

        var url = _service.GerarLinkWebAdicionarEvento(compromisso, "Residência Alpha", "Carlos Silva");

        url.Should().StartWith("https://calendar.google.com/calendar/render?action=TEMPLATE");
        url.Should().Contain(Uri.EscapeDataString("Reunião de Briefing Residencial"));
        url.Should().Contain("20261015T140000Z");
        url.Should().Contain("20261015T153000Z");
        url.Should().Contain(Uri.EscapeDataString("Projeto: Residência Alpha"));
        url.Should().Contain(Uri.EscapeDataString("Cliente: Carlos Silva"));
    }

    [Fact]
    public void ExportarIcs_DeveGerarConteudoValidoIcalendar()
    {
        var compromissos = new List<Compromisso>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Titulo = "Visita à Obra",
                Descricao = "Conferência de alvenaria",
                DataHoraInicio = new DateTime(2026, 10, 20, 9, 0, 0, DateTimeKind.Utc),
                DataHoraFim = new DateTime(2026, 10, 20, 11, 0, 0, DateTimeKind.Utc),
                Local = "Rua das Flores, 123",
                Status = StatusCompromisso.Agendado
            }
        };

        var ics = _service.ExportarIcs(compromissos, "Minha Agenda");

        ics.Should().Contain("BEGIN:VCALENDAR");
        ics.Should().Contain("END:VCALENDAR");
        ics.Should().Contain("SUMMARY:Visita à Obra");
        ics.Should().Contain("LOCATION:Rua das Flores\\, 123");
        ics.Should().Contain("STATUS:CONFIRMED");
    }

    [Fact]
    public void GerarUrlAutorizacaoOAuth_DeveRetornarUrlValidaGoogleOAuth()
    {
        var url = _service.GerarUrlAutorizacaoOAuth("my-client-id", "https://app.archiflow.com/callback", "state-123");
        url.Should().StartWith("https://accounts.google.com/o/oauth2/v2/auth");
        url.Should().Contain("client_id=my-client-id");
        url.Should().Contain("redirect_uri=" + Uri.EscapeDataString("https://app.archiflow.com/callback"));
        url.Should().Contain("state=state-123");
    }

    [Fact]
    public async Task CriarEventoDiretoNoGoogleCalendarAsync_SemCredenciais_DeveRetornarFalha()
    {
        var compromisso = new Compromisso { Id = Guid.NewGuid(), Titulo = "Teste" };
        var req = new GoogleCalendarEventRequest { Compromisso = compromisso, CalendarId = "" };
        var res = await _service.CriarEventoDiretoNoGoogleCalendarAsync(req, "");
        res.Sucesso.Should().BeFalse();
        res.GoogleEventId.Should().BeNull();
    }

    [Fact]
    public async Task CriarEventoViaOAuthAsync_SemCredenciais_DeveRetornarFalha()
    {
        var compromisso = new Compromisso { Id = Guid.NewGuid(), Titulo = "Teste" };
        var req = new GoogleCalendarEventRequest { Compromisso = compromisso, CalendarId = "" };
        var res = await _service.CriarEventoViaOAuthAsync(req, "", "", "");
        res.Sucesso.Should().BeFalse();
        res.GoogleEventId.Should().BeNull();
    }

    [Fact]
    public async Task AtualizarEventoDiretoAsync_SemCredenciais_DeveRetornarFalse()
    {
        var compromisso = new Compromisso { Id = Guid.NewGuid(), Titulo = "Teste", GoogleEventId = "evt-123" };
        var req = new GoogleCalendarEventRequest { Compromisso = compromisso, CalendarId = "primary" };
        var ok = await _service.AtualizarEventoDiretoAsync(req, "");
        ok.Should().BeFalse();
    }

    [Fact]
    public async Task AtualizarEventoViaOAuthAsync_SemCredenciais_DeveRetornarFalse()
    {
        var compromisso = new Compromisso { Id = Guid.NewGuid(), Titulo = "Teste", GoogleEventId = "evt-123" };
        var req = new GoogleCalendarEventRequest { Compromisso = compromisso, CalendarId = "primary" };
        var ok = await _service.AtualizarEventoViaOAuthAsync(req, "", "", "");
        ok.Should().BeFalse();
    }

    [Fact]
    public async Task ExcluirEventoDiretoAsync_SemCredenciais_DeveRetornarFalse()
    {
        var ok = await _service.ExcluirEventoDiretoAsync("evt-123", "primary", "");
        ok.Should().BeFalse();
    }

    [Fact]
    public async Task ExcluirEventoViaOAuthAsync_SemCredenciais_DeveRetornarFalse()
    {
        var ok = await _service.ExcluirEventoViaOAuthAsync("evt-123", "primary", "", "", "");
        ok.Should().BeFalse();
    }

    [Fact]
    public void GerarLinkWebAdicionarEvento_SemLocalMasComMeetEEmailEmpresa_DeveUsarMeetComoLocalEAdicionarEmail()
    {
        var compromisso = new Compromisso
        {
            Id = Guid.NewGuid(),
            Titulo = "Alinhamento Remoto",
            Descricao = "Linha 1\nLinha 2",
            DataHoraInicio = DateTime.UtcNow,
            DataHoraFim = DateTime.UtcNow.AddHours(1),
            Local = null,
            LinkGoogleMeet = "https://meet.google.com/xyz-abcd-jkl"
        };

        var url = _service.GerarLinkWebAdicionarEvento(
            compromisso,
            nomeProjeto: null,
            nomeCliente: null,
            nomeLead: "Lead Teste",
            emailAgendaEmpresa: "empresa@archiflow.com");

        url.Should().Contain("&location=" + Uri.EscapeDataString("https://meet.google.com/xyz-abcd-jkl"));
        url.Should().Contain("&add=" + Uri.EscapeDataString("empresa@archiflow.com"));
        url.Should().Contain("Lead%3A%20Lead%20Teste");
    }

    [Fact]
    public void ExportarIcs_ComCompromissoCanceladoECaracteresEspeciais_DeveHigienizarESinalizarCancelado()
    {
        var compromissos = new List<Compromisso>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Titulo = "Reunião; Vírgula, e Barra \\",
                Descricao = "Linha 1\r\nLinha 2",
                DataHoraInicio = DateTime.UtcNow,
                DataHoraFim = DateTime.UtcNow.AddHours(1),
                Local = null,
                LinkGoogleMeet = "https://meet.google.com/xyz",
                Status = StatusCompromisso.Cancelado
            }
        };

        var ics = _service.ExportarIcs(compromissos, "Agenda Especial");

        ics.Should().Contain("STATUS:CANCELLED");
        ics.Should().Contain(@"Reunião\; Vírgula\, e Barra \\");
        ics.Should().Contain(@"LOCATION:https://meet.google.com/xyz");
    }

    [Fact]
    public void MontarEvento_ComMeetEStatusCancelado_DeveConfigurarPropriedadesCorretamente()
    {
        var compromisso = new Compromisso
        {
            Id = Guid.NewGuid(),
            Titulo = "Evento Teste",
            Local = "Sala 1",
            DataHoraInicio = new DateTime(2026, 10, 10, 10, 0, 0, DateTimeKind.Utc),
            DataHoraFim = new DateTime(2026, 10, 10, 11, 0, 0, DateTimeKind.Utc),
            Status = StatusCompromisso.Cancelado
        };

        var evt = GoogleCalendarService.MontarEvento(compromisso, "Descricao detalhada", gerarMeet: true);

        evt.Summary.Should().Be("Evento Teste");
        evt.Description.Should().Be("Descricao detalhada");
        evt.Location.Should().Be("Sala 1");
        evt.Status.Should().Be("cancelled");
        evt.ConferenceData.Should().NotBeNull();
        evt.ConferenceData.CreateRequest.ConferenceSolutionKey.Type.Should().Be("hangoutsMeet");
    }

    [Fact]
    public void MontarEvento_SemMeet_NaoDeveAdicionarConferenceData()
    {
        var compromisso = new Compromisso
        {
            Id = Guid.NewGuid(),
            Titulo = "Evento Simples",
            DataHoraInicio = DateTime.UtcNow,
            DataHoraFim = DateTime.UtcNow.AddHours(1)
        };

        var evt = GoogleCalendarService.MontarEvento(compromisso, "", gerarMeet: false);

        evt.ConferenceData.Should().BeNull();
        evt.Status.Should().BeNull();
    }

    [Fact]
    public void ExtrairMeetUrl_ComHangoutLink_DeveRetornarLink()
    {
        var evt = new Google.Apis.Calendar.v3.Data.Event
        {
            HangoutLink = "https://meet.google.com/test-meet"
        };

        var url = GoogleCalendarService.ExtrairMeetUrl(evt);

        url.Should().Be("https://meet.google.com/test-meet");
    }

    [Fact]
    public void ExtrairMeetUrl_ComEntryPointVideo_DeveRetornarUri()
    {
        var evt = new Google.Apis.Calendar.v3.Data.Event
        {
            ConferenceData = new Google.Apis.Calendar.v3.Data.ConferenceData
            {
                EntryPoints = new List<Google.Apis.Calendar.v3.Data.EntryPoint>
                {
                    new() { EntryPointType = "video", Uri = "https://meet.google.com/video-uri" }
                }
            }
        };

        var url = GoogleCalendarService.ExtrairMeetUrl(evt);

        url.Should().Be("https://meet.google.com/video-uri");
    }

    [Fact]
    public void ExtrairMeetUrl_SemLinks_DeveRetornarNull()
    {
        GoogleCalendarService.ExtrairMeetUrl(null).Should().BeNull();
        GoogleCalendarService.ExtrairMeetUrl(new Google.Apis.Calendar.v3.Data.Event()).Should().BeNull();
    }

    [Fact]
    public async Task TrocarCodigoPorRefreshTokenAsync_QuandoSucesso_DeveRetornarRefreshTokenEEmail()
    {
        var mockHttp = new MockHttpMessageHandler((req) =>
        {
            if (req.RequestUri!.ToString().Contains("token"))
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"refresh_token\":\"rt-12345\",\"access_token\":\"at-67890\"}", System.Text.Encoding.UTF8, "application/json")
                };
            }
            if (req.RequestUri!.ToString().Contains("userinfo"))
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"email\":\"arquiteto@archiflow.com\"}", System.Text.Encoding.UTF8, "application/json")
                };
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
        });

        using var client = new HttpClient(mockHttp);
        var service = new GoogleCalendarService(httpClient: client);

        var (rt, email) = await service.TrocarCodigoPorRefreshTokenAsync("code-123", "client-id", "client-secret", "https://redirect.com");

        rt.Should().Be("rt-12345");
        email.Should().Be("arquiteto@archiflow.com");
    }

    [Fact]
    public async Task TrocarCodigoPorRefreshTokenAsync_QuandoTokenRetornaErro_DeveRetornarNull()
    {
        var mockHttp = new MockHttpMessageHandler((req) =>
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"error\":\"invalid_grant\"}", System.Text.Encoding.UTF8, "application/json")
            };
        });

        using var client = new HttpClient(mockHttp);
        var service = new GoogleCalendarService(httpClient: client);

        var (rt, email) = await service.TrocarCodigoPorRefreshTokenAsync("bad-code", "client-id", "client-secret", "https://redirect.com");

        rt.Should().BeNull();
        email.Should().BeNull();
    }

    [Fact]
    public async Task OperacoesComServiceAccount_ComJsonInvalido_DevemCapturarExcecaoERetornarFalha()
    {
        var compromisso = new Compromisso { Id = Guid.NewGuid(), Titulo = "T", GoogleEventId = "evt-123" };
        var req = new GoogleCalendarEventRequest { Compromisso = compromisso, CalendarId = "primary" };

        var resCriacao = await _service.CriarEventoDiretoNoGoogleCalendarAsync(req, "{ \"invalido\": true }");
        resCriacao.Sucesso.Should().BeFalse();

        var resAtualizacao = await _service.AtualizarEventoDiretoAsync(req, "{ \"invalido\": true }");
        resAtualizacao.Should().BeFalse();

        var resExclusao = await _service.ExcluirEventoDiretoAsync("evt-123", "primary", "{ \"invalido\": true }");
        resExclusao.Should().BeFalse();
    }

    [Fact]
    public async Task OperacoesComOAuth_ComCredenciaisInvalidas_DevemCapturarExcecaoERetornarFalha()
    {
        var compromisso = new Compromisso { Id = Guid.NewGuid(), Titulo = "T", GoogleEventId = "evt-123" };
        var req = new GoogleCalendarEventRequest { Compromisso = compromisso, CalendarId = "primary", SolicitarGoogleMeet = true };

        var resCriacao = await _service.CriarEventoViaOAuthAsync(req, "rt-fake", "client-fake", "secret-fake");
        resCriacao.Sucesso.Should().BeFalse();

        var resAtualizacao = await _service.AtualizarEventoViaOAuthAsync(req, "rt-fake", "client-fake", "secret-fake");
        resAtualizacao.Should().BeFalse();

        var resExclusao = await _service.ExcluirEventoViaOAuthAsync("evt-123", "primary", "rt-fake", "client-fake", "secret-fake");
        resExclusao.Should().BeFalse();
    }

    [Fact]
    public async Task TrocarCodigoPorRefreshTokenAsync_QuandoHttpRequestDisparaExcecao_DeveRetornarNull()
    {
        var mockHttp = new MockHttpMessageHandler((req) => throw new HttpRequestException("Network failure"));
        using var client = new HttpClient(mockHttp);
        var service = new GoogleCalendarService(httpClient: client);

        var (rt, email) = await service.TrocarCodigoPorRefreshTokenAsync("code-123", "client-id", "client-secret", "https://redirect.com");
        rt.Should().BeNull();
        email.Should().BeNull();
    }

    [Fact]
    public async Task TrocarCodigoPorRefreshTokenAsync_QuandoUserInfoFalha_AindaDeveRetornarRefreshToken()
    {
        var mockHttp = new MockHttpMessageHandler((req) =>
        {
            if (req.RequestUri!.ToString().Contains("token"))
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"refresh_token\":\"rt-12345\",\"access_token\":\"at-67890\"}", System.Text.Encoding.UTF8, "application/json")
                };
            }
            // Userinfo fails
            return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
        });

        using var client = new HttpClient(mockHttp);
        var service = new GoogleCalendarService(httpClient: client);

        var (rt, email) = await service.TrocarCodigoPorRefreshTokenAsync("code-123", "client-id", "client-secret", "https://redirect.com");
        rt.Should().Be("rt-12345");
        email.Should().BeNull();
    }


    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;
        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) => _handler = handler;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            => Task.FromResult(_handler(request));
    }
}
