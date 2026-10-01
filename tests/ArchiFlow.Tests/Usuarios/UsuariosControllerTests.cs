using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Usuarios.Commands;
using ArchiFlow.Application.Usuarios.DTOs;
using ArchiFlow.Domain.Usuarios;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Usuarios;

public class UsuariosControllerTests
{
    private readonly Mock<IUsuarioFacade> _facadeMock;
    private readonly UsuariosController _controller;

    public UsuariosControllerTests()
    {
        _facadeMock = new Mock<IUsuarioFacade>();
        _controller = new UsuariosController(_facadeMock.Object);
    }

    [Fact]
    public async Task ObterEquipe_DeveRetornarOkComLista()
    {
        // Arrange
        var lista = new List<MembroEquipeDto>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Admin", "admin@teste.com", Roles.ArquitetoAdmin, "Titular", null, true, DateTime.UtcNow, null)
        };
        _facadeMock.Setup(f => f.ObterEquipeAsync()).ReturnsAsync(lista);

        // Act
        var result = await _controller.ObterEquipe();

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(lista);
    }

    [Fact]
    public async Task ObterMembroPorId_QuandoEncontrado_DeveRetornarOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new MembroEquipeDto(id, Guid.NewGuid(), "Nome", "email@teste.com", Roles.Colaborador, null, null, true, DateTime.UtcNow, null);
        _facadeMock.Setup(f => f.ObterMembroPorIdAsync(id)).ReturnsAsync(dto);

        // Act
        var result = await _controller.ObterMembroPorId(id);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task ObterMembroPorId_QuandoNaoEncontrado_DeveRetornarNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _facadeMock.Setup(f => f.ObterMembroPorIdAsync(id)).ReturnsAsync((MembroEquipeDto?)null);

        // Act
        var result = await _controller.ObterMembroPorId(id);

        // Assert
        var notFound = result as NotFoundResult;
        notFound.Should().NotBeNull();
        notFound!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task ConvidarMembro_DeveRetornarCreatedAtAction()
    {
        // Arrange
        var command = new ConvidarMembroEquipeCommand("Nome", "email@teste.com", Roles.Colaborador, null, null, null);
        var novoId = Guid.NewGuid();
        var dto = new MembroEquipeDto(novoId, Guid.NewGuid(), "Nome", "email@teste.com", Roles.Colaborador, null, null, true, DateTime.UtcNow, null);
        _facadeMock.Setup(f => f.ConvidarMembroAsync(command)).ReturnsAsync(dto);

        // Act
        var result = await _controller.ConvidarMembro(command);

        // Assert
        var createdResult = result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult!.StatusCode.Should().Be(201);
        createdResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task AtualizarMembro_DeveRetornarOkComDtoAtualizado()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new AtualizarMembroEquipeCommand("Nome Atualizado", Roles.Gerente, null, null);
        var dto = new MembroEquipeDto(id, Guid.NewGuid(), "Nome Atualizado", "email@teste.com", Roles.Gerente, null, null, true, DateTime.UtcNow, null);
        _facadeMock.Setup(f => f.AtualizarMembroAsync(id, command)).ReturnsAsync(dto);

        // Act
        var result = await _controller.AtualizarMembro(id, command);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task AlterarStatusMembro_DeveRetornarOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new AlterarStatusMembroCommand(Ativo: false);
        var dto = new MembroEquipeDto(id, Guid.NewGuid(), "Nome", "email@teste.com", Roles.Gerente, null, null, false, DateTime.UtcNow, null);
        _facadeMock.Setup(f => f.AlterarStatusMembroAsync(id, command)).ReturnsAsync(dto);

        // Act
        var result = await _controller.AlterarStatusMembro(id, command);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task RedefinirSenhaMembro_DeveRetornarNoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new RedefinirSenhaMembroCommand(NovaSenha: "NovaSenha123!");

        // Act
        var result = await _controller.RedefinirSenhaMembro(id, command);

        // Assert
        var noContent = result as NoContentResult;
        noContent.Should().NotBeNull();
        noContent!.StatusCode.Should().Be(204);
        _facadeMock.Verify(f => f.RedefinirSenhaMembroAsync(id, command), Times.Once);
    }

    [Fact]
    public async Task ExcluirMembro_DeveRetornarNoContent()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = await _controller.ExcluirMembro(id);

        // Assert
        var noContent = result as NoContentResult;
        noContent.Should().NotBeNull();
        noContent!.StatusCode.Should().Be(204);
        _facadeMock.Verify(f => f.ExcluirMembroAsync(id), Times.Once);
    }
}
