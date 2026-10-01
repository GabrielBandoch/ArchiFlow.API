using System;
using System.Collections.Generic;
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
    public void GerarLinkGoogleMeet_DeveRetornarUrlFormatada()
    {
        var link = _service.GerarLinkGoogleMeet("reuniao-123");

        link.Should().StartWith("https://meet.google.com/");
        link.Should().MatchRegex(@"https:\/\/meet\.google\.com\/[a-z0-9]{3}-[a-z0-9]{4}-[a-z0-9]{3}");
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
}
