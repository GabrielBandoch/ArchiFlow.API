using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Projetos.Enum;
using ArchiFlow.Infrastructure.Repositories.Projetos;
using ArchiFlow.Tests.Common;
using FluentAssertions;
using Xunit;

namespace ArchiFlow.Tests.Repositories;

public class ProjetoRepositoryTests
{
    private static Projeto CriarProjeto(string nome) =>
        new Projeto
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Descricao = "Desc",
            Tipo = TipoProjeto.Residencial,
            DataInicio = DateTime.UtcNow,
            MetragemTotal = 100,
            ClienteId = Guid.NewGuid(),
            CriadoEm = DateTime.UtcNow
        };

    [Fact]
    public async Task GetById_QuandoProjetoExiste_DeveRetornarComEtapas()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ProjetoRepository(context);

        var projeto = CriarProjeto("Casa de Praia");
        var etapa = new EtapaProjeto
        {
            Id = Guid.NewGuid(),
            ProjetoId = projeto.Id,
            Nome = "Fundação",
            Descricao = "Desc",
            Ordem = 1,
            Status = StatusEtapa.Pendente
        };
        projeto.Etapas.Add(etapa);

        context.Projetos.Add(projeto);
        await context.SaveChangesAsync();

        var result = await repository.GetById(projeto.Id);

        result.Should().NotBeNull();
        result!.Nome.Should().Be("Casa de Praia");
    }

    [Fact]
    public async Task GetByIdWithEtapas_DeveRetornarProjetoComEtapas()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ProjetoRepository(context);

        var projeto = CriarProjeto("Casa de Campo");
        var etapa = new EtapaProjeto
        {
            Id = Guid.NewGuid(),
            ProjetoId = projeto.Id,
            Nome = "Fundação",
            Descricao = "Desc",
            Ordem = 1,
            Status = StatusEtapa.Pendente
        };
        projeto.Etapas.Add(etapa);

        context.Projetos.Add(projeto);
        await context.SaveChangesAsync();

        var result = await repository.GetByIdWithEtapas(projeto.Id);

        result.Should().NotBeNull();
        result!.Etapas.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllWithEtapas_DeveRetornarTodos()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ProjetoRepository(context);

        context.Projetos.Add(CriarProjeto("P1"));
        await context.SaveChangesAsync();

        var result = await repository.GetAllWithEtapas();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByClienteId_DeveRetornarFiltradoPorCliente()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ProjetoRepository(context);

        var clienteId = Guid.NewGuid();
        var p = CriarProjeto("PCliente");
        p.ClienteId = clienteId;

        context.Projetos.Add(p);
        await context.SaveChangesAsync();

        var result = await repository.GetByClienteId(clienteId);
        result.Should().ContainSingle();
        result.First().ClienteId.Should().Be(clienteId);
    }

    [Fact]
    public async Task GetById_QuandoInexistente_DeveRetornarNull()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ProjetoRepository(context);

        var result = await repository.GetById(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByStatus_DeveRetornarFiltrado()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ProjetoRepository(context);

        var p1 = CriarProjeto("P1");
        p1.Status = StatusProjeto.Briefing;
        var p2 = CriarProjeto("P2");
        p2.Status = StatusProjeto.Desenvolvimento;

        context.Projetos.AddRange(p1, p2);
        await context.SaveChangesAsync();

        var result = await repository.GetByStatus(StatusProjeto.Briefing);

        result.Should().ContainSingle();
        result.First().Nome.Should().Be("P1");
    }

    [Fact]
    public async Task GetAll_DeveRetornarTodosSemEtapas()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ProjetoRepository(context);

        context.Projetos.Add(CriarProjeto("P1"));
        await context.SaveChangesAsync();

        var result = await repository.GetAll();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Create_DeveAdicionarAoBanco()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ProjetoRepository(context);

        var p = CriarProjeto("Criado");
        var result = await repository.Create(p);

        result.Should().NotBeNull();
        context.Projetos.Find(p.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task Update_DeveModificarNoBanco()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ProjetoRepository(context);

        var p = CriarProjeto("Original");
        context.Projetos.Add(p);
        await context.SaveChangesAsync();

        p.Nome = "Modificado";
        await repository.Update(p);
        await context.SaveChangesAsync();

        context.Projetos.Find(p.Id)!.Nome.Should().Be("Modificado");
    }

    [Fact]
    public async Task CreateOrUpdate_QuandoNaoExiste_DeveCriar()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ProjetoRepository(context);

        var p = CriarProjeto("Novo");
        await repository.CreateOrUpdate(p, p.Id);
        await context.SaveChangesAsync();

        context.Projetos.Find(p.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task CreateOrUpdate_QuandoExiste_DeveAtualizar()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ProjetoRepository(context);

        var p = CriarProjeto("Original");
        context.Projetos.Add(p);
        await context.SaveChangesAsync();

        p.Nome = "Atualizado";
        await repository.CreateOrUpdate(p, p.Id);
        await context.SaveChangesAsync();

        context.Projetos.Find(p.Id)!.Nome.Should().Be("Atualizado");
    }

    [Fact]
    public async Task Delete_QuandoExiste_DeveRemover()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ProjetoRepository(context);

        var p = CriarProjeto("Original");
        context.Projetos.Add(p);
        await context.SaveChangesAsync();

        await repository.Delete(p.Id);
        await context.SaveChangesAsync();

        context.Projetos.Find(p.Id).Should().BeNull();
    }

    [Fact]
    public async Task Delete_QuandoNaoExiste_DeveLancarKeyNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ProjetoRepository(context);

        var act = async () => await repository.Delete(Guid.NewGuid());
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Exists_DeveRetornarTrueSeExiste()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ProjetoRepository(context);

        var p = CriarProjeto("Original");
        context.Projetos.Add(p);
        await context.SaveChangesAsync();

        var result = await repository.Exists(p.Id);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Etapas_OperacoesRepository_DeveFuncionar()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ProjetoRepository(context);

        var p = CriarProjeto("Projeto");
        var etapa = new EtapaProjeto
        {
            Id = Guid.NewGuid(),
            ProjetoId = p.Id,
            Nome = "Etapa1",
            Descricao = "Desc",
            Ordem = 1,
            Status = StatusEtapa.Pendente
        };

        await repository.CreateEtapa(etapa);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetEtapaById(etapa.Id);
        retrieved.Should().NotBeNull();

        retrieved!.Nome = "EtapaModificada";
        await repository.UpdateEtapa(retrieved);
        await context.SaveChangesAsync();

        var retrieved2 = await repository.GetEtapaById(etapa.Id);
        retrieved2!.Nome.Should().Be("EtapaModificada");
    }
}
