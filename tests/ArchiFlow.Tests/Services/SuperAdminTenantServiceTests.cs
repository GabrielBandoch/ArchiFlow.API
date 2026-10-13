using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchiFlow.Application.SuperAdmin.DTOs;
using ArchiFlow.Application.SuperAdmin.Services;
using ArchiFlow.Domain.Assinaturas;
using ArchiFlow.Domain.Shared;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Services;

public class SuperAdminTenantServiceTests
{
    private readonly Mock<ITenantAssinaturaRepository> _repositoryMock;
    private readonly Mock<ITenantDatabaseProvisioner> _provisionerMock;
    private readonly Mock<ILogger<SuperAdminTenantService>> _loggerMock;
    private readonly SuperAdminTenantService _service;

    public SuperAdminTenantServiceTests()
    {
        _repositoryMock = new Mock<ITenantAssinaturaRepository>();
        _provisionerMock = new Mock<ITenantDatabaseProvisioner>();
        _loggerMock = new Mock<ILogger<SuperAdminTenantService>>();
        _service = new SuperAdminTenantService(_repositoryMock.Object, _provisionerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ObterKpisAsync_Should_Calculate_Totals_Correctly()
    {
        // Arrange
        var lista = new List<TenantAssinatura>
        {
            new TenantAssinatura { Id = Guid.NewGuid(), StatusFinanceiro = "Adimplente", ValorMensalidade = 349m },
            new TenantAssinatura { Id = Guid.NewGuid(), StatusFinanceiro = "Adimplente", ValorMensalidade = 189.9m },
            new TenantAssinatura { Id = Guid.NewGuid(), StatusFinanceiro = "EmDebito", ValorMensalidade = 349m },
            new TenantAssinatura { Id = Guid.NewGuid(), StatusFinanceiro = "Suspenso", BloqueadoPorInadimplencia = true, ValorMensalidade = 599m },
            new TenantAssinatura { Id = Guid.NewGuid(), StatusFinanceiro = "Trial", ValorMensalidade = 349m }
        };

        _repositoryMock.Setup(r => r.ListarTodosAsync(null, null)).ReturnsAsync(lista);

        // Act
        var kpis = await _service.ObterKpisAsync();

        // Assert
        kpis.TotalEscritorios.Should().Be(5);
        kpis.TotalAdimplentes.Should().Be(2);
        kpis.TotalEmDebito.Should().Be(1);
        kpis.TotalSuspensos.Should().Be(1);
        kpis.TotalTrial.Should().Be(1);
        kpis.TotalEmAtrasoReais.Should().Be(349m + 599m);
    }

    [Fact]
    public async Task CriarEscritorioAsync_Should_Create_And_Call_Provisioner()
    {
        // Arrange
        var request = new CriarTenantAssinaturaRequestDto
        {
            Subdominio = "meu-studio",
            NomeEscritorio = "Meu Studio",
            NomeResponsavel = "Responsavel",
            EmailResponsavel = "resp@studio.com",
            Plano = "Professional",
            ValorMensalidade = 349m,
            StatusFinanceiro = "Adimplente"
        };

        _repositoryMock.Setup(r => r.SubdominioExisteAsync("meu-studio", null)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<TenantAssinatura>())).Returns(Task.CompletedTask);
        _provisionerMock.Setup(p => p.EnsureTenantDatabaseAsync("meu-studio")).Returns(Task.CompletedTask);

        // Act
        var result = await _service.CriarEscritorioAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Subdominio.Should().Be("meu-studio");
        result.NomeEscritorio.Should().Be("Meu Studio");
        _provisionerMock.Verify(p => p.EnsureTenantDatabaseAsync("meu-studio"), Times.Once);
        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<TenantAssinatura>()), Times.Once);
    }

    [Fact]
    public async Task AlternarBloqueioAcessoAsync_Should_Block_And_Set_Status_Suspenso()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new TenantAssinatura
        {
            Id = id,
            Subdominio = "studio-alfa",
            BloqueadoPorInadimplencia = false,
            StatusFinanceiro = "EmDebito"
        };

        _repositoryMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(entity);
        _repositoryMock.Setup(r => r.AtualizarAsync(entity)).Returns(Task.CompletedTask);

        // Act
        var result = await _service.AlternarBloqueioAcessoAsync(id, new BloquearTenantRequestDto { Bloquear = true, Motivo = "Débito" });

        // Assert
        result.BloqueadoPorInadimplencia.Should().BeTrue();
        result.StatusFinanceiro.Should().Be("Suspenso");
        entity.MotivoBloqueio.Should().Be("Débito");
    }

    [Fact]
    public async Task RegistrarPagamentoAsync_Should_Set_Adimplente_And_Unblock()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new TenantAssinatura
        {
            Id = id,
            Subdominio = "studio-beta",
            BloqueadoPorInadimplencia = true,
            StatusFinanceiro = "Suspenso",
            CicloFaturamento = "Mensal"
        };

        _repositoryMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(entity);
        _repositoryMock.Setup(r => r.AtualizarAsync(entity)).Returns(Task.CompletedTask);

        // Act
        var result = await _service.RegistrarPagamentoAsync(id);

        // Assert
        result.StatusFinanceiro.Should().Be("Adimplente");
        result.BloqueadoPorInadimplencia.Should().BeFalse();
        result.UltimoPagamento.Should().NotBeNull();
    }
}
