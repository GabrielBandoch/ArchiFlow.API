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

    [Fact]
    public async Task ObterPorEscritorioIdAsync_DeveRetornarMembrosDoMesmoEscritorioOuOwner()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new UsuarioRepository(context);

        var escritorioId = Guid.NewGuid();
        var outroEscritorioId = Guid.NewGuid();

        var admin = new Usuario
        {
            Id = escritorioId,
            EscritorioId = null,
            Nome = "Admin Owner",
            Email = "admin@studio.com",
            Role = "Administrador"
        };

        var colaborador = new Usuario
        {
            Id = Guid.NewGuid(),
            EscritorioId = escritorioId,
            Nome = "Colaborador",
            Email = "colaborador@studio.com",
            Role = "Colaborador"
        };

        var deOutroEscritorio = new Usuario
        {
            Id = Guid.NewGuid(),
            EscritorioId = outroEscritorioId,
            Nome = "Outro",
            Email = "outro@studio.com",
            Role = "Colaborador"
        };

        context.Usuarios.AddRange(admin, colaborador, deOutroEscritorio);
        await context.SaveChangesAsync();

        var result = await repository.ObterPorEscritorioIdAsync(escritorioId);

        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Email == "admin@studio.com");
        result.Should().Contain(u => u.Email == "colaborador@studio.com");
        result.Should().NotContain(u => u.Email == "outro@studio.com");
    }
}
