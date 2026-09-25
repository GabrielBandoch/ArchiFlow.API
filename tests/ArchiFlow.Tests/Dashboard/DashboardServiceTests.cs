using ArchiFlow.Application.Dashboard.DTOs;
using ArchiFlow.Application.Dashboard.Services;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Leads;
using ArchiFlow.Domain.Leads.Enum;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Projetos.Enum;
using ArchiFlow.Infrastructure.Data;
using ArchiFlow.Infrastructure.Repositories;
using ArchiFlow.Infrastructure.Repositories.Clientes;
using ArchiFlow.Infrastructure.Repositories.Dashboard;
using ArchiFlow.Infrastructure.Repositories.Honorarios;
using ArchiFlow.Infrastructure.Repositories.Leads;
using ArchiFlow.Infrastructure.Repositories.Projetos;
using ArchiFlow.Tests.Common;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Dashboard;

public class DashboardServiceTests
{
    private static (DashboardService service, ArchiFlowDbContext context) CreateService()
    {
        var context = TestDbContextFactory.Create();
        var projetoRepo = new ProjetoRepository(context);
        var clienteRepo = new ClienteRepository(context);
        var leadRepo = new LeadRepository(context);
        var propostaRepo = new PropostaHonorarioRepository(context);
        var preferenciaRepo = new PreferenciaDashboardRepository(context);
        var unitOfWork = new UnitOfWork(context);
        var service = new DashboardService(projetoRepo, clienteRepo, leadRepo, propostaRepo, preferenciaRepo, unitOfWork);

        return (service, context);
    }

    [Fact]
    public async Task ObterMetricasAsync_When_Database_Is_Empty_Should_Return_Zeroed_Metrics()
    {
        var (service, _) = CreateService();

        var resultado = await service.ObterMetricasAsync();

        resultado.Should().NotBeNull();
        resultado.Kpis.TotalProjetosAtivos.Should().Be(0);
        resultado.Kpis.TotalProjetosConcluidos.Should().Be(0);
        resultado.Kpis.TotalLeadsAtivos.Should().Be(0);
        resultado.Kpis.TotalLeadsConvertidos.Should().Be(0);
        resultado.Kpis.TaxaConversaoLeads.Should().Be(0m);
        resultado.Kpis.TotalClientes.Should().Be(0);
        resultado.Kpis.TotalPropostas.Should().Be(0);
        resultado.Kpis.ValorTotalPropostas.Should().Be(0m);
        resultado.ProjetosPorStatus.Should().HaveCount(7);
        resultado.ProjetosPorTipo.Should().HaveCount(4);
        resultado.LeadsPorStatus.Should().HaveCount(6);
        resultado.PropostasMensais.Should().HaveCount(6);
        resultado.ProjetosRecentes.Should().BeEmpty();
        resultado.LeadsRecentes.Should().BeEmpty();
        resultado.PropostasRecentes.Should().BeEmpty();
    }

    [Fact]
    public async Task ObterMetricasAsync_With_Data_Should_Calculate_Kpis_And_Distributions_Accurately()
    {
        var (service, context) = CreateService();

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = "Cliente Alpha",
            Email = "alpha@teste.com",
            Ativo = true
        };
        await context.Clientes.AddAsync(cliente);

        var origem = new OrigemLead
        {
            Id = Guid.NewGuid(),
            Descricao = "Instagram",
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };
        await context.OrigensLead.AddAsync(origem);

        var lead1 = new Lead
        {
            Id = Guid.NewGuid(),
            Nome = "Lead 1",
            Email = "lead1@teste.com",
            Status = StatusLead.Novo,
            OrigemId = origem.Id,
            Origem = origem,
            CriadoEm = DateTime.UtcNow
        };
        var lead2 = new Lead
        {
            Id = Guid.NewGuid(),
            Nome = "Lead 2",
            Email = "lead2@teste.com",
            Status = StatusLead.Convertido,
            OrigemId = origem.Id,
            Origem = origem,
            CriadoEm = DateTime.UtcNow
        };
        await context.Leads.AddRangeAsync(lead1, lead2);

        var projeto = new Projeto
        {
            Id = Guid.NewGuid(),
            Nome = "Projeto Residencial Luxo",
            ClienteId = cliente.Id,
            Status = StatusProjeto.Desenvolvimento,
            Tipo = TipoProjeto.Residencial,
            MetragemTotal = 250m,
            CriadoEm = DateTime.UtcNow,
            Etapas = new List<EtapaProjeto>
            {
                new() { Id = Guid.NewGuid(), Nome = "Estudo Preliminar", Status = StatusEtapa.Concluida, Ordem = 1 },
                new() { Id = Guid.NewGuid(), Nome = "Anteprojeto", Status = StatusEtapa.Pendente, Ordem = 2 }
            }
        };
        await context.Projetos.AddAsync(projeto);

        var proposta = new PropostaHonorario
        {
            Id = Guid.NewGuid(),
            Titulo = "Proposta Alpha",
            Codigo = "PROP-2026-001",
            ClienteId = cliente.Id,
            ClienteNome = cliente.Nome,
            TipoProjeto = TipoProjeto.Residencial,
            PadraoImovel = PadraoImovel.AltoPadrao,
            MetragemQuadrada = 250m,
            ValorTotalSugerido = 18000m,
            ValorFinalAjustado = 17500m,
            Status = StatusProposta.Enviada,
            CriadoEm = DateTime.UtcNow
        };
        await context.PropostasHonorarios.AddAsync(proposta);

        await context.SaveChangesAsync();

        var resultado = await service.ObterMetricasAsync();

        resultado.Should().NotBeNull();
        resultado.Kpis.TotalProjetosAtivos.Should().Be(1);
        resultado.Kpis.TotalProjetosConcluidos.Should().Be(0);
        resultado.Kpis.TotalLeadsAtivos.Should().Be(1);
        resultado.Kpis.TotalLeadsConvertidos.Should().Be(1);
        resultado.Kpis.TaxaConversaoLeads.Should().Be(50.0m);
        resultado.Kpis.TotalClientes.Should().Be(1);
        resultado.Kpis.TotalPropostas.Should().Be(1);
        resultado.Kpis.ValorTotalPropostas.Should().Be(17500m);
        resultado.Kpis.ValorMedioProposta.Should().Be(17500m);

        resultado.ProjetosRecentes.Should().HaveCount(1);
        resultado.ProjetosRecentes[0].Nome.Should().Be("Projeto Residencial Luxo");
        resultado.ProjetosRecentes[0].ClienteNome.Should().Be("Cliente Alpha");
        resultado.ProjetosRecentes[0].ProgressoPercentual.Should().Be(50m);

        resultado.LeadsRecentes.Should().HaveCount(2);
        resultado.PropostasRecentes.Should().HaveCount(1);
    }

    [Fact]
    public async Task Salvar_And_Obter_Preferencias_Should_Persist_Layout_In_Database()
    {
        var (service, _) = CreateService();
        var usuarioId = Guid.NewGuid();

        var layoutJson = "{\"widgets\":[{\"id\":\"kpi-projetos\",\"visible\":true,\"order\":1}]}";
        var command = new SalvarPreferenciaDashboardCommand(layoutJson);

        var saved = await service.SalvarPreferenciasAsync(usuarioId, command);
        saved.Should().NotBeNull();
        saved.UsuarioId.Should().Be(usuarioId);
        saved.LayoutJson.Should().Be(layoutJson);

        var fetched = await service.ObterPreferenciasAsync(usuarioId);
        fetched.Should().NotBeNull();
        fetched!.LayoutJson.Should().Be(layoutJson);

        var updateCommand = new SalvarPreferenciaDashboardCommand("{\"widgets\":[]}");
        var updated = await service.SalvarPreferenciasAsync(usuarioId, updateCommand);
        updated.LayoutJson.Should().Be("{\"widgets\":[]}");
    }
}
