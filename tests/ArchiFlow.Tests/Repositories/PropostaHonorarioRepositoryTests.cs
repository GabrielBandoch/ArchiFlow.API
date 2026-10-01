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
    public async Task GerarProximoCodigo_Should_Increment_Based_On_Highest_Existing_Code()
    {
        using var context = GetInMemoryDbContext();
        var repo = new PropostaHonorarioRepository(context);

        var ano = DateTime.UtcNow.Year;
        await repo.Create(new PropostaHonorario
        {
            Id = Guid.NewGuid(),
            Titulo = "Proposta Existente",
            Codigo = $"PROP-{ano}-0005",
            CriadoEm = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var proximoCodigo = await repo.GerarProximoCodigo();
        proximoCodigo.Should().Be($"PROP-{ano}-0006");
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

    [Fact]
    public async Task ConfiguracaoPropostaRepository_ObterPorUsuarioIdAsync_Should_Return_Correct_Config()
    {
        using var context = GetInMemoryDbContext();
        var repo = new ConfiguracaoPropostaRepository(context);

        var usuarioId1 = Guid.NewGuid();
        var usuarioId2 = Guid.NewGuid();

        var config1 = new ConfiguracaoProposta
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId1,
            NomeEscritorio = "Studio 1",
            Configurado = true
        };

        var config2 = new ConfiguracaoProposta
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId2,
            NomeEscritorio = "Studio 2",
            Configurado = true
        };

        await repo.Create(config1);
        await repo.Create(config2);
        await context.SaveChangesAsync();

        var res1 = await repo.ObterPorUsuarioIdAsync(usuarioId1);
        res1.Should().NotBeNull();
        res1!.NomeEscritorio.Should().Be("Studio 1");

        var resInexistente = await repo.ObterPorUsuarioIdAsync(Guid.NewGuid());
        resInexistente.Should().BeNull();
    }
}
