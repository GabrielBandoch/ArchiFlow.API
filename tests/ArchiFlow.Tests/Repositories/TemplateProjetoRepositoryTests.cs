using ArchiFlow.Domain.Projetos;
using ArchiFlow.Infrastructure.Repositories.Projetos;
using ArchiFlow.Tests.Common;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Repositories;

public class TemplateProjetoRepositoryTests
{
    private static TemplateProjeto CriarTemplate(string codigo, string nome, bool ativo = true) =>
        new()
        {
            Id = Guid.NewGuid(),
            Codigo = codigo,
            Nome = nome,
            Descricao = "Descricao do template",
            Icone = "home",
            Ativo = ativo,
            CriadoEm = DateTime.UtcNow,
            Etapas = new List<TemplateEtapa>()
        };

    [Fact]
    public async Task GetAllWithEtapas_DeveRetornarApenasTemplatesAtivosOrdenados()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new TemplateProjetoRepository(context);

        var t1 = CriarTemplate("b-comercial", "B Comercial", ativo: true);
        var t2 = CriarTemplate("a-residencial", "A Residencial", ativo: true);
        var tInativo = CriarTemplate("c-inativo", "C Inativo", ativo: false);

        await repo.Create(t1);
        await repo.Create(t2);
        await repo.Create(tInativo);
        await context.SaveChangesAsync();

        var result = (await repo.GetAllWithEtapas()).ToList();

        result.Should().HaveCount(2);
        result.First().Nome.Should().Be("A Residencial");
        result.Last().Nome.Should().Be("B Comercial");
        result.Should().NotContain(t => t.Codigo == "c-inativo");
    }

    [Fact]
    public async Task GetByIdWithEtapas_DeveRetornarTemplateComEtapas()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new TemplateProjetoRepository(context);

        var template = CriarTemplate("residencial", "Residencial Completo");
        var etapa = new TemplateEtapa
        {
            Id = Guid.NewGuid(),
            TemplateProjetoId = template.Id,
            Nome = "Briefing",
            Descricao = "Estudo inicial",
            Ordem = 1,
            TarefasJson = "[\"Reunião inicial\"]"
        };
        template.Etapas.Add(etapa);

        await repo.Create(template);
        await context.SaveChangesAsync();

        var result = await repo.GetByIdWithEtapas(template.Id);

        result.Should().NotBeNull();
        result!.Nome.Should().Be("Residencial Completo");
        result.Etapas.Should().HaveCount(1);
        result.Etapas.First().Nome.Should().Be("Briefing");
    }

    [Fact]
    public async Task GetByCodigoWithEtapas_DeveRetornarTemplatePorCodigo()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new TemplateProjetoRepository(context);

        var template = CriarTemplate("interiores-luxo", "Interiores Luxo");
        await repo.Create(template);
        await context.SaveChangesAsync();

        var result = await repo.GetByCodigoWithEtapas("interiores-luxo");

        result.Should().NotBeNull();
        result!.Nome.Should().Be("Interiores Luxo");
    }

    [Fact]
    public async Task AddEtapas_ERemoveEtapas_DevePersistirAlteracoes()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new TemplateProjetoRepository(context);

        var template = CriarTemplate("temp-etapas", "Template Etapas");
        await repo.Create(template);
        await context.SaveChangesAsync();

        var etapa1 = new TemplateEtapa { Id = Guid.NewGuid(), TemplateProjetoId = template.Id, Nome = "Etapa 1", Ordem = 1 };
        repo.AddEtapas(new[] { etapa1 });
        await context.SaveChangesAsync();

        var check = await repo.GetByIdWithEtapas(template.Id);
        check!.Etapas.Should().HaveCount(1);

        repo.RemoveEtapas(new[] { etapa1 });
        await context.SaveChangesAsync();

        var checkAfterRemove = await repo.GetByIdWithEtapas(template.Id);
        checkAfterRemove!.Etapas.Should().BeEmpty();
    }
}
