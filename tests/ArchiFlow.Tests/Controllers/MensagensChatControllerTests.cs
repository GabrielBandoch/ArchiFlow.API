using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Chat.Commands;
using ArchiFlow.Application.Chat.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Controllers;

public class MensagensChatControllerTests
{
    private readonly Mock<IMensagemChatFacade> _mockFacade;
    private readonly MensagensChatController _controller;

    public MensagensChatControllerTests()
    {
        _mockFacade = new Mock<IMensagemChatFacade>();
        _controller = new MensagensChatController(_mockFacade.Object);
    }

    [Fact]
    public async Task GetByProjeto_Should_Return_Ok_With_Messages_From_Facade()
    {
        // Arrange
        var projetoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var mensagens = new List<MensagemChatDto>
        {
            new(Guid.NewGuid(), projetoId, usuarioId, "Arquiteto", "Arquiteto", "Olá cliente!", DateTime.UtcNow, false)
        };

        _mockFacade.Setup(f => f.GetByProjetoId(projetoId, 50)).ReturnsAsync(mensagens);

        // Act
        var result = await _controller.GetByProjeto(projetoId, 50);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(mensagens);
        _mockFacade.Verify(f => f.GetByProjetoId(projetoId, 50), Times.Once);
    }

    [Fact]
    public async Task EnviarMensagem_Should_Return_CreatedAtAction_With_New_Message()
    {
        // Arrange
        var projetoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var command = new EnviarMensagemCommand(projetoId, "Mensagem de teste");
        var msgRetorno = new MensagemChatDto(
            Guid.NewGuid(),
            projetoId,
            usuarioId,
            "Marina Sievert",
            "Arquiteto",
            "Mensagem de teste",
            DateTime.UtcNow,
            false
        );

        _mockFacade.Setup(f => f.EnviarMensagem(projetoId, command))
                   .ReturnsAsync(msgRetorno);

        // Act
        var result = await _controller.EnviarMensagem(projetoId, command);

        // Assert
        var createdResult = result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult!.Value.Should().BeEquivalentTo(msgRetorno);
        _mockFacade.Verify(f => f.EnviarMensagem(projetoId, command), Times.Once);
    }
}
