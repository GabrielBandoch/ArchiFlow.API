using ArchiFlow.Application.Financeiro.Commands;
using ArchiFlow.Application.Financeiro.DTOs;
using ArchiFlow.Application.Financeiro.Services;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Financeiro;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Projetos.Enum;
using ArchiFlow.Infrastructure.Data;
using ArchiFlow.Infrastructure.Repositories;
using ArchiFlow.Infrastructure.Repositories.Financeiro;
using ArchiFlow.Infrastructure.Repositories.Projetos;
using ArchiFlow.Tests.Common;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ArchiFlow.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Financeiro;

public class FinanceiroServiceTests
{
    private static (FinanceiroService service, ArchiFlowDbContext context) CreateService()
    {
        var context = TestDbContextFactory.Create();
        var parcelaRepo = new ParcelaFinanceiraRepository(context);
        var contratoRepo = new ContratoFinanceiroRepository(context);
        var despesaRepo = new DespesaProjetoRepository(context);
        var projetoRepo = new ProjetoRepository(context);
        var unitOfWork = new UnitOfWork(context);
        var mapperConfig = new MapperConfiguration(cfg => { });
        var mapper = mapperConfig.CreateMapper();
        var logger = NullLogger<FinanceiroService>.Instance;

        var service = new FinanceiroService(
            parcelaRepo,
            contratoRepo,
            despesaRepo,
            projetoRepo,
            unitOfWork,
            mapper,
            logger
        );

        return (service, context);
    }

    [Fact]
    public async Task ObterPainelConsolidadoAsync_When_Empty_Should_Return_Zeroed_Painel()
    {
        var (service, _) = CreateService();

        var painel = await service.ObterPainelConsolidadoAsync();

        painel.Should().NotBeNull();
        painel.TotalPrevisto.Should().Be(0m);
        painel.TotalRecebido.Should().Be(0m);
        painel.TotalPendente.Should().Be(0m);
        painel.TotalAtrasado.Should().Be(0m);
        painel.TotalDespesas.Should().Be(0m);
        painel.SaldoLiquido.Should().Be(0m);
        painel.ReceitasPorMes.Should().HaveCount(12);
        painel.Alertas.Should().BeEmpty();
        painel.ParcelasRecentes.Should().BeEmpty();
    }

    [Fact]
    public async Task ObterPainelConsolidadoAsync_With_Data_Should_Calculate_Totals_Correctly()
    {
        var (service, context) = CreateService();

        var projeto = new Projeto
        {
            Id = Guid.NewGuid(),
            Nome = "Residência Vista Verde",
            Status = StatusProjeto.Desenvolvimento
        };
        await context.Projetos.AddAsync(projeto);

        var hoje = DateTime.UtcNow.Date;

        var p1 = new ParcelaFinanceira
        {
            Id = Guid.NewGuid(),
            ProjetoId = projeto.Id,
            Descricao = "Parcela 1",
            Valor = 10000m,
            DataVencimento = hoje.AddDays(-10),
            DataPagamento = hoje.AddDays(-10),
            Status = StatusParcela.Pago
        };

        var p2 = new ParcelaFinanceira
        {
            Id = Guid.NewGuid(),
            ProjetoId = projeto.Id,
            Descricao = "Parcela 2",
            Valor = 5000m,
            DataVencimento = hoje.AddDays(5),
            Status = StatusParcela.Pendente
        };

        var p3 = new ParcelaFinanceira
        {
            Id = Guid.NewGuid(),
            ProjetoId = projeto.Id,
            Descricao = "Parcela 3",
            Valor = 3000m,
            DataVencimento = hoje.AddDays(-2),
            Status = StatusParcela.Pendente // Should be converted to Atrasado
        };

        await context.ParcelasFinanceiras.AddRangeAsync(p1, p2, p3);

        var despesa = new DespesaProjeto
        {
            Id = Guid.NewGuid(),
            ProjetoId = projeto.Id,
            Descricao = "Plotagem",
            Valor = 1200m,
            DataDespesa = hoje,
            Categoria = CategoriaDespesa.PlotagemImpressao
        };
        await context.DespesasProjetos.AddAsync(despesa);
        await context.SaveChangesAsync();

        var painel = await service.ObterPainelConsolidadoAsync();

        painel.TotalPrevisto.Should().Be(18000m);
        painel.TotalRecebido.Should().Be(10000m);
        painel.TotalPendente.Should().Be(5000m);
        painel.TotalAtrasado.Should().Be(3000m);
        painel.TotalDespesas.Should().Be(1200m);
        painel.SaldoLiquido.Should().Be(8800m);
        painel.Alertas.Should().HaveCount(2); // p2 (due in 5 days) + p3 (overdue)
    }

    [Fact]
    public async Task CriarContratoAsync_Should_Generate_Contract_And_Installments()
    {
        var (service, context) = CreateService();

        var projeto = new Projeto
        {
            Id = Guid.NewGuid(),
            Nome = "Edifício Horizonte",
            Status = StatusProjeto.Desenvolvimento
        };
        await context.Projetos.AddAsync(projeto);
        await context.SaveChangesAsync();

        var command = new CriarContratoCommand(
            projeto.Id,
            ValorTotal: 30000m,
            NumeroParcelas: 3,
            DataPrimeiroVencimento: DateTime.UtcNow.Date.AddDays(15),
            IntervaloDias: 30,
            CondicoesPagamento: "Entrada + 2x",
            Observacoes: "Contrato firmado"
        );

        var resultado = await service.CriarContratoAsync(command);

        resultado.Should().NotBeNull();
        resultado.ValorTotal.Should().Be(30000m);
        resultado.Parcelas.Should().HaveCount(3);
        resultado.Parcelas.Sum(p => p.Valor).Should().Be(30000m);
        resultado.Parcelas[0].NumeroParcela.Should().Be(1);
        resultado.Parcelas[1].NumeroParcela.Should().Be(2);
        resultado.Parcelas[2].NumeroParcela.Should().Be(3);
    }

    [Fact]
    public async Task CriarContratoAsync_With_Invalid_ProjectId_Should_Throw_KeyNotFoundException()
    {
        var (service, _) = CreateService();

        var command = new CriarContratoCommand(
            Guid.NewGuid(),
            30000m,
            3,
            DateTime.UtcNow,
            30,
            null,
            null
        );

        var act = async () => await service.CriarContratoAsync(command);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task RegistrarParcelaAsync_Should_Create_Individual_Installment()
    {
        var (service, context) = CreateService();

        var projeto = new Projeto
        {
            Id = Guid.NewGuid(),
            Nome = "Reforma Comercial",
            Status = StatusProjeto.Desenvolvimento
        };
        await context.Projetos.AddAsync(projeto);
        await context.SaveChangesAsync();

        var command = new CriarParcelaCommand(
            projeto.Id,
            null,
            1,
            1,
            "Honorário Único",
            8500m,
            DateTime.UtcNow.AddDays(10),
            "Sem observações"
        );

        var parcela = await service.RegistrarParcelaAsync(command);

        parcela.Should().NotBeNull();
        parcela.Valor.Should().Be(8500m);
        parcela.Descricao.Should().Be("Honorário Único");
        parcela.Status.Should().Be(StatusParcela.Pendente);
    }

    [Fact]
    public async Task DarBaixaParcelaAsync_Should_Mark_Installment_As_Paid()
    {
        var (service, context) = CreateService();

        var projeto = new Projeto { Id = Guid.NewGuid(), Nome = "Casa de Praia" };
        await context.Projetos.AddAsync(projeto);

        var parcela = new ParcelaFinanceira
        {
            Id = Guid.NewGuid(),
            ProjetoId = projeto.Id,
            Descricao = "Parcela 1",
            Valor = 4000m,
            DataVencimento = DateTime.UtcNow.AddDays(5),
            Status = StatusParcela.Pendente
        };
        await context.ParcelasFinanceiras.AddAsync(parcela);
        await context.SaveChangesAsync();

        var command = new DarBaixaParcelaCommand(
            DataPagamento: DateTime.UtcNow,
            FormaPagamento: FormaPagamento.Pix,
            Observacoes: "Pago via Pix com sucesso",
            ComprovanteUrl: "https://comprovantes.com/123.pdf"
        );

        var resultado = await service.DarBaixaParcelaAsync(parcela.Id, command);

        resultado.Status.Should().Be(StatusParcela.Pago);
        resultado.FormaPagamento.Should().Be(FormaPagamento.Pix);
        resultado.DataPagamento.Should().NotBeNull();
        resultado.Observacoes.Should().Be("Pago via Pix com sucesso");
    }

    [Fact]
    public async Task Despesa_CRUD_Operations_Should_Work_Correctly()
    {
        var (service, context) = CreateService();

        var projeto = new Projeto { Id = Guid.NewGuid(), Nome = "Edifício Solar" };
        await context.Projetos.AddAsync(projeto);
        await context.SaveChangesAsync();

        // Create
        var criarCommand = new CriarDespesaCommand(
            projeto.Id,
            "Taxas da Prefeitura",
            650m,
            DateTime.UtcNow,
            CategoriaDespesa.TaxasPrefeitura,
            "Alvará de construção",
            null
        );

        var despesa = await service.CriarDespesaAsync(criarCommand);
        despesa.Should().NotBeNull();
        despesa.Valor.Should().Be(650m);
        despesa.Categoria.Should().Be(CategoriaDespesa.TaxasPrefeitura);

        // Update
        var updateCommand = new AtualizarDespesaCommand(
            "Taxas de Alvará Atualizadas",
            700m,
            DateTime.UtcNow,
            CategoriaDespesa.TaxasPrefeitura,
            "Revisado",
            null
        );
        var despesaAtualizada = await service.AtualizarDespesaAsync(despesa.Id, updateCommand);
        despesaAtualizada.Valor.Should().Be(700m);
        despesaAtualizada.Descricao.Should().Be("Taxas de Alvará Atualizadas");

        // Delete
        await service.ExcluirDespesaAsync(despesa.Id);
        var despesaAposExclusao = await service.ObterDespesaPorIdAsync(despesa.Id);
        despesaAposExclusao.Should().BeNull();
    }

    [Fact]
    public async Task UploadComprovanteAsync_When_File_Null_Should_Throw_ArgumentException()
    {
        var (service, _) = CreateService();
        var command = new UploadComprovanteCommand(null);

        var act = () => service.UploadComprovanteAsync(command);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Nenhum arquivo enviado.*");
    }

    [Fact]
    public async Task UploadComprovanteAsync_When_Storage_Configured_Should_Upload_And_Return_Dto()
    {
        var storageMock = new Mock<IStorageService>();
        storageMock.Setup(s => s.UploadAsync(It.IsAny<System.IO.Stream>(), "recibo.pdf", "application/pdf"))
            .ReturnsAsync("https://s3.amazonaws.com/recibo.pdf");

        var fileMock = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
        var stream = new System.IO.MemoryStream(new byte[] { 1, 2, 3 });
        fileMock.Setup(f => f.Length).Returns(3);
        fileMock.Setup(f => f.FileName).Returns("recibo.pdf");
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

        var options = new DbContextOptionsBuilder<ArchiFlowDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new ArchiFlowDbContext(options);
        var service = new FinanceiroService(
            new ParcelaFinanceiraRepository(context),
            new ContratoFinanceiroRepository(context),
            new DespesaProjetoRepository(context),
            new ProjetoRepository(context),
            new UnitOfWork(context),
            new MapperConfiguration(cfg => { }).CreateMapper(),
            NullLogger<FinanceiroService>.Instance,
            storageMock.Object
        );

        var command = new UploadComprovanteCommand(fileMock.Object);
        var result = await service.UploadComprovanteAsync(command);

        result.Should().NotBeNull();
        result.Url.Should().Be("https://s3.amazonaws.com/recibo.pdf");
        result.Nome.Should().Be("recibo.pdf");
    }
}
