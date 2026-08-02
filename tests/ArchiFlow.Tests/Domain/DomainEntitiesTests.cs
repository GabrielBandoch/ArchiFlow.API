using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Usuarios;
using FluentAssertions;
using System;
using Xunit;

namespace ArchiFlow.Tests.DomainTests;

public class DomainEntitiesTests
{
    [Fact]
    public void Usuario_DevePermitirObterEDefinirPropriedades()
    {
        var id = Guid.NewGuid();
        var data = DateTime.UtcNow;
        var usuario = new Usuario
        {
            Id = id,
            Nome = "Nome",
            Email = "email@test.com",
            SenhaHash = "hash",
            Role = "Admin",
            Ativo = false,
            CriadoEm = data,
            AtualizadoEm = data
        };

        usuario.Id.Should().Be(id);
        usuario.Nome.Should().Be("Nome");
        usuario.Email.Should().Be("email@test.com");
        usuario.SenhaHash.Should().Be("hash");
        usuario.Role.Should().Be("Admin");
        usuario.Ativo.Should().BeFalse();
        usuario.CriadoEm.Should().Be(data);
        usuario.AtualizadoEm.Should().Be(data);
    }

    [Fact]
    public void Cliente_DevePermitirObterEDefinirPropriedades()
    {
        var id = Guid.NewGuid();
        var leadId = Guid.NewGuid();
        var cliente = new Cliente
        {
            Id = id,
            LeadId = leadId,
            Nome = "Cliente",
            Email = "cliente@test.com",
            Telefone = "123",
            CpfCnpj = "456",
            SenhaPortal = "senha",
            Ativo = false,
            Endereco = "Rua X"
        };

        cliente.Id.Should().Be(id);
        cliente.LeadId.Should().Be(leadId);
        cliente.Nome.Should().Be("Cliente");
        cliente.Email.Should().Be("cliente@test.com");
        cliente.Telefone.Should().Be("123");
        cliente.CpfCnpj.Should().Be("456");
        cliente.SenhaPortal.Should().Be("senha");
        cliente.Ativo.Should().BeFalse();
        cliente.Endereco.Should().Be("Rua X");
    }
}
