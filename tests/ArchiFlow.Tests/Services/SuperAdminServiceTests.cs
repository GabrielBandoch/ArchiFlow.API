using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchiFlow.Application.SuperAdmin.DTOs;
using ArchiFlow.Application.SuperAdmin.Services;
using ArchiFlow.Domain.MultiTenancy;
using ArchiFlow.Domain.Shared;
using FluentAssertions;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Services;

public class SuperAdminServiceTests
{
    private readonly Mock<ITenantAssinaturaRepository> _repositoryMock;
    private readonly Mock<ITenantDatabaseProvisioner> _provisionerMock;
    private readonly SuperAdminService _service;

    public SuperAdminServiceTests()
    {
        _repositoryMock = new Mock<ITenantAssinaturaRepository>();
        _provisionerMock = new Mock<ITenantDatabaseProvisioner>();
        _service = new SuperAdminService(_repositoryMock.Object, _provisionerMock.Object);
    }

    [Fact]
    public async Task ObterKpis_DeveCalcularMetricasCorretamente()
    {
        var lista = new List<TenantAssinatura>
        {
            new() { Subdominio = "t1", StatusFinanceiro = "Adimplente", BloqueadoInadimplencia = false, ValorMensalidade = 300m },
            new() { Subdominio = "t2", StatusFinanceiro = "Adimplente", BloqueadoInadimplencia = false, ValorMensalidade = 500m },
            new() { Subdominio = "t3", StatusFinanceiro = "EmDebito", BloqueadoInadimplencia = false, ValorMensalidade = 200m },
            new() { Subdominio = "t4", StatusFinanceiro = "Suspenso", BloqueadoInadimplencia = true, ValorMensalidade = 400m }
        };

        _repositoryMock.Setup(r => r.ListarTodosAsync(null, null)).ReturnsAsync(lista);

        var kpis = await _service.ObterKpisAsync();

        kpis.TotalEscritorios.Should().Be(4);
        kpis.TotalAtivos.Should().Be(2);
        kpis.TotalInadimplentes.Should().Be(1);
        kpis.TotalBloqueados.Should().Be(1);
        kpis.MrrTotal.Should().Be(1000m);
        kpis.ValorTotalEmAtraso.Should().Be(200m);
    }

    [Fact]
    public async Task CriarEscritorio_QuandoSubdominioDisponivel_DeveCriarEProvisionar()
    {
        var request = new CriarTenantRequestDto
        {
            NomeEscritorio = "Studio Teste",
            Subdominio = "studio-teste",
            EmailAdministrador = "admin@studio.com",
            ValorMensalidade = 350m
        };

        _repositoryMock.Setup(r => r.ObterPorSubdominioAsync("studio-teste")).ReturnsAsync((TenantAssinatura?)null);

        var resultado = await _service.CriarEscritorioAsync(request);

        resultado.Subdominio.Should().Be("studio-teste");
        resultado.NomeEscritorio.Should().Be("Studio Teste");
        resultado.ValorMensalidade.Should().Be(350m);
        resultado.StatusFinanceiro.Should().Be("Trial");

        _repositoryMock.Verify(r => r.AdicionarAsync(It.Is<TenantAssinatura>(t => t.Subdominio == "studio-teste")), Times.Once);
        _provisionerMock.Verify(p => p.EnsureTenantDatabaseAsync("studio-teste"), Times.Once);
    }

    [Fact]
    public async Task CriarEscritorio_QuandoSubdominioJaExiste_DeveLancarExcecao()
    {
        var request = new CriarTenantRequestDto
        {
            Subdominio = "existente"
        };

        _repositoryMock.Setup(r => r.ObterPorSubdominioAsync("existente")).ReturnsAsync(new TenantAssinatura { Subdominio = "existente" });

        var act = () => _service.CriarEscritorioAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<TenantAssinatura>()), Times.Never);
        _provisionerMock.Verify(p => p.EnsureTenantDatabaseAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarPagamento_DeveNormalizarStatusParaAdimplente()
    {
        var id = Guid.NewGuid();
        var entity = new TenantAssinatura
        {
            Id = id,
            StatusFinanceiro = "EmDebito",
            BloqueadoInadimplencia = true,
            MotivoBloqueio = "Atraso"
        };

        _repositoryMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(entity);

        var resultado = await _service.RegistrarPagamentoAsync(id);

        resultado.Should().NotBeNull();
        resultado!.StatusFinanceiro.Should().Be("Adimplente");
        resultado.AcessoBloqueado.Should().BeFalse();
        entity.BloqueadoInadimplencia.Should().BeFalse();
        entity.UltimoPagamento.Should().NotBeNull();
        _repositoryMock.Verify(r => r.AtualizarAsync(entity), Times.Once);
    }
}
