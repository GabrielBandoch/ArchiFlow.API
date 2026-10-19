using ArchiFlow.Application.Empresa.Commands;
using ArchiFlow.Application.Empresa.DTOs;
using ArchiFlow.Application.Empresa.Facades;
using ArchiFlow.Application.Interfaces.Services;
using FluentAssertions;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Facades;

public class EmpresaFacadeTests
{
    private readonly Mock<IEmpresaService> _serviceMock = new();
    private readonly EmpresaFacade _facade;

    public EmpresaFacadeTests()
    {
        _facade = new EmpresaFacade(_serviceMock.Object);
    }

    private static DadosEmpresaDto CriarDto(string nome = "Studio Teste", string cor = "#2563EB") =>
        new(Guid.NewGuid(), null, nome, null, null, null, null, null, null, null, null, null, null, null, null, cor, DateTime.UtcNow);

    [Fact]
    public async Task ObterDadosEmpresaAsync_DeveDelegarParaService()
    {
        var dto = CriarDto();
        _serviceMock.Setup(s => s.ObterDadosEmpresaAsync()).ReturnsAsync(dto);

        var result = await _facade.ObterDadosEmpresaAsync();

        result.Should().Be(dto);
        _serviceMock.Verify(s => s.ObterDadosEmpresaAsync(), Times.Once);
    }

    [Fact]
    public async Task SalvarDadosEmpresaAsync_DeveDelegarParaService()
    {
        var cmd = new SalvarDadosEmpresaCommand("Studio Arquitetura");
        var dto = CriarDto("Studio Arquitetura");
        _serviceMock.Setup(s => s.SalvarDadosEmpresaAsync(cmd)).ReturnsAsync(dto);

        var result = await _facade.SalvarDadosEmpresaAsync(cmd);

        result.Should().Be(dto);
        _serviceMock.Verify(s => s.SalvarDadosEmpresaAsync(cmd), Times.Once);
    }

    [Fact]
    public async Task AtualizarCorPrimariaAsync_DeveDelegarParaService()
    {
        var cor = "#10B981";
        var dto = CriarDto(cor: cor);
        _serviceMock.Setup(s => s.AtualizarCorPrimariaAsync(cor)).ReturnsAsync(dto);

        var result = await _facade.AtualizarCorPrimariaAsync(cor);

        result.Should().Be(dto);
        _serviceMock.Verify(s => s.AtualizarCorPrimariaAsync(cor), Times.Once);
    }

    [Fact]
    public async Task AtualizarLogotipoAsync_DeveDelegarParaService()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("fake-logo"));
        var url = "https://storage.cotu.com.br/empresa/logo.png";
        _serviceMock.Setup(s => s.AtualizarLogotipoAsync(stream, "logo.png", "image/png")).ReturnsAsync(url);

        var result = await _facade.AtualizarLogotipoAsync(stream, "logo.png", "image/png");

        result.Should().Be(url);
        _serviceMock.Verify(s => s.AtualizarLogotipoAsync(stream, "logo.png", "image/png"), Times.Once);
    }

    [Fact]
    public async Task RemoverLogotipoAsync_DeveDelegarParaService()
    {
        _serviceMock.Setup(s => s.RemoverLogotipoAsync()).Returns(Task.CompletedTask);

        await _facade.RemoverLogotipoAsync();

        _serviceMock.Verify(s => s.RemoverLogotipoAsync(), Times.Once);
    }
}
