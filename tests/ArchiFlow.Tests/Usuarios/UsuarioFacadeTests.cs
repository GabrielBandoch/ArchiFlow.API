using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Application.Usuarios.Commands;
using ArchiFlow.Application.Usuarios.DTOs;
using ArchiFlow.Application.Usuarios.Facades;
using FluentAssertions;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Usuarios;

public class UsuarioFacadeTests
{
    private readonly Mock<IUsuarioService> _serviceMock;
    private readonly UsuarioFacade _facade;

    public UsuarioFacadeTests()
    {
        _serviceMock = new Mock<IUsuarioService>();
        _facade = new UsuarioFacade(_serviceMock.Object);
    }

    [Fact]
    public async Task ObterEquipeAsync_DeveDelegarParaService()
    {
        var lista = new List<MembroEquipeDto>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Nome", "email@teste.com", "Colaborador", "Cargo", null, true, DateTime.UtcNow, null)
        };
        _serviceMock.Setup(s => s.ObterEquipeAsync()).ReturnsAsync(lista);

        var result = await _facade.ObterEquipeAsync();

        result.Should().BeEquivalentTo(lista);
        _serviceMock.Verify(s => s.ObterEquipeAsync(), Times.Once);
    }

    [Fact]
    public async Task ObterMembroPorIdAsync_DeveDelegarParaService()
    {
        var id = Guid.NewGuid();
        var dto = new MembroEquipeDto(id, Guid.NewGuid(), "Nome", "email@teste.com", "Colaborador", null, null, true, DateTime.UtcNow, null);
        _serviceMock.Setup(s => s.ObterMembroPorIdAsync(id)).ReturnsAsync(dto);

        var result = await _facade.ObterMembroPorIdAsync(id);

        result.Should().Be(dto);
        _serviceMock.Verify(s => s.ObterMembroPorIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task ConvidarMembroAsync_DeveDelegarParaService()
    {
        var cmd = new ConvidarMembroEquipeCommand("Nome", "email@teste.com", "Colaborador", "Cargo", null, null);
        var dto = new MembroEquipeDto(Guid.NewGuid(), Guid.NewGuid(), "Nome", "email@teste.com", "Colaborador", "Cargo", null, true, DateTime.UtcNow, null);
        _serviceMock.Setup(s => s.ConvidarMembroAsync(cmd)).ReturnsAsync(dto);

        var result = await _facade.ConvidarMembroAsync(cmd);

        result.Should().Be(dto);
        _serviceMock.Verify(s => s.ConvidarMembroAsync(cmd), Times.Once);
    }

    [Fact]
    public async Task AtualizarMembroAsync_DeveDelegarParaService()
    {
        var id = Guid.NewGuid();
        var cmd = new AtualizarMembroEquipeCommand("Nome", "Colaborador", "Cargo", null, "email@teste.com");
        var dto = new MembroEquipeDto(id, Guid.NewGuid(), "Nome", "email@teste.com", "Colaborador", "Cargo", null, true, DateTime.UtcNow, null);
        _serviceMock.Setup(s => s.AtualizarMembroAsync(id, cmd)).ReturnsAsync(dto);

        var result = await _facade.AtualizarMembroAsync(id, cmd);

        result.Should().Be(dto);
        _serviceMock.Verify(s => s.AtualizarMembroAsync(id, cmd), Times.Once);
    }

    [Fact]
    public async Task AlterarStatusMembroAsync_DeveDelegarParaService()
    {
        var id = Guid.NewGuid();
        var cmd = new AlterarStatusMembroCommand(false);
        var dto = new MembroEquipeDto(id, Guid.NewGuid(), "Nome", "email@teste.com", "Colaborador", null, null, false, DateTime.UtcNow, null);
        _serviceMock.Setup(s => s.AlterarStatusMembroAsync(id, cmd)).ReturnsAsync(dto);

        var result = await _facade.AlterarStatusMembroAsync(id, cmd);

        result.Should().Be(dto);
        _serviceMock.Verify(s => s.AlterarStatusMembroAsync(id, cmd), Times.Once);
    }

    [Fact]
    public async Task RedefinirSenhaMembroAsync_DeveDelegarParaService()
    {
        var id = Guid.NewGuid();
        var cmd = new RedefinirSenhaMembroCommand("NovaSenha123");
        _serviceMock.Setup(s => s.RedefinirSenhaMembroAsync(id, cmd)).Returns(Task.CompletedTask);

        await _facade.RedefinirSenhaMembroAsync(id, cmd);

        _serviceMock.Verify(s => s.RedefinirSenhaMembroAsync(id, cmd), Times.Once);
    }

    [Fact]
    public async Task ExcluirMembroAsync_DeveDelegarParaService()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.ExcluirMembroAsync(id)).Returns(Task.CompletedTask);

        await _facade.ExcluirMembroAsync(id);

        _serviceMock.Verify(s => s.ExcluirMembroAsync(id), Times.Once);
    }
}
