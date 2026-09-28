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
}
