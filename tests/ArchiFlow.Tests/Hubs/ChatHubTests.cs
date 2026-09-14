using ArchiFlow.API.Hubs;
using ArchiFlow.Application.Chat.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Hubs;

public class ChatHubTests
{
    private readonly Mock<IMensagemChatFacade> _mockFacade;
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly ChatHub _hub;

    public ChatHubTests()
    {
        _mockFacade = new Mock<IMensagemChatFacade>();
        _mockClients = new Mock<IHubCallerClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockGroups = new Mock<IGroupManager>();
        _mockContext = new Mock<HubCallerContext>();

        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockContext.Setup(c => c.ConnectionId).Returns("conn-123");

        _hub = new ChatHub(_mockFacade.Object)
        {
            Clients = _mockClients.Object,
            Groups = _mockGroups.Object,
            Context = _mockContext.Object
        };
    }

    [Fact]
    public async Task EntrarNoProjeto_Should_AddToGroup_And_MarkAsRead_When_User_Authenticated()
    {
        var projetoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new(ClaimTypes.Name, "Marina"),
            new(ClaimTypes.Role, "Arquiteto")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        _mockContext.Setup(c => c.User).Returns(principal);

        _mockGroups.Setup(g => g.AddToGroupAsync("conn-123", $"projeto_{projetoId}", default))
            .Returns(Task.CompletedTask);
        _mockFacade.Setup(f => f.MarcarComoLidas(projetoId, usuarioId))
            .Returns(Task.CompletedTask);

        await _hub.EntrarNoProjeto(projetoId.ToString());

        _mockGroups.Verify(g => g.AddToGroupAsync("conn-123", $"projeto_{projetoId}", default), Times.Once);
        _mockFacade.Verify(f => f.MarcarComoLidas(projetoId, usuarioId), Times.Once);
    }

    [Fact]
    public async Task EntrarNoProjeto_Should_Ignore_When_Invalid_Guid()
    {
        await _hub.EntrarNoProjeto("invalid-guid");

        _mockGroups.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
        _mockFacade.Verify(f => f.MarcarComoLidas(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task SairDoProjeto_Should_RemoveFromGroup_When_Valid()
    {
        var projetoId = Guid.NewGuid();

        _mockGroups.Setup(g => g.RemoveFromGroupAsync("conn-123", $"projeto_{projetoId}", default))
            .Returns(Task.CompletedTask);

        await _hub.SairDoProjeto(projetoId.ToString());

        _mockGroups.Verify(g => g.RemoveFromGroupAsync("conn-123", $"projeto_{projetoId}", default), Times.Once);
    }

    [Fact]
    public async Task EnviarMensagem_Should_Call_Facade_And_SendAsync()
    {
        var projetoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var dto = new MensagemChatDto(
            Guid.NewGuid(),
            projetoId,
            usuarioId,
            "Carlos",
            "Cliente",
            "Olá!",
            DateTime.UtcNow,
            false
        );

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new(ClaimTypes.Name, "Carlos"),
            new("user_type", "client"),
            new("projeto_id", projetoId.ToString())
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        _mockContext.Setup(c => c.User).Returns(principal);

        _mockFacade.Setup(f => f.EnviarMensagem(projetoId, usuarioId, "Carlos", "Cliente", "Olá!"))
            .ReturnsAsync(dto);

        await _hub.EnviarMensagem(projetoId.ToString(), "Olá!");

        _mockFacade.Verify(f => f.EnviarMensagem(projetoId, usuarioId, "Carlos", "Cliente", "Olá!"), Times.Once);
        _mockClients.Verify(c => c.Group($"projeto_{projetoId}"), Times.Once);
    }

    [Fact]
    public async Task EntrarNoProjeto_Should_AddToGroup_WithoutMarkAsRead_When_User_Has_No_Id()
    {
        var projetoId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, "Arquiteto"),
            new("user_type", "staff")
        };
        _mockContext.Setup(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));
        _mockGroups.Setup(g => g.AddToGroupAsync("conn-123", $"projeto_{projetoId}", default))
            .Returns(Task.CompletedTask);

        await _hub.EntrarNoProjeto(projetoId.ToString());

        _mockGroups.Verify(g => g.AddToGroupAsync("conn-123", $"projeto_{projetoId}", default), Times.Once);
        _mockFacade.Verify(f => f.MarcarComoLidas(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task SairDoProjeto_Should_Ignore_When_Invalid_Guid()
    {
        await _hub.SairDoProjeto("invalid-guid");
        _mockGroups.Verify(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task EnviarMensagem_Should_Fallback_To_Defaults_When_Staff_Has_No_Specific_Name_Claims()
    {
        var projetoId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new("user_type", "staff")
        };
        _mockContext.Setup(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));

        var dto = new MensagemChatDto(
            Guid.NewGuid(),
            projetoId,
            Guid.NewGuid(),
            "Usuário",
            "Arquiteto",
            "Mensagem Anônima",
            DateTime.UtcNow,
            false
        );

        _mockFacade.Setup(f => f.EnviarMensagem(projetoId, It.IsAny<Guid>(), "Usuário", "Arquiteto", "Mensagem Anônima"))
            .ReturnsAsync(dto);

        await _hub.EnviarMensagem(projetoId.ToString(), "Mensagem Anônima");

        _mockFacade.Verify(f => f.EnviarMensagem(projetoId, It.IsAny<Guid>(), "Usuário", "Arquiteto", "Mensagem Anônima"), Times.Once);
        _mockClients.Verify(c => c.Group($"projeto_{projetoId}"), Times.Once);
    }

    [Fact]
    public async Task EnviarMensagem_Should_Use_Email_And_Role_When_Available()
    {
        var projetoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new("sub", usuarioId.ToString()),
            new(ClaimTypes.Email, "teste@archiflow.com"),
            new(ClaimTypes.Role, "Cliente"),
            new("projeto_id", projetoId.ToString())
        };
        _mockContext.Setup(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));

        var dto = new MensagemChatDto(
            Guid.NewGuid(),
            projetoId,
            usuarioId,
            "teste@archiflow.com",
            "Cliente",
            "Mensagem com Email",
            DateTime.UtcNow,
            false
        );

        _mockFacade.Setup(f => f.EnviarMensagem(projetoId, usuarioId, "teste@archiflow.com", "Cliente", "Mensagem com Email"))
            .ReturnsAsync(dto);

        await _hub.EnviarMensagem(projetoId.ToString(), "Mensagem com Email");

        _mockFacade.Verify(f => f.EnviarMensagem(projetoId, usuarioId, "teste@archiflow.com", "Cliente", "Mensagem com Email"), Times.Once);
    }

    [Fact]
    public async Task EnviarMensagem_Should_Ignore_When_Content_Empty()
    {
        await _hub.EnviarMensagem(Guid.NewGuid().ToString(), "   ");

        _mockFacade.Verify(f => f.EnviarMensagem(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task EntrarNoProjeto_Should_Block_When_Client_Attempts_Access_To_Another_Project()
    {
        var projetoAlheioId = Guid.NewGuid();
        var meuProjetoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new(ClaimTypes.Name, "Cliente Invasor"),
            new(ClaimTypes.Role, "Cliente"),
            new("user_type", "client"),
            new("projeto_id", meuProjetoId.ToString())
        };
        _mockContext.Setup(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));

        await _hub.EntrarNoProjeto(projetoAlheioId.ToString());

        _mockGroups.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
        _mockFacade.Verify(f => f.MarcarComoLidas(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task EnviarMensagem_Should_Block_When_Client_Attempts_Sending_To_Another_Project()
    {
        var projetoAlheioId = Guid.NewGuid();
        var meuProjetoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new(ClaimTypes.Name, "Cliente Invasor"),
            new(ClaimTypes.Role, "Cliente"),
            new("user_type", "client"),
            new("projeto_id", meuProjetoId.ToString())
        };
        _mockContext.Setup(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));

        await _hub.EnviarMensagem(projetoAlheioId.ToString(), "Mensagem Indevida");

        _mockFacade.Verify(f => f.EnviarMensagem(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockClients.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task EntrarNoProjeto_Should_Allow_When_Client_Accesses_Own_Project()
    {
        var meuProjetoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new(ClaimTypes.Name, "Cliente Legítimo"),
            new(ClaimTypes.Role, "Cliente"),
            new("user_type", "client"),
            new("projeto_id", meuProjetoId.ToString())
        };
        _mockContext.Setup(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));
        _mockGroups.Setup(g => g.AddToGroupAsync("conn-123", $"projeto_{meuProjetoId}", default)).Returns(Task.CompletedTask);
        _mockFacade.Setup(f => f.MarcarComoLidas(meuProjetoId, usuarioId)).Returns(Task.CompletedTask);

        await _hub.EntrarNoProjeto(meuProjetoId.ToString());

        _mockGroups.Verify(g => g.AddToGroupAsync("conn-123", $"projeto_{meuProjetoId}", default), Times.Once);
        _mockFacade.Verify(f => f.MarcarComoLidas(meuProjetoId, usuarioId), Times.Once);
    }

    [Fact]
    public async Task EnviarMensagem_Should_Allow_When_Staff_Accesses_Any_Project()
    {
        var qualquerProjetoId = Guid.NewGuid();
        var arquitetoId = Guid.NewGuid();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, arquitetoId.ToString()),
            new(ClaimTypes.Name, "Marina Arquiteta"),
            new(ClaimTypes.Role, "Arquiteto"),
            new("user_type", "staff")
        };
        _mockContext.Setup(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));

        var dto = new MensagemChatDto(
            Guid.NewGuid(),
            qualquerProjetoId,
            arquitetoId,
            "Marina Arquiteta",
            "Arquiteto",
            "Mensagem da Equipe",
            DateTime.UtcNow,
            false
        );

        _mockFacade.Setup(f => f.EnviarMensagem(qualquerProjetoId, arquitetoId, "Marina Arquiteta", "Arquiteto", "Mensagem da Equipe"))
            .ReturnsAsync(dto);

        await _hub.EnviarMensagem(qualquerProjetoId.ToString(), "Mensagem da Equipe");

        _mockFacade.Verify(f => f.EnviarMensagem(qualquerProjetoId, arquitetoId, "Marina Arquiteta", "Arquiteto", "Mensagem da Equipe"), Times.Once);
        _mockClients.Verify(c => c.Group($"projeto_{qualquerProjetoId}"), Times.Once);
    }
}
