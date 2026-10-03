using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.Application.Agenda.Commands;
using ArchiFlow.Application.Agenda.DTOs;
using ArchiFlow.Application.Agenda.Facades;
using ArchiFlow.Application.Interfaces.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Agenda;

public class AgendaFacadeTests
{
    private readonly Mock<IAgendaService> _serviceMock;
    private readonly AgendaFacade _facade;

    public AgendaFacadeTests()
    {
        _serviceMock = new Mock<IAgendaService>();
        _facade = new AgendaFacade(_serviceMock.Object);
    }

    [Fact]
    public async Task TodasOperacoes_DevemDelegarParaService()
    {
        var id = Guid.NewGuid();
        var inicio = DateTime.UtcNow;
        var fim = inicio.AddHours(1);
        var dto = new CompromissoDto(id, Guid.NewGuid(), null, null, null, null, null, null, "Titulo", null, "Geral", "Agendado", inicio, fim, null, null, null, "http", inicio, null);

        _serviceMock.Setup(s => s.ObterPorPeriodoAsync(inicio, fim, null, null)).ReturnsAsync(new[] { dto });
        _serviceMock.Setup(s => s.ObterProximosAsync(10, null)).ReturnsAsync(new[] { dto });
        _serviceMock.Setup(s => s.ObterPorIdAsync(id)).ReturnsAsync(dto);

        var rPeriodo = await _facade.ObterPorPeriodoAsync(inicio, fim);
        var rProximos = await _facade.ObterProximosAsync(10);
        var rId = await _facade.ObterPorIdAsync(id);

        rPeriodo.Should().HaveCount(1);
        rProximos.Should().HaveCount(1);
        rId.Should().Be(dto);

        var cmdCriar = new CriarCompromissoCommand("T", inicio, fim, null, null, null, null, null, null);
        _serviceMock.Setup(s => s.CriarCompromissoAsync(cmdCriar)).ReturnsAsync(dto);
        await _facade.CriarCompromissoAsync(cmdCriar);
        _serviceMock.Verify(s => s.CriarCompromissoAsync(cmdCriar), Times.Once);

        var cmdAtualizar = new AtualizarCompromissoCommand("T", inicio, fim, null, null, null, null, null, null, null, null);
        _serviceMock.Setup(s => s.AtualizarCompromissoAsync(id, cmdAtualizar)).ReturnsAsync(dto);
        await _facade.AtualizarCompromissoAsync(id, cmdAtualizar);
        _serviceMock.Verify(s => s.AtualizarCompromissoAsync(id, cmdAtualizar), Times.Once);

        var cmdStatus = new AlterarStatusCompromissoCommand("Concluido");
        _serviceMock.Setup(s => s.AlterarStatusCompromissoAsync(id, cmdStatus)).ReturnsAsync(dto);
        await _facade.AlterarStatusCompromissoAsync(id, cmdStatus);
        _serviceMock.Verify(s => s.AlterarStatusCompromissoAsync(id, cmdStatus), Times.Once);

        await _facade.ExcluirCompromissoAsync(id);
        _serviceMock.Verify(s => s.ExcluirCompromissoAsync(id), Times.Once);

        _serviceMock.Setup(s => s.ExportarIcsAsync(null, null)).ReturnsAsync("ICS");
        var ics = await _facade.ExportarIcsAsync();
        ics.Should().Be("ICS");

        var configDto = new ConfiguracaoAgendaEscritorioDto { Id = Guid.NewGuid(), EscritorioId = Guid.NewGuid() };
        _serviceMock.Setup(s => s.ObterConfiguracaoAgendaEscritorioAsync()).ReturnsAsync(configDto);
        var rConfig = await _facade.ObterConfiguracaoAgendaEscritorioAsync();
        rConfig.Should().Be(configDto);

        var cmdSalvar = new SalvarConfiguracaoAgendaEscritorioCommand { GoogleCalendarId = "cal@google.com" };
        _serviceMock.Setup(s => s.SalvarConfiguracaoAgendaEscritorioAsync(cmdSalvar)).ReturnsAsync(configDto);
        var rSalvar = await _facade.SalvarConfiguracaoAgendaEscritorioAsync(cmdSalvar);
        rSalvar.Should().Be(configDto);

        _serviceMock.Setup(s => s.ObterLinkCompartilhadoGoogleAgendaAsync()).ReturnsAsync("https://cal.link");
        var rLink = await _facade.ObterLinkCompartilhadoGoogleAgendaAsync();
        rLink.Should().Be("https://cal.link");

        _serviceMock.Setup(s => s.ObterUrlGoogleOAuthAsync("http://redir")).ReturnsAsync("https://oauth.url");
        var rOAuthUrl = await _facade.ObterUrlGoogleOAuthAsync("http://redir");
        rOAuthUrl.Should().Be("https://oauth.url");

        var cmdOAuth = new ConectarGoogleOAuthCommand { Code = "code123", RedirectUri = "http://redir" };
        _serviceMock.Setup(s => s.ConectarGoogleOAuthAsync(cmdOAuth)).ReturnsAsync(configDto);
        var rOAuth = await _facade.ConectarGoogleOAuthAsync(cmdOAuth);
        rOAuth.Should().Be(configDto);

        await _facade.DesconectarGoogleOAuthAsync();
        _serviceMock.Verify(s => s.DesconectarGoogleOAuthAsync(), Times.Once);
    }
}
