using ArchiFlow.Domain.Usuarios;
using ArchiFlow.Infrastructure.Repositories.Usuarios;
using ArchiFlow.Tests.Common;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Repositories;

public class UsuarioRepositoryTests
{
    [Fact]
    public async Task GetByEmail_DeveRetornarUsuario_SeExistir()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new UsuarioRepository(context);

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Arquiteto",
            Email = "teste@archiflow.com",
            SenhaHash = "hash",
            Role = "Administrador"
        };

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var result = await repository.GetByEmail("teste@archiflow.com");

        result.Should().NotBeNull();
        result!.Nome.Should().Be("Arquiteto");
    }

    [Fact]
    public async Task GetByEmail_DeveRetornarUsuario_SeExistirComCaseDiferente()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new UsuarioRepository(context);

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Arquiteto",
            Email = "Teste@ArchiFlow.com",
            SenhaHash = "hash",
            Role = "Administrador"
        };

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var result = await repository.GetByEmail("tEsTe@ArChIfLoW.cOm");

        result.Should().NotBeNull();
        result!.Nome.Should().Be("Arquiteto");
    }

    [Fact]
    public async Task GetByEmail_DeveRetornarNull_SeNaoExistir()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new UsuarioRepository(context);

        var result = await repository.GetByEmail("inexistente@test.com");

        result.Should().BeNull();
    }
}
