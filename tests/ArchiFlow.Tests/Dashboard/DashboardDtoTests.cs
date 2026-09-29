using ArchiFlow.Application.Dashboard.DTOs;
using ArchiFlow.Domain.Dashboard;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace ArchiFlow.Tests.Dashboard;

public class DashboardDtoTests
{
    [Fact]
    public void DashboardMetricasDto_And_Related_Dtos_Should_Instantiate_Correctly()
    {
        var kpis = new DashboardKpisDto(5, 2, 8, 4, 50m, 10, 6, 90000m, 15000m);
        var statusList = new List<ProjetosPorStatusDto> { new("Briefing", "Briefing", 2, 20m) };
        var tipoList = new List<ProjetosPorTipoDto> { new("Residencial", "Residencial", 3, 30m) };
        var leadStatusList = new List<LeadsPorStatusDto> { new("Novo", "Novo Lead", 4, 40m) };
        var leadOrigemList = new List<LeadsPorOrigemDto> { new("Instagram", 5, 50m) };
        var propostasMensalList = new List<PropostasMensalDto> { new("2026-01", "Jan/26", 2, 30000m) };
        var agora = DateTime.UtcNow;
        var projetosRecentes = new List<ProjetoResumoDashboardDto>
        {
            new(Guid.NewGuid(), "Proj", "Cliente", "Briefing", "Residencial", 100m, 4, 1, 25m, agora, agora.AddMonths(2))
        };
        var leadsRecentes = new List<LeadResumoDashboardDto>
        {
            new(Guid.NewGuid(), "Lead", "lead@email.com", "123456", "Novo", "Site", agora)
        };
        var propostasRecentes = new List<PropostaResumoDashboardDto>
        {
            new(Guid.NewGuid(), "Prop", "COD-1", "Destinatario", 120m, 20000m, "Enviada", agora)
        };

        var metricas = new DashboardMetricasDto(
            kpis,
            statusList,
            tipoList,
            leadStatusList,
            leadOrigemList,
            propostasMensalList,
            projetosRecentes,
            leadsRecentes,
            propostasRecentes
        );

        metricas.Kpis.TotalProjetosAtivos.Should().Be(5);
        metricas.ProjetosPorStatus.Should().HaveCount(1);
        metricas.ProjetosPorTipo.Should().HaveCount(1);
        metricas.LeadsPorStatus.Should().HaveCount(1);
        metricas.LeadsPorOrigem.Should().HaveCount(1);
        metricas.PropostasMensais.Should().HaveCount(1);
        metricas.ProjetosRecentes.Should().HaveCount(1);
        metricas.LeadsRecentes.Should().HaveCount(1);
        metricas.PropostasRecentes.Should().HaveCount(1);
    }

    [Fact]
    public void PreferenciaDashboard_And_Commands_Should_Instantiate_Correctly()
    {
        var usuarioId = Guid.NewGuid();
        var agora = DateTime.UtcNow;
        var pref = new PreferenciaDashboard
        {
            UsuarioId = usuarioId,
            LayoutJson = "{\"widgets\":[]}",
            AtualizadoEm = agora
        };

        pref.UsuarioId.Should().Be(usuarioId);
        pref.LayoutJson.Should().Be("{\"widgets\":[]}");
        pref.AtualizadoEm.Should().Be(agora);

        var command = new SalvarPreferenciaDashboardCommand("{\"widgets\":[]}");
        command.LayoutJson.Should().Be("{\"widgets\":[]}");

        var dto = new PreferenciaDashboardDto(usuarioId, "{\"widgets\":[]}", agora);
        dto.UsuarioId.Should().Be(usuarioId);
        dto.LayoutJson.Should().Be("{\"widgets\":[]}");
        dto.AtualizadoEm.Should().Be(agora);
    }
}
