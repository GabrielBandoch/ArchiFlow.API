using ArchiFlow.Domain.Chat;
using ArchiFlow.Infrastructure.Data;
using ArchiFlow.Infrastructure.Repositories.Chat;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Repositories;

public class MensagemChatRepositoryTests
{
    private static ArchiFlowDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ArchiFlowDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ArchiFlowDbContext(options);
    }

    [Fact]
    public async Task Create_And_GetByProjetoId_Should_Persist_And_Return_Ordered_Messages()
    {
        using var context = GetInMemoryDbContext();
        var repo = new MensagemChatRepository(context);

        var projetoId = Guid.NewGuid();
        var msg1 = new MensagemChat
        {
            Id = Guid.NewGuid(),
            ProjetoId = projetoId,
            RemetenteId = Guid.NewGuid(),
            RemetenteNome = "Carlos",
            RemetentePerfil = "Cliente",
            Conteudo = "Mensagem 1",
            CriadoEm = DateTime.UtcNow.AddMinutes(-5),
            Lida = false
        };

        var msg2 = new MensagemChat
        {
            Id = Guid.NewGuid(),
            ProjetoId = projetoId,
            RemetenteId = Guid.NewGuid(),
            RemetenteNome = "Marina",
            RemetentePerfil = "Arquiteto",
            Conteudo = "Mensagem 2",
            CriadoEm = DateTime.UtcNow,
            Lida = false
        };

        await repo.Create(msg1);
        await repo.Create(msg2);
        await context.SaveChangesAsync();

        var mensagens = await repo.GetByProjetoId(projetoId, 10);

        mensagens.Should().HaveCount(2);
    }

    [Fact]
    public async Task MarcarComoLidas_Should_Update_Unread_Messages_From_Other_Senders()
    {
        using var context = GetInMemoryDbContext();
        var repo = new MensagemChatRepository(context);

        var projetoId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var arquitetoId = Guid.NewGuid();

        var msgCliente = new MensagemChat
        {
            Id = Guid.NewGuid(),
            ProjetoId = projetoId,
            RemetenteId = clienteId,
            RemetenteNome = "Carlos",
            RemetentePerfil = "Cliente",
            Conteudo = "Dúvida sobre a planta",
            CriadoEm = DateTime.UtcNow,
            Lida = false
        };

        await repo.Create(msgCliente);
        await context.SaveChangesAsync();

        // Arquiteto visualiza o chat
        await repo.MarcarComoLidas(projetoId, arquitetoId);
        await context.SaveChangesAsync();

        var atualizada = await context.MensagensChat.FindAsync(msgCliente.Id);
        atualizada!.Lida.Should().BeTrue();
    }
}
