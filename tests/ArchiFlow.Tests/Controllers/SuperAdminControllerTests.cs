using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Application.SuperAdmin.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Controllers;

public class SuperAdminControllerTests
{
    private readonly Mock<ISuperAdminService> _serviceMock;
    private readonly SuperAdminController _controller;

    public SuperAdminControllerTests()
    {
        _serviceMock = new Mock<ISuperAdminService>();
        _controller = new SuperAdminController(_serviceMock.Object);
    }

    [Fact]
    public async Task ObterKpis_DeveRetornarOkComKpis()
    {
        var kpis = new SuperAdminDashboardKpisDto
        {
            TotalEscritorios = 10,
            TotalAtivos = 8,
            TotalInadimplentes = 2,
            TotalBloqueados = 1,
            MrrTotal = 3500m,
            ValorTotalEmAtraso = 700m
        };

        _serviceMock.Setup(s => s.ObterKpisAsync()).ReturnsAsync(kpis);

        var result = await _controller.ObterKpis();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(kpis);
    }

    [Fact]
    public async Task ListarEscritorios_DeveRetornarOkComLista()
    {
        var lista = new List<TenantAssinaturaDto>
        {
            new() { Id = Guid.NewGuid(), NomeEscritorio = "Escritorio 1", Subdominio = "esc1" }
        };

        _serviceMock.Setup(s => s.ListarEscritoriosAsync(null, null)).ReturnsAsync(lista);

        var result = await _controller.ListarEscritorios(null, null);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(lista);
    }

    [Fact]
    public async Task ObterEscritorio_QuandoExiste_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var dto = new TenantAssinaturaDto { Id = id, NomeEscritorio = "Escritorio Teste" };

        _serviceMock.Setup(s => s.ObterEscritorioPorIdAsync(id)).ReturnsAsync(dto);

        var result = await _controller.ObterEscritorio(id);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task ObterEscritorio_QuandoNaoExiste_DeveRetornarNotFound()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.ObterEscritorioPorIdAsync(id)).ReturnsAsync((TenantAssinaturaDto?)null);

        var result = await _controller.ObterEscritorio(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CriarEscritorio_ComSucesso_DeveRetornarCreatedAtAction()
    {
        var request = new CriarTenantRequestDto
        {
            NomeEscritorio = "Novo",
            Subdominio = "novo",
            EmailAdministrador = "admin@novo.com",
            SenhaInicial = "123456",
            ValorMensalidade = 300m
        };

        var dto = new TenantAssinaturaDto { Id = Guid.NewGuid(), Subdominio = "novo" };
        _serviceMock.Setup(s => s.CriarEscritorioAsync(request)).ReturnsAsync(dto);

        var result = await _controller.CriarEscritorio(request);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task CriarEscritorio_QuandoSubdominioDuplicado_DeveRetornarBadRequest()
    {
        var request = new CriarTenantRequestDto
        {
            Subdominio = "duplicado"
        };

        _serviceMock.Setup(s => s.CriarEscritorioAsync(request))
            .ThrowsAsync(new InvalidOperationException("Subdominio ja existente"));

        var result = await _controller.CriarEscritorio(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AlternarBloqueio_QuandoExiste_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var request = new BloquearTenantRequestDto { Bloquear = true, Motivo = "Inadimplencia" };
        var dto = new TenantAssinaturaDto { Id = id, AcessoBloqueado = true };

        _serviceMock.Setup(s => s.AlternarBloqueioAsync(id, request)).ReturnsAsync(dto);

        var result = await _controller.AlternarBloqueio(id, request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task RegistrarPagamento_QuandoExiste_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var dto = new TenantAssinaturaDto { Id = id, StatusFinanceiro = "Adimplente" };

        _serviceMock.Setup(s => s.RegistrarPagamentoAsync(id)).ReturnsAsync(dto);

        var result = await _controller.RegistrarPagamento(id);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(dto);
    }
}
