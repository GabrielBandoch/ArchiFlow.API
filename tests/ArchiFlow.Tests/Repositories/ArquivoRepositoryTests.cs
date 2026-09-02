using ArchiFlow.Domain.Projetos;
using ArchiFlow.Infrastructure.Repositories.Projetos;
using ArchiFlow.Tests.Common;
using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Repositories;

public class ArquivoRepositoryTests
{
    [Fact]
    public async Task GetByProjetoId_DeveRetornarApenasArquivosDoProjetoOrdenados()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new ArquivoRepository(context);

        var projetoId1 = Guid.NewGuid();
        var projetoId2 = Guid.NewGuid();

        var arq1 = new Arquivo
        {
            Id = Guid.NewGuid(),
            ProjetoId = projetoId1,
            Nome = "planta1.pdf",
            UrlStorage = "https://s3/planta1.pdf",
            Tipo = "Planta",
            VisivelCliente = true,
            CriadoEm = DateTime.UtcNow.AddMinutes(-10)
        };

        var arq2 = new Arquivo
        {
            Id = Guid.NewGuid(),
            ProjetoId = projetoId1,
            Nome = "planta2.pdf",
            UrlStorage = "https://s3/planta2.pdf",
            Tipo = "Planta",
            VisivelCliente = true,
            CriadoEm = DateTime.UtcNow
        };

        var arqOutro = new Arquivo
        {
            Id = Guid.NewGuid(),
            ProjetoId = projetoId2,
            Nome = "outro.pdf",
            UrlStorage = "https://s3/outro.pdf",
            Tipo = "Documento",
            VisivelCliente = false,
            CriadoEm = DateTime.UtcNow
        };

        await repo.Create(arq1);
        await repo.Create(arq2);
        await repo.Create(arqOutro);
        await context.SaveChangesAsync();

        var result = (await repo.GetByProjetoId(projetoId1)).ToList();

        result.Should().HaveCount(2);
        result.First().Nome.Should().Be("planta2.pdf"); // mais recente primeiro
        result.Last().Nome.Should().Be("planta1.pdf");
        result.Should().NotContain(a => a.ProjetoId == projetoId2);
    }

    [Fact]
    public async Task GetByProjetoId_ComApenasVisiveisCliente_DeveFiltrarArquivosOcultos()
    {
        using var context = TestDbContextFactory.Create();
        var repo = new ArquivoRepository(context);

        var projetoId = Guid.NewGuid();

        var arqVisivel = new Arquivo
        {
            Id = Guid.NewGuid(),
            ProjetoId = projetoId,
            Nome = "visivel.pdf",
            UrlStorage = "https://s3/visivel.pdf",
            Tipo = "Planta",
            VisivelCliente = true,
            CriadoEm = DateTime.UtcNow
        };

        var arqInterno = new Arquivo
        {
            Id = Guid.NewGuid(),
            ProjetoId = projetoId,
            Nome = "interno.pdf",
            UrlStorage = "https://s3/interno.pdf",
            Tipo = "Contrato",
            VisivelCliente = false,
            CriadoEm = DateTime.UtcNow
        };

        await repo.Create(arqVisivel);
        await repo.Create(arqInterno);
        await context.SaveChangesAsync();

        var result = (await repo.GetByProjetoId(projetoId, apenasVisiveisCliente: true)).ToList();

        result.Should().HaveCount(1);
        result.First().Nome.Should().Be("visivel.pdf");
    }
}
