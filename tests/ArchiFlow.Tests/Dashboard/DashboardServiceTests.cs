using ArchiFlow.Application.Dashboard.DTOs;
using ArchiFlow.Application.Dashboard.Services;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Leads;
using ArchiFlow.Domain.Leads.Enum;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Projetos.Enum;
using ArchiFlow.Infrastructure.Data;
using ArchiFlow.Infrastructure.Repositories;
using ArchiFlow.Infrastructure.Repositories.Clientes;
using ArchiFlow.Infrastructure.Repositories.Dashboard;
using ArchiFlow.Infrastructure.Repositories.Leads;
using ArchiFlow.Infrastructure.Repositories.Projetos;
using ArchiFlow.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Dashboard;

public class DashboardServiceTests
{
    private static (DashboardService service, ArchiFlowDbContext context) CreateService(IHttpContextAccessor? httpContextAccessor = null)
    {
        var context = TestDbContextFactory.Create();
        var projetoRepo = new ProjetoRepository(context);
        var clienteRepo = new ClienteRepository(context);
        var leadRepo = new LeadRepository(context);
        var preferenciaRepo = new PreferenciaDashboardRepository(context);
        var unitOfWork = new UnitOfWork(context);
        var service = new DashboardService(projetoRepo, clienteRepo, leadRepo, preferenciaRepo, unitOfWork, httpContextAccessor);

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
        resultado.ProjetosPorStatus.Should().HaveCount(7);
        resultado.ProjetosPorTipo.Should().HaveCount(4);
        resultado.LeadsPorStatus.Should().HaveCount(6);
        resultado.ProjetosRecentes.Should().BeEmpty();
        resultado.LeadsRecentes.Should().BeEmpty();
    }

    [Fact]
    public async Task ObterMetricasAsync_With_Data_Should_Calculate_Kpis_And_Distributions_Accurately()
    {
        var (service, context) = CreateService();

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = "Cliente Alpha",
            Email = "alpha@cliente.com",
            Ativo = true
        };
        await context.Clientes.AddAsync(cliente);

        var origem = new OrigemLead
        {
            Id = Guid.NewGuid(),
            Descricao = "Instagram",
            Ativo = true
        };
        await context.OrigensLead.AddAsync(origem);

        var lead1 = new Lead
        {
            Id = Guid.NewGuid(),
            Nome = "Lead 1",
            Email = "lead1@teste.com",
            Status = StatusLead.Convertido,
            OrigemId = origem.Id,
            Origem = origem,
            CriadoEm = DateTime.UtcNow
        };

        var lead2 = new Lead
        {
            Id = Guid.NewGuid(),
            Nome = "Lead 2",
            Email = "lead2@teste.com",
            Status = StatusLead.EmContato,
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

        await context.SaveChangesAsync();

        var resultado = await service.ObterMetricasAsync();

        resultado.Should().NotBeNull();
        resultado.Kpis.TotalProjetosAtivos.Should().Be(1);
        resultado.Kpis.TotalProjetosConcluidos.Should().Be(0);
        resultado.Kpis.TotalLeadsAtivos.Should().Be(1);
        resultado.Kpis.TotalLeadsConvertidos.Should().Be(1);
        resultado.Kpis.TaxaConversaoLeads.Should().Be(50.0m);
        resultado.Kpis.TotalClientes.Should().Be(1);

        resultado.ProjetosRecentes.Should().HaveCount(1);
        resultado.ProjetosRecentes[0].Nome.Should().Be("Projeto Residencial Luxo");
        resultado.ProjetosRecentes[0].ClienteNome.Should().Be("Cliente Alpha");
        resultado.ProjetosRecentes[0].ProgressoPercentual.Should().Be(50m);

        resultado.LeadsRecentes.Should().HaveCount(2);
    }

    [Fact]
    public async Task ObterMetricasAsync_Lead_Without_Origem_Should_Fallback_To_Direto()
    {
        var (service, context) = CreateService();

        var leadSemOrigem = new Lead
        {
            Id = Guid.NewGuid(),
            Nome = "Lead Direto",
            Email = "direto@teste.com",
            Status = StatusLead.Novo,
            OrigemId = null,
            Origem = null,
            CriadoEm = DateTime.UtcNow
        };
        await context.Leads.AddAsync(leadSemOrigem);
        await context.SaveChangesAsync();

        var resultado = await service.ObterMetricasAsync();

        resultado.LeadsPorOrigem.Should().Contain(o => o.Origem == "Direto");
        resultado.LeadsRecentes[0].OrigemNome.Should().Be("Direto");
    }

    [Fact]
    public async Task ObterMetricasAsync_Active_Leads_KPI_Should_Exclude_Converted_And_Lost()
    {
        var (service, context) = CreateService();

        var leads = new List<Lead>
        {
            new() { Id = Guid.NewGuid(), Nome = "L1", Email = "l1@t.com", Status = StatusLead.Novo, CriadoEm = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Nome = "L2", Email = "l2@t.com", Status = StatusLead.EmContato, CriadoEm = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Nome = "L3", Email = "l3@t.com", Status = StatusLead.Negociando, CriadoEm = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Nome = "L4", Email = "l4@t.com", Status = StatusLead.Convertido, CriadoEm = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Nome = "L5", Email = "l5@t.com", Status = StatusLead.Perdido, CriadoEm = DateTime.UtcNow }
        };

        await context.Leads.AddRangeAsync(leads);
        await context.SaveChangesAsync();

        var resultado = await service.ObterMetricasAsync();

        resultado.Kpis.TotalLeadsAtivos.Should().Be(3);
        resultado.Kpis.TotalLeadsConvertidos.Should().Be(1);
    }

    private static HttpContextAccessor CreateHttpContextAccessor(string? claimType = null, string? claimValue = null)
    {
        var httpContext = new DefaultHttpContext();
        if (!string.IsNullOrEmpty(claimType) && !string.IsNullOrEmpty(claimValue))
        {
            var claims = new Claim[] { new(claimType, claimValue) };
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }
        return new HttpContextAccessor { HttpContext = httpContext };
    }

    [Fact]
    public async Task Salvar_And_Obter_Preferencias_With_Valid_User_And_Layout_Should_Persist()
    {
        var usuarioId = Guid.NewGuid();
        var accessor = CreateHttpContextAccessor(ClaimTypes.NameIdentifier, usuarioId.ToString());
        var (service, _) = CreateService(accessor);

        var layoutJson = "[{\"id\":\"kpi_resumo\",\"ordem\":1,\"visivel\":true,\"largura\":\"full\"}]";
        var command = new SalvarPreferenciaDashboardCommand(layoutJson);

        var saved = await service.SalvarPreferenciasAsync(command);
        saved.Should().NotBeNull();
        saved.UsuarioId.Should().Be(usuarioId);
        saved.LayoutJson.Should().Be(layoutJson);

        var fetched = await service.ObterPreferenciasAsync();
        fetched.Should().NotBeNull();
        fetched!.LayoutJson.Should().Be(layoutJson);

        var updateJson = "[{\"id\":\"kpi_resumo\",\"ordem\":1,\"visivel\":true,\"largura\":\"full\"},{\"id\":\"atalhos_rapidos\",\"ordem\":2,\"visivel\":false,\"largura\":\"half\"}]";
        var updated = await service.SalvarPreferenciasAsync(new SalvarPreferenciaDashboardCommand(updateJson));
        updated.LayoutJson.Should().Be(updateJson);
    }

    [Fact]
    public async Task ObterPreferenciasAsync_When_No_Preferences_Exist_Should_Return_Null()
    {
        var usuarioId = Guid.NewGuid();
        var accessor = CreateHttpContextAccessor(ClaimTypes.NameIdentifier, usuarioId.ToString());
        var (service, _) = CreateService(accessor);

        var resultado = await service.ObterPreferenciasAsync();
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObterPreferencias_Without_User_Should_Throw_UnauthorizedAccessException()
    {
        var (service, _) = CreateService(null);
        var act = async () => await service.ObterPreferenciasAsync();
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ObterPreferencias_With_Missing_Or_Invalid_Claim_Should_Throw_UnauthorizedAccessException()
    {
        var accessorInvalid = CreateHttpContextAccessor(ClaimTypes.NameIdentifier, "not-a-guid");
        var (serviceInvalid, _) = CreateService(accessorInvalid);
        var actInvalid = async () => await serviceInvalid.ObterPreferenciasAsync();
        await actInvalid.Should().ThrowAsync<UnauthorizedAccessException>();

        var accessorEmpty = CreateHttpContextAccessor(ClaimTypes.NameIdentifier, Guid.Empty.ToString());
        var (serviceEmpty, _) = CreateService(accessorEmpty);
        var actEmpty = async () => await serviceEmpty.ObterPreferenciasAsync();
        await actEmpty.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ObterPreferencias_With_Sub_Claim_Fallback_Should_Succeed()
    {
        var usuarioId = Guid.NewGuid();
        var accessorSub = CreateHttpContextAccessor("sub", usuarioId.ToString());
        var (service, _) = CreateService(accessorSub);

        var resultado = await service.ObterPreferenciasAsync();
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task SalvarPreferencias_Without_User_Should_Throw_UnauthorizedAccessException_And_Never_Persist_GuidEmpty()
    {
        var (service, context) = CreateService(null);
        var command = new SalvarPreferenciaDashboardCommand("[{\"id\":\"kpi_resumo\",\"ordem\":1,\"visivel\":true,\"largura\":\"full\"}]");

        var act = async () => await service.SalvarPreferenciasAsync(command);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();

        context.PreferenciasDashboard.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not a json")]
    [InlineData("{\"id\":\"kpi_resumo\"}")]
    [InlineData("[]")]
    [InlineData("[{\"id\":\"widget_desconhecido\",\"ordem\":1,\"visivel\":true}]")]
    [InlineData("[{\"id\":\"kpi_resumo\",\"ordem\":1},{\"id\":\"kpi_resumo\",\"ordem\":2}]")]
    [InlineData("[{\"id\":\"kpi_resumo\",\"ordem\":0}]")]
    [InlineData("[{\"id\":\"kpi_resumo\",\"ordem\":99}]")]
    [InlineData("[{\"id\":\"kpi_resumo\",\"largura\":\"quarter\"}]")]
    public async Task SalvarPreferencias_With_Invalid_Layout_Should_Throw_ArgumentException(string invalidLayout)
    {
        var usuarioId = Guid.NewGuid();
        var accessor = CreateHttpContextAccessor(ClaimTypes.NameIdentifier, usuarioId.ToString());
        var (service, context) = CreateService(accessor);

        var act = async () => await service.SalvarPreferenciasAsync(new SalvarPreferenciaDashboardCommand(invalidLayout));
        await act.Should().ThrowAsync<ArgumentException>();

        context.PreferenciasDashboard.Should().BeEmpty();
    }

    [Fact]
    public async Task SalvarPreferencias_Payload_Too_Large_Should_Throw_ArgumentException()
    {
        var usuarioId = Guid.NewGuid();
        var accessor = CreateHttpContextAccessor(ClaimTypes.NameIdentifier, usuarioId.ToString());
        var (service, context) = CreateService(accessor);

        var largeComment = new string('x', 17000);
        var act = async () => await service.SalvarPreferenciasAsync(new SalvarPreferenciaDashboardCommand(largeComment));
        await act.Should().ThrowAsync<ArgumentException>();

        context.PreferenciasDashboard.Should().BeEmpty();
    }
}
