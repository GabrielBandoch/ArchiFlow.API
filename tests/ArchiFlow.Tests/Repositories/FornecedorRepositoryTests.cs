using ArchiFlow.Domain.Fornecedores;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Projetos.Enum;
using ArchiFlow.Infrastructure.Repositories.Fornecedores;
using ArchiFlow.Tests.Common;
using FluentAssertions;
using Xunit;

namespace ArchiFlow.Tests.Repositories;

public class FornecedorRepositoryTests
{
    [Fact]
    public async Task ObterTodosComRelacionamentosAsync_DeveRetornarFornecedoresOrdenadosPorAvaliacaoMedia()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new FornecedorRepository(context);

        var f1 = new Fornecedor { Id = Guid.NewGuid(), Nome = "Fornecedor B", Especialidade = "Marcenaria", AvaliacaoMedia = 3.0m };
        var f2 = new Fornecedor { Id = Guid.NewGuid(), Nome = "Fornecedor A", Especialidade = "Iluminacao", AvaliacaoMedia = 4.8m };

        context.Fornecedores.AddRange(f1, f2);
        await context.SaveChangesAsync();

        var result = (await repository.ObterTodosComRelacionamentosAsync()).ToList();

        result.Should().HaveCount(2);
        result[0].Nome.Should().Be("Fornecedor A");
        result[1].Nome.Should().Be("Fornecedor B");
    }

    [Fact]
    public async Task ObterPorIdComRelacionamentosAsync_QuandoExiste_DeveRetornarFornecedorComAvaliacoesEVinculos()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new FornecedorRepository(context);

        var fornecedorId = Guid.NewGuid();
        var projetoId = Guid.NewGuid();

        var projeto = new Projeto { Id = projetoId, Nome = "Projeto Teste", Status = StatusProjeto.Execucao };
        var fornecedor = new Fornecedor
        {
            Id = fornecedorId,
            Nome = "Fornecedor Detalhes",
            Especialidade = "Vidracaria",
            AvaliacaoMedia = 4.0m
        };
        var avaliacao = new AvaliacaoFornecedor
        {
            Id = Guid.NewGuid(),
            FornecedorId = fornecedorId,
            Nota = 4,
            Comentario = "Bom atendimento"
        };
        var vinculo = new ProjetoFornecedor
        {
            Id = Guid.NewGuid(),
            FornecedorId = fornecedorId,
            ProjetoId = projetoId,
            FuncaoNoProjeto = "Esquadrias"
        };

        context.Projetos.Add(projeto);
        context.Fornecedores.Add(fornecedor);
        context.AvaliacoesFornecedores.Add(avaliacao);
        context.ProjetosFornecedores.Add(vinculo);
        await context.SaveChangesAsync();

        var result = await repository.ObterPorIdComRelacionamentosAsync(fornecedorId);

        result.Should().NotBeNull();
        result!.Nome.Should().Be("Fornecedor Detalhes");
        result.Avaliacoes.Should().HaveCount(1);
        result.ProjetosVinculados.Should().HaveCount(1);
    }

    [Fact]
    public async Task ObterPorEspecialidadeAsync_DeveFiltrarPorEspecialidade()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new FornecedorRepository(context);

        var f1 = new Fornecedor { Id = Guid.NewGuid(), Nome = "Pintor 1", Especialidade = "Pintura" };
        var f2 = new Fornecedor { Id = Guid.NewGuid(), Nome = "Eletricista 1", Especialidade = "Eletrica" };

        context.Fornecedores.AddRange(f1, f2);
        await context.SaveChangesAsync();

        var result = (await repository.ObterPorEspecialidadeAsync("pintura")).ToList();

        result.Should().HaveCount(1);
        result[0].Nome.Should().Be("Pintor 1");
    }

    [Fact]
    public async Task ObterFornecedoresDoProjetoAsync_DeveRetornarVinculosDoProjeto()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new FornecedorRepository(context);

        var projetoId = Guid.NewGuid();
        var projeto = new Projeto { Id = projetoId, Nome = "Projeto Armarios" };
        var fornecedor = new Fornecedor { Id = Guid.NewGuid(), Nome = "Marcenaria Prime" };
        var vinculo = new ProjetoFornecedor
        {
            Id = Guid.NewGuid(),
            ProjetoId = projetoId,
            FornecedorId = fornecedor.Id,
            FuncaoNoProjeto = "Armarios"
        };

        context.Projetos.Add(projeto);
        context.Fornecedores.Add(fornecedor);
        context.ProjetosFornecedores.Add(vinculo);
        await context.SaveChangesAsync();

        var result = (await repository.ObterFornecedoresDoProjetoAsync(projetoId)).ToList();

        result.Should().HaveCount(1);
        result[0].Fornecedor?.Nome.Should().Be("Marcenaria Prime");
    }

    [Fact]
    public async Task AdicionarAvaliacaoAsync_DeveAdicionarAoDbSet()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new FornecedorRepository(context);

        var avaliacao = new AvaliacaoFornecedor
        {
            Id = Guid.NewGuid(),
            FornecedorId = Guid.NewGuid(),
            Nota = 5,
            Comentario = "Show"
        };

        await repository.AdicionarAvaliacaoAsync(avaliacao);
        await context.SaveChangesAsync();

        var noBanco = await context.AvaliacoesFornecedores.FindAsync(avaliacao.Id);
        noBanco.Should().NotBeNull();
    }

    [Fact]
    public async Task AdicionarVinculoProjetoAsync_DeveAdicionarAoDbSet()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new FornecedorRepository(context);

        var vinculo = new ProjetoFornecedor
        {
            Id = Guid.NewGuid(),
            ProjetoId = Guid.NewGuid(),
            FornecedorId = Guid.NewGuid(),
            FuncaoNoProjeto = "Gesso"
        };

        await repository.AdicionarVinculoProjetoAsync(vinculo);
        await context.SaveChangesAsync();

        var noBanco = await context.ProjetosFornecedores.FindAsync(vinculo.Id);
        noBanco.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoverVinculoProjetoAsync_QuandoExiste_DeveRemoverERetornarTrue()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new FornecedorRepository(context);

        var vinculo = new ProjetoFornecedor
        {
            Id = Guid.NewGuid(),
            ProjetoId = Guid.NewGuid(),
            FornecedorId = Guid.NewGuid(),
            FuncaoNoProjeto = "Piso"
        };
        context.ProjetosFornecedores.Add(vinculo);
        await context.SaveChangesAsync();

        var removido = await repository.RemoverVinculoProjetoAsync(vinculo.Id);
        await context.SaveChangesAsync();

        removido.Should().BeTrue();
        var noBanco = await context.ProjetosFornecedores.FindAsync(vinculo.Id);
        noBanco.Should().BeNull();
    }

    [Fact]
    public async Task RemoverVinculoProjetoAsync_QuandoNaoExiste_DeveRetornarFalse()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new FornecedorRepository(context);

        var removido = await repository.RemoverVinculoProjetoAsync(Guid.NewGuid());

        removido.Should().BeFalse();
    }
}
