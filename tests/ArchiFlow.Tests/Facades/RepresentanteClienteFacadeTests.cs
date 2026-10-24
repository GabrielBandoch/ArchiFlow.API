using ArchiFlow.Application.Clientes.Commands;
using ArchiFlow.Application.Clientes.DTOs;
using ArchiFlow.Application.Clientes.Facades;
using ArchiFlow.Application.Interfaces.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Facades;

public class RepresentanteClienteFacadeTests
{
    private readonly Mock<IRepresentanteClienteService> _serviceMock = new();
    private readonly RepresentanteClienteFacade _facade;

    public RepresentanteClienteFacadeTests()
    {
        _facade = new RepresentanteClienteFacade(_serviceMock.Object);
    }

    [Fact]
    public async Task ObterPorClienteIdAsync_DeveDelegarParaService()
    {
        var clienteId = Guid.NewGuid();
        var lista = new List<RepresentanteClienteDto>
        {
            new(Guid.NewGuid(), clienteId, "Representante 1", "rep1@teste.com", null, null, true, "Ativo", DateTime.UtcNow, null)
        };
        _serviceMock.Setup(s => s.ObterPorClienteIdAsync(clienteId)).ReturnsAsync(lista);

        var result = await _facade.ObterPorClienteIdAsync(clienteId);

        result.Should().BeEquivalentTo(lista);
        _serviceMock.Verify(s => s.ObterPorClienteIdAsync(clienteId), Times.Once);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveDelegarParaService()
    {
        var id = Guid.NewGuid();
        var dto = new RepresentanteClienteDto(id, Guid.NewGuid(), "Representante", "rep@teste.com", null, null, true, "Ativo", DateTime.UtcNow, null);
        _serviceMock.Setup(s => s.ObterPorIdAsync(id)).ReturnsAsync(dto);

        var result = await _facade.ObterPorIdAsync(id);

        result.Should().Be(dto);
        _serviceMock.Verify(s => s.ObterPorIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task ConvidarRepresentanteAsync_DeveDelegarParaService()
    {
        var clienteId = Guid.NewGuid();
        var cmd = new ConvidarRepresentanteCommand("Novo Rep", "novorep@teste.com");
        var baseUrl = "https://app.cotu.com.br";
        var dto = new RepresentanteClienteDto(Guid.NewGuid(), clienteId, "Novo Rep", "novorep@teste.com", null, null, true, "Pendente", DateTime.UtcNow, null);
        _serviceMock.Setup(s => s.ConvidarRepresentanteAsync(clienteId, cmd, baseUrl)).ReturnsAsync(dto);

        var result = await _facade.ConvidarRepresentanteAsync(clienteId, cmd, baseUrl);

        result.Should().Be(dto);
        _serviceMock.Verify(s => s.ConvidarRepresentanteAsync(clienteId, cmd, baseUrl), Times.Once);
    }

    [Fact]
    public async Task ReenviarConviteAsync_DeveDelegarParaService()
    {
        var repId = Guid.NewGuid();
        var baseUrl = "https://app.cotu.com.br";
        _serviceMock.Setup(s => s.ReenviarConviteAsync(repId, baseUrl)).Returns(Task.CompletedTask);

        await _facade.ReenviarConviteAsync(repId, baseUrl);

        _serviceMock.Verify(s => s.ReenviarConviteAsync(repId, baseUrl), Times.Once);
    }

    [Fact]
    public async Task AlterarStatusAsync_DeveDelegarParaService()
    {
        var repId = Guid.NewGuid();
        var dto = new RepresentanteClienteDto(repId, Guid.NewGuid(), "Rep", "rep@teste.com", null, null, false, "Inativo", DateTime.UtcNow, null);
        _serviceMock.Setup(s => s.AlterarStatusAsync(repId, false)).ReturnsAsync(dto);

        var result = await _facade.AlterarStatusAsync(repId, false);

        result.Should().Be(dto);
        _serviceMock.Verify(s => s.AlterarStatusAsync(repId, false), Times.Once);
    }

    [Fact]
    public async Task RemoverRepresentanteAsync_DeveDelegarParaService()
    {
        var repId = Guid.NewGuid();
        _serviceMock.Setup(s => s.RemoverRepresentanteAsync(repId)).Returns(Task.CompletedTask);

        await _facade.RemoverRepresentanteAsync(repId);

        _serviceMock.Verify(s => s.RemoverRepresentanteAsync(repId), Times.Once);
    }

    [Fact]
    public async Task ValidarTokenConviteAsync_DeveDelegarParaService()
    {
        var token = "token123";
        var dto = new ResultadoValidacaoConviteDto(true, "Escritório", "rep@teste.com", null);
        _serviceMock.Setup(s => s.ValidarTokenConviteAsync(token)).ReturnsAsync(dto);

        var result = await _facade.ValidarTokenConviteAsync(token);

        result.Should().Be(dto);
        _serviceMock.Verify(s => s.ValidarTokenConviteAsync(token), Times.Once);
    }

    [Fact]
    public async Task AtivarContaAsync_DeveDelegarParaService()
    {
        var cmd = new AtivarContaRepresentanteCommand("token123", "SenhaForte@123");
        _serviceMock.Setup(s => s.AtivarContaAsync(cmd)).Returns(Task.CompletedTask);

        await _facade.AtivarContaAsync(cmd);

        _serviceMock.Verify(s => s.AtivarContaAsync(cmd), Times.Once);
    }
}

