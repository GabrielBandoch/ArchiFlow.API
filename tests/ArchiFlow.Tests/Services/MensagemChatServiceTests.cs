using ArchiFlow.Application.Chat.Services;
using ArchiFlow.Domain.Chat;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Shared;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Services;

public class MensagemChatServiceTests
{
    private readonly Mock<IMensagemChatRepository> _mockRepo;
    private readonly Mock<IProjetoRepository> _mockProjetoRepo;
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly MensagemChatService _service;

    public MensagemChatServiceTests()
    {
        _mockRepo = new Mock<IMensagemChatRepository>();
        _mockProjetoRepo = new Mock<IProjetoRepository>();
        _mockUow = new Mock<IUnitOfWork>();

        _service = new MensagemChatService(
            _mockRepo.Object,
            _mockProjetoRepo.Object,
            _mockUow.Object
        );
    }

    [Fact]
    public async Task GetByProjetoId_Should_Return_Mapped_Dtos()
    {
        var projetoId = Guid.NewGuid();
        var mensagens = new List<MensagemChat>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ProjetoId = projetoId,
                RemetenteId = Guid.NewGuid(),
                RemetenteNome = "Carlos",
                RemetentePerfil = "Cliente",
                Conteudo = "Bom dia",
                CriadoEm = DateTime.UtcNow,
                Lida = false
            }
        };

        _mockRepo.Setup(r => r.GetByProjetoId(projetoId, It.IsAny<int>())).ReturnsAsync(mensagens);

        var result = await _service.GetByProjetoId(projetoId, 50);

        result.Should().HaveCount(1);
        result.Should().ContainSingle(m => m.Conteudo == "Bom dia" && m.RemetenteNome == "Carlos");
    }

    [Fact]
    public async Task EnviarMensagem_Should_Throw_When_Content_Is_Empty()
    {
        var act = () => _service.EnviarMensagem(Guid.NewGuid(), Guid.NewGuid(), "Nome", "Arquiteto", "");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task EnviarMensagem_Should_Throw_When_Project_Not_Found()
    {
        var projetoId = Guid.NewGuid();
        _mockProjetoRepo.Setup(r => r.GetById(projetoId)).ReturnsAsync((Projeto?)null);

        var act = () => _service.EnviarMensagem(projetoId, Guid.NewGuid(), "Nome", "Arquiteto", "Teste");
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task EnviarMensagem_Should_Create_And_Commit_When_Valid()
    {
        var projetoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var projeto = new Projeto { Id = projetoId, Nome = "Projeto Teste" };

        _mockProjetoRepo.Setup(r => r.GetById(projetoId)).ReturnsAsync(projeto);
        _mockRepo.Setup(r => r.Create(It.IsAny<MensagemChat>())).ReturnsAsync((MensagemChat m) => m);
        _mockUow.Setup(u => u.Commit(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(1);

        var result = await _service.EnviarMensagem(projetoId, usuarioId, "Marina", "Arquiteto", "Projeto atualizado!");

        result.Should().NotBeNull();
        result.Conteudo.Should().Be("Projeto atualizado!");
        result.RemetenteNome.Should().Be("Marina");
        result.RemetentePerfil.Should().Be("Arquiteto");

        _mockRepo.Verify(r => r.Create(It.IsAny<MensagemChat>()), Times.Once);
        _mockUow.Verify(u => u.Commit(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarcarComoLidas_Should_Call_Repository_And_Commit()
    {
        var projetoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        _mockRepo.Setup(r => r.MarcarComoLidas(projetoId, usuarioId)).Returns(Task.CompletedTask);
        _mockUow.Setup(u => u.Commit(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(1);

        await _service.MarcarComoLidas(projetoId, usuarioId);

        _mockRepo.Verify(r => r.MarcarComoLidas(projetoId, usuarioId), Times.Once);
        _mockUow.Verify(u => u.Commit(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
    }
}
