using ArchiFlow.Domain.Financeiro;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Projetos.Enum;
using ArchiFlow.Infrastructure.Data;
using ArchiFlow.Infrastructure.Repositories.Financeiro;
using ArchiFlow.Tests.Common;
using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Financeiro;

public class FinanceiroRepositoryTests
{
    private readonly ArchiFlowDbContext _context;
    private readonly ParcelaFinanceiraRepository _parcelaRepo;
    private readonly ContratoFinanceiroRepository _contratoRepo;
    private readonly DespesaProjetoRepository _despesaRepo;

    public FinanceiroRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
        _parcelaRepo = new ParcelaFinanceiraRepository(_context);
        _contratoRepo = new ContratoFinanceiroRepository(_context);
        _despesaRepo = new DespesaProjetoRepository(_context);
    }

    [Fact]
    public async Task ParcelaRepository_ObterPorProjetoIdAsync_Should_Return_Only_For_That_Project()
    {
        var proj1 = new Projeto { Id = Guid.NewGuid(), Nome = "P1" };
        var proj2 = new Projeto { Id = Guid.NewGuid(), Nome = "P2" };
        await _context.Projetos.AddRangeAsync(proj1, proj2);

        var p1 = new ParcelaFinanceira { Id = Guid.NewGuid(), ProjetoId = proj1.Id, Descricao = "Parcela P1", Valor = 1000m, DataVencimento = DateTime.UtcNow };
        var p2 = new ParcelaFinanceira { Id = Guid.NewGuid(), ProjetoId = proj2.Id, Descricao = "Parcela P2", Valor = 2000m, DataVencimento = DateTime.UtcNow };
        await _context.ParcelasFinanceiras.AddRangeAsync(p1, p2);
        await _context.SaveChangesAsync();

        var resultado = await _parcelaRepo.ObterPorProjetoIdAsync(proj1.Id);

        resultado.Should().HaveCount(1);
        resultado.First().ProjetoId.Should().Be(proj1.Id);
    }

    [Fact]
    public async Task ContratoRepository_ObterPorProjetoIdAsync_Should_Return_Contract_With_Installments()
    {
        var proj = new Projeto { Id = Guid.NewGuid(), Nome = "Projeto Contrato" };
        await _context.Projetos.AddAsync(proj);

        var contrato = new ContratoFinanceiro { Id = Guid.NewGuid(), ProjetoId = proj.Id, ValorTotal = 12000m, CriadoEm = DateTime.UtcNow };
        await _context.ContratosFinanceiros.AddAsync(contrato);

        var parcela = new ParcelaFinanceira
        {
            Id = Guid.NewGuid(),
            ProjetoId = proj.Id,
            ContratoFinanceiroId = contrato.Id,
            Descricao = "1/1",
            Valor = 12000m,
            DataVencimento = DateTime.UtcNow
        };
        await _context.ParcelasFinanceiras.AddAsync(parcela);
        await _context.SaveChangesAsync();

        var resultado = await _contratoRepo.ObterPorProjetoIdAsync(proj.Id);

        resultado.Should().NotBeNull();
        resultado!.ValorTotal.Should().Be(12000m);
        resultado.Parcelas.Should().HaveCount(1);
    }

    [Fact]
    public async Task DespesaRepository_ObterComFiltroAsync_Should_Filter_By_Category()
    {
        var proj = new Projeto { Id = Guid.NewGuid(), Nome = "Projeto Despesa" };
        await _context.Projetos.AddAsync(proj);

        var d1 = new DespesaProjeto { Id = Guid.NewGuid(), ProjetoId = proj.Id, Descricao = "Plotagem", Valor = 300m, DataDespesa = DateTime.UtcNow, Categoria = CategoriaDespesa.PlotagemImpressao };
        var d2 = new DespesaProjeto { Id = Guid.NewGuid(), ProjetoId = proj.Id, Descricao = "Uber", Valor = 50m, DataDespesa = DateTime.UtcNow, Categoria = CategoriaDespesa.DeslocamentoVisita };
        await _context.DespesasProjetos.AddRangeAsync(d1, d2);
        await _context.SaveChangesAsync();

        var resultado = await _despesaRepo.ObterComFiltroAsync(proj.Id, CategoriaDespesa.PlotagemImpressao, null, null);

        resultado.Should().HaveCount(1);
        resultado.First().Categoria.Should().Be(CategoriaDespesa.PlotagemImpressao);
    }

    [Fact]
    public async Task DespesaRepository_ObterComFiltroAsync_Should_Filter_By_Date_Range_With_Unspecified_Kind()
    {
        var proj = new Projeto { Id = Guid.NewGuid(), Nome = "Projeto Despesa Datas" };
        await _context.Projetos.AddAsync(proj);

        var dDentro = new DespesaProjeto
        {
            Id = Guid.NewGuid(),
            ProjetoId = proj.Id,
            Descricao = "Dentro do periodo",
            Valor = 150m,
            DataDespesa = new DateTime(2026, 10, 15, 14, 30, 0, DateTimeKind.Utc),
            Categoria = CategoriaDespesa.Outros
        };
        var dNoUltimoDia = new DespesaProjeto
        {
            Id = Guid.NewGuid(),
            ProjetoId = proj.Id,
            Descricao = "No limite do ultimo dia",
            Valor = 200m,
            DataDespesa = new DateTime(2026, 10, 18, 22, 0, 0, DateTimeKind.Utc),
            Categoria = CategoriaDespesa.Outros
        };
        var dFora = new DespesaProjeto
        {
            Id = Guid.NewGuid(),
            ProjetoId = proj.Id,
            Descricao = "Fora do periodo",
            Valor = 80m,
            DataDespesa = new DateTime(2026, 10, 19, 10, 0, 0, DateTimeKind.Utc),
            Categoria = CategoriaDespesa.Outros
        };

        await _context.DespesasProjetos.AddRangeAsync(dDentro, dNoUltimoDia, dFora);
        await _context.SaveChangesAsync();

        var inicioUnspecified = new DateTime(2026, 10, 12, 0, 0, 0, DateTimeKind.Unspecified);
        var fimUnspecified = new DateTime(2026, 10, 18, 0, 0, 0, DateTimeKind.Unspecified);

        var resultado = await _despesaRepo.ObterComFiltroAsync(proj.Id, null, inicioUnspecified, fimUnspecified);

        resultado.Should().HaveCount(2);
        resultado.Select(d => d.Id).Should().Contain(new[] { dDentro.Id, dNoUltimoDia.Id });
        resultado.Select(d => d.Id).Should().NotContain(dFora.Id);
    }

    [Fact]
    public async Task ParcelaRepository_ObterComFiltroAsync_Should_Filter_By_Date_Range_With_Unspecified_Kind()
    {
        var proj = new Projeto { Id = Guid.NewGuid(), Nome = "Projeto Parcela Datas" };
        await _context.Projetos.AddAsync(proj);

        var pDentro = new ParcelaFinanceira
        {
            Id = Guid.NewGuid(),
            ProjetoId = proj.Id,
            Descricao = "Parcela Dentro",
            Valor = 500m,
            DataVencimento = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc)
        };
        var pNoUltimoDia = new ParcelaFinanceira
        {
            Id = Guid.NewGuid(),
            ProjetoId = proj.Id,
            Descricao = "Parcela No Limite",
            Valor = 500m,
            DataVencimento = new DateTime(2026, 10, 18, 18, 0, 0, DateTimeKind.Utc)
        };
        var pFora = new ParcelaFinanceira
        {
            Id = Guid.NewGuid(),
            ProjetoId = proj.Id,
            Descricao = "Parcela Fora",
            Valor = 500m,
            DataVencimento = new DateTime(2026, 10, 25, 0, 0, 0, DateTimeKind.Utc)
        };

        await _context.ParcelasFinanceiras.AddRangeAsync(pDentro, pNoUltimoDia, pFora);
        await _context.SaveChangesAsync();

        var inicioUnspecified = new DateTime(2026, 10, 12, 0, 0, 0, DateTimeKind.Unspecified);
        var fimUnspecified = new DateTime(2026, 10, 18, 0, 0, 0, DateTimeKind.Unspecified);

        var resultado = await _parcelaRepo.ObterComFiltroAsync(proj.Id, null, inicioUnspecified, fimUnspecified);

        resultado.Should().HaveCount(2);
        resultado.Select(p => p.Id).Should().Contain(new[] { pDentro.Id, pNoUltimoDia.Id });
        resultado.Select(p => p.Id).Should().NotContain(pFora.Id);
    }
}
