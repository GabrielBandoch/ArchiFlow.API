using ArchiFlow.Application.Chat.DTOs;
using ArchiFlow.Application.Chat.Facades;
using ArchiFlow.Application.Interfaces.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Facades;

public class MensagemChatFacadeTests
{
    private readonly Mock<IMensagemChatService> _mockService;
    private readonly MensagemChatFacade _facade;

    public MensagemChatFacadeTests()
    {
        _mockService = new Mock<IMensagemChatService>();
        _facade = new MensagemChatFacade(_mockService.Object);
    }

    [Fact]
    public async Task GetByProjetoId_Should_Delegate_To_Service()
    {
        var projetoId = Guid.NewGuid();
        var lista = new List<MensagemChatDto>
        {
            new(Guid.NewGuid(), projetoId, Guid.NewGuid(), "User", "Arquiteto", "Teste", DateTime.UtcNow, false)
        };
        _mockService.Setup(s => s.GetByProjetoId(projetoId, 50)).ReturnsAsync(lista);

        var result = await _facade.GetByProjetoId(projetoId, 50);

        result.Should().BeEquivalentTo(lista);
        _mockService.Verify(s => s.GetByProjetoId(projetoId, 50), Times.Once);
    }

    [Fact]
    public async Task EnviarMensagem_Should_Delegate_To_Service()
    {
        var projetoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var msg = new MensagemChatDto(Guid.NewGuid(), projetoId, usuarioId, "User", "Cliente", "Teste", DateTime.UtcNow, false);

        _mockService.Setup(s => s.EnviarMensagem(projetoId, usuarioId, "User", "Cliente", "Teste"))
                    .ReturnsAsync(msg);

        var result = await _facade.EnviarMensagem(projetoId, usuarioId, "User", "Cliente", "Teste");

        result.Should().Be(msg);
        _mockService.Verify(s => s.EnviarMensagem(projetoId, usuarioId, "User", "Cliente", "Teste"), Times.Once);
    }

    [Fact]
    public async Task MarcarComoLidas_Should_Delegate_To_Service()
    {
        var projetoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        _mockService.Setup(s => s.MarcarComoLidas(projetoId, usuarioId)).Returns(Task.CompletedTask);

        await _facade.MarcarComoLidas(projetoId, usuarioId);

        _mockService.Verify(s => s.MarcarComoLidas(projetoId, usuarioId), Times.Once);
    }
}
