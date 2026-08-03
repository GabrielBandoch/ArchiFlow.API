using ArchiFlow.Domain.Clientes;
using ArchiFlow.Infrastructure.Repositories.Clientes;
using ArchiFlow.Tests.Common;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Repositories;

public class ClienteRepositoryTests
{
    [Fact]
    public async Task GetByEmail_DeveRetornarCliente_SeExistir()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ClienteRepository(context);

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = "Cliente",
            Email = "cliente@archiflow.com",
            SenhaPortal = "hash"
        };

        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var result = await repository.GetByEmail("cliente@archiflow.com");

        result.Should().NotBeNull();
        result!.Nome.Should().Be("Cliente");
    }

    [Fact]
    public async Task GetByEmail_DeveRetornarCliente_SeExistirComCaseDiferente()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ClienteRepository(context);

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = "Cliente",
            Email = "Cliente@ArchiFlow.com",
            SenhaPortal = "hash"
        };

        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var result = await repository.GetByEmail("cLienTe@ArChIfLoW.cOm");

        result.Should().NotBeNull();
        result!.Nome.Should().Be("Cliente");
    }

    [Fact]
    public async Task GetByEmail_DeveRetornarNull_SeNaoExistir()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ClienteRepository(context);

        var result = await repository.GetByEmail("inexistente@test.com");

        result.Should().BeNull();
    }
}
