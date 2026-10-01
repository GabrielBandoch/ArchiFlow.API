using System;
using System.Threading.Tasks;
using ArchiFlow.Domain.Agenda;
using FluentAssertions;
using Xunit;

namespace ArchiFlow.Tests.Agenda;

public class GoogleAgendaEmpresaTests
{
    [Fact]
    public void ConfiguracaoAgendaEmpresa_DeveArmazenarDadosDaAgendaCentralizadaDoEscritorio()
    {
        // Fase RED: Especificação da Agenda Corporativa do Escritório
        // A empresa/escritório configura uma agenda central e toda a equipe visualiza e gerencia os compromissos por ela.
        var escritorioId = Guid.NewGuid();
        var agora = DateTime.UtcNow;

        var config = new ConfiguracaoAgendaEscritorio
        {
            Id = Guid.NewGuid(),
            EscritorioId = escritorioId,
            EmailAgendaEmpresa = "agenda@estudioarquitetura.com.br",
            GoogleCalendarId = "c_188abcde12345@group.calendar.google.com",
            NomeAgenda = "Agenda Oficial do Estúdio",
            SincronizacaoAutomaticaAtiva = true,
            ConectadoEm = agora
        };

        config.EscritorioId.Should().Be(escritorioId);
        config.EmailAgendaEmpresa.Should().Be("agenda@estudioarquitetura.com.br");
        config.GoogleCalendarId.Should().NotBeNullOrEmpty();
        config.SincronizacaoAutomaticaAtiva.Should().BeTrue();
    }

    [Fact]
    public void GerarUrlVisualizacaoGoogleAgenda_DeveRetornarLinkCompartilhadoDoEscritorio()
    {
        // Fase RED: Link centralizado do Google Calendar da empresa para que todos os colaboradores visualizem
        var calendarId = "agenda@estudioarquitetura.com.br";
        var urlEsperada = $"https://calendar.google.com/calendar/embed?src={Uri.EscapeDataString(calendarId)}&ctz=America%2FSao_Paulo";

        urlEsperada.Should().Contain("calendar.google.com/calendar/embed");
        urlEsperada.Should().Contain(Uri.EscapeDataString(calendarId));
    }

    [Fact]
    public async Task ObterConfiguracaoAgendaEscritorio_DeveRetornarConfiguracaoCentralizada()
    {
        // Fase RED: O serviço ainda não possui a persistência completa de configuração corporativa
        Func<Task> action = () => throw new NotImplementedException("Método ObterConfiguracaoAgendaEscritorio pendente de implementação (Fase RED)");
        await action.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*Fase RED*");
    }
}
