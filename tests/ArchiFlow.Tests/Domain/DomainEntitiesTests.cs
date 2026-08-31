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

    [Fact]
    public void Roles_DeveConterValoresCorretos()
    {
        Roles.Administrador.Should().Be("Administrador");
        Roles.Gerente.Should().Be("Gerente");
        Roles.Colaborador.Should().Be("Colaborador");
        Roles.Cliente.Should().Be("Cliente");
    }

    [Fact]
    public void ProjetosEntities_DevePermitirObterEDefinirPropriedades()
    {
        var id = Guid.NewGuid();
        var data = DateTime.UtcNow;

        var template = new Domain.Projetos.TemplateProjeto
        {
            Id = id,
            Codigo = "res",
            Nome = "Residencial",
            Descricao = "Desc",
            Icone = "home",
            Ativo = true,
            CriadoEm = data
        };
        template.Id.Should().Be(id);
        template.Codigo.Should().Be("res");
        template.Nome.Should().Be("Residencial");
        template.Descricao.Should().Be("Desc");
        template.Icone.Should().Be("home");
        template.Ativo.Should().BeTrue();
        template.CriadoEm.Should().Be(data);

        var etapaTemplate = new Domain.Projetos.TemplateEtapa
        {
            Id = id,
            TemplateProjetoId = template.Id,
            Nome = "Etapa 1",
            Descricao = "Desc",
            Ordem = 1,
            TarefasJson = "[]",
            TemplateProjeto = template
        };
        etapaTemplate.Id.Should().Be(id);
        etapaTemplate.TemplateProjetoId.Should().Be(template.Id);
        etapaTemplate.Nome.Should().Be("Etapa 1");
        etapaTemplate.Descricao.Should().Be("Desc");
        etapaTemplate.Ordem.Should().Be(1);
        etapaTemplate.TarefasJson.Should().Be("[]");
        etapaTemplate.TemplateProjeto.Should().Be(template);

        var tarefa = new Domain.Projetos.TarefaEtapa
        {
            Id = id,
            EtapaId = id,
            Titulo = "Desenho",
            Concluida = true,
            CriadoEm = data
        };
        tarefa.Id.Should().Be(id);
        tarefa.EtapaId.Should().Be(id);
        tarefa.Titulo.Should().Be("Desenho");
        tarefa.Concluida.Should().BeTrue();
        tarefa.CriadoEm.Should().Be(data);

        var etapaProj = new Domain.Projetos.EtapaProjeto
        {
            Id = id,
            ProjetoId = id,
            Nome = "Fase",
            Descricao = "Desc",
            Ordem = 1,
            Status = Domain.Projetos.Enum.StatusEtapa.Concluida,
            DataConclusao = data
        };
        etapaProj.Id.Should().Be(id);
        etapaProj.ProjetoId.Should().Be(id);
        etapaProj.Nome.Should().Be("Fase");
        etapaProj.Descricao.Should().Be("Desc");
        etapaProj.Ordem.Should().Be(1);
        etapaProj.Status.Should().Be(Domain.Projetos.Enum.StatusEtapa.Concluida);
        etapaProj.DataConclusao.Should().Be(data);

        var arq = new Domain.Projetos.Arquivo
        {
            Id = id,
            ProjetoId = id,
            Nome = "orig.pdf",
            UrlStorage = "https://storage/orig.pdf",
            Tipo = "PDF",
            VisivelCliente = true,
            CriadoEm = data
        };
        arq.Id.Should().Be(id);
        arq.ProjetoId.Should().Be(id);
        arq.Nome.Should().Be("orig.pdf");
        arq.UrlStorage.Should().Be("https://storage/orig.pdf");
        arq.Tipo.Should().Be("PDF");
        arq.VisivelCliente.Should().BeTrue();
        arq.CriadoEm.Should().Be(data);
    }
}
