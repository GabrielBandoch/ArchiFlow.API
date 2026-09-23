using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Projetos.Enum;
using ArchiFlow.Infrastructure.Data;
using ArchiFlow.Infrastructure.Repositories.Honorarios;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Repositories;

public class PropostaHonorarioRepositoryTests
{
    private static ArchiFlowDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ArchiFlowDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ArchiFlowDbContext(options);
    }

    [Fact]
    public async Task Create_And_GetByIdWithItens_Should_Persist_Correctly()
    {
        using var context = GetInMemoryDbContext();
        var repo = new PropostaHonorarioRepository(context);

        var propostaId = Guid.NewGuid();
        var proposta = new PropostaHonorario
        {
            Id = propostaId,
            Titulo = "Proposta Alpha",
            Codigo = "PROP-2026-0001",
            TipoProjeto = TipoProjeto.Residencial,
            PadraoImovel = PadraoImovel.Medio,
            MetragemQuadrada = 150m,
            ValorTotalSugerido = 18450m,
            ValorFinalAjustado = 18450m,
            Status = StatusProposta.Rascunho,
            CriadoEm = DateTime.UtcNow,
            ItensEtapa = new List<ItemPropostaEtapa>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    PropostaId = propostaId,
                    NomeEtapa = "Estudo Preliminar",
                    Incluso = true,
                    Percentual = 20m,
                    Valor = 3690m,
                    HorasEstimadas = 24m,
                    Ordem = 1
                }
            }
        };

        await repo.Create(proposta);
        await context.SaveChangesAsync();

        var consultado = await repo.GetByIdWithItens(propostaId);

        consultado.Should().NotBeNull();
        consultado!.Titulo.Should().Be("Proposta Alpha");
        consultado.ItensEtapa.Should().HaveCount(1);
    }

    [Fact]
    public async Task GerarProximoCodigo_Should_Format_Sequential_Number()
    {
        using var context = GetInMemoryDbContext();
        var repo = new PropostaHonorarioRepository(context);

        var codigo1 = await repo.GerarProximoCodigo();
        codigo1.Should().StartWith($"PROP-{DateTime.UtcNow.Year}-0001");
    }

    [Fact]
    public async Task Delete_Should_Remove_Entity_When_Exists()
    {
        using var context = GetInMemoryDbContext();
        var repo = new PropostaHonorarioRepository(context);

        var id = Guid.NewGuid();
        var proposta = new PropostaHonorario
        {
            Id = id,
            Titulo = "Para Excluir",
            Codigo = "PROP-2026-0099",
            CriadoEm = DateTime.UtcNow
        };

        await repo.Create(proposta);
        await context.SaveChangesAsync();

        await repo.Delete(id);
        await context.SaveChangesAsync();

        var consultado = await repo.GetById(id);
        consultado.Should().BeNull();
    }
}
