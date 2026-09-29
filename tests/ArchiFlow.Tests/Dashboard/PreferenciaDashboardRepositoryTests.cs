using ArchiFlow.Domain.Dashboard;
using ArchiFlow.Infrastructure.Repositories.Dashboard;
using ArchiFlow.Tests.Common;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Dashboard;

public class PreferenciaDashboardRepositoryTests
{
    [Fact]
    public async Task ObterPorUsuarioIdAsync_When_Not_Exists_Should_Return_Null()
    {
        var context = TestDbContextFactory.Create();
        var repo = new PreferenciaDashboardRepository(context);

        var result = await repo.ObterPorUsuarioIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task Create_Update_And_ObterPorUsuarioIdAsync_Should_Persist_Correctly()
    {
        var context = TestDbContextFactory.Create();
        var repo = new PreferenciaDashboardRepository(context);
        var usuarioId = Guid.NewGuid();

        var pref = new PreferenciaDashboard
        {
            UsuarioId = usuarioId,
            LayoutJson = "{\"test\":1}",
            AtualizadoEm = DateTime.UtcNow
        };

        await repo.Create(pref);
        await context.SaveChangesAsync();

        var fetched = await repo.ObterPorUsuarioIdAsync(usuarioId);
        fetched.Should().NotBeNull();
        fetched!.UsuarioId.Should().Be(usuarioId);
        fetched.LayoutJson.Should().Be("{\"test\":1}");

        fetched.LayoutJson = "{\"test\":2}";
        await repo.Update(fetched);
        await context.SaveChangesAsync();

        var updated = await repo.ObterPorUsuarioIdAsync(usuarioId);
        updated.Should().NotBeNull();
        updated!.LayoutJson.Should().Be("{\"test\":2}");
    }
}
