using ArchiFlow.Application.Financeiro.Commands;
using ArchiFlow.Application.Financeiro.DTOs;
using ArchiFlow.Application.Financeiro.Services;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Financeiro;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Projetos.Enum;
using ArchiFlow.Infrastructure.Data;
using ArchiFlow.Infrastructure.Repositories;
using ArchiFlow.Infrastructure.Repositories.Financeiro;
using ArchiFlow.Infrastructure.Repositories.Projetos;
using ArchiFlow.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Financeiro;

public class FinanceiroServiceTests
{
    private static (FinanceiroService service, ArchiFlowDbContext context, Mock<IStorageService> storageMock) CreateService(
        Mock<IStorageService>? storageMock = null,
        bool withoutStorage = false)
    {
        var context = TestDbContextFactory.Create();
        var parcelaRepo = new ParcelaFinanceiraRepository(context);
        var contratoRepo = new ContratoFinanceiroRepository(context);
        var despesaRepo = new DespesaProjetoRepository(context);
        var projetoRepo = new ProjetoRepository(context);
        var unitOfWork = new UnitOfWork(context);
        var logger = NullLogger<FinanceiroService>.Instance;
        storageMock ??= new Mock<IStorageService>();

        var service = new FinanceiroService(
            parcelaRepo,
            contratoRepo,
            despesaRepo,
            projetoRepo,
            unitOfWork,
            logger,
            withoutStorage ? null : storageMock.Object
        );

        return (service, context, storageMock);
    }

    private static Mock<IFormFile> CreateMockFormFile(
        string fileName,
        string contentType,
        long length = 100,
        byte[]? content = null)
    {
        var fileMock = new Mock<IFormFile>();
        var stream = new MemoryStream(content ?? new byte[length]);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.Length).Returns(length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        return fileMock;
    }

    #region Painel e Status

    [Fact]
    public async Task ObterPainelConsolidadoAsync_When_Empty_Should_Return_Zeroed_Painel()
    {
        var (service, _, _) = CreateService();

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
        var (service, context, _) = CreateService();

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
            Status = StatusParcela.Pendente // Effective: Atrasado
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
    public async Task StatusEfetivo_Overdue_Installment_Should_Have_Same_Status_Across_All_Endpoints()
    {
        var (service, context, _) = CreateService();

        var projeto = new Projeto { Id = Guid.NewGuid(), Nome = "Edifício Status" };
        await context.Projetos.AddAsync(projeto);

        var parcelaVencida = new ParcelaFinanceira
        {
            Id = Guid.NewGuid(),
            ProjetoId = projeto.Id,
            Descricao = "Parcela Vencida",
            Valor = 2500m,
            DataVencimento = DateTime.UtcNow.Date.AddDays(-5),
            Status = StatusParcela.Pendente
        };

        await context.ParcelasFinanceiras.AddAsync(parcelaVencida);
        await context.SaveChangesAsync();

        // 1. ObterPainelConsolidadoAsync
        var painel = await service.ObterPainelConsolidadoAsync();
        painel.TotalAtrasado.Should().Be(2500m);
        painel.TotalPendente.Should().Be(0m);
        painel.ParcelasRecentes.First(p => p.Id == parcelaVencida.Id).Status.Should().Be(StatusParcela.Atrasado);
        painel.Alertas.Should().Contain(a => a.ParcelaId == parcelaVencida.Id && a.EmAtraso);

        // 2. ObterParcelasAsync
        var parcelas = (await service.ObterParcelasAsync(projeto.Id, null, null, null)).ToList();
        parcelas.Should().HaveCount(1);
        parcelas[0].Status.Should().Be(StatusParcela.Atrasado);
        parcelas[0].StatusNome.Should().Be("Atrasado");

        // 3. ObterParcelaPorIdAsync
        var parcelaPorId = await service.ObterParcelaPorIdAsync(parcelaVencida.Id);
        parcelaPorId.Should().NotBeNull();
        parcelaPorId!.Status.Should().Be(StatusParcela.Atrasado);
        parcelaPorId.StatusNome.Should().Be("Atrasado");

        // 4. ObterAlertasAsync
        var alertas = (await service.ObterAlertasAsync()).ToList();
        alertas.Should().Contain(a => a.ParcelaId == parcelaVencida.Id && a.EmAtraso);
    }

    [Fact]
    public async Task StatusEfetivo_Pago_And_Cancelado_Past_Due_Should_Never_Convert_To_Atrasado()
    {
        var (service, context, _) = CreateService();

        var projeto = new Projeto { Id = Guid.NewGuid(), Nome = "Edifício Integridade" };
        await context.Projetos.AddAsync(projeto);

        var parcelaPaga = new ParcelaFinanceira
        {
            Id = Guid.NewGuid(),
            ProjetoId = projeto.Id,
            Descricao = "Parcela Paga",
            Valor = 1000m,
            DataVencimento = DateTime.UtcNow.Date.AddDays(-20),
            DataPagamento = DateTime.UtcNow.Date.AddDays(-15),
            Status = StatusParcela.Pago
        };

        var parcelaCancelada = new ParcelaFinanceira
        {
            Id = Guid.NewGuid(),
            ProjetoId = projeto.Id,
            Descricao = "Parcela Cancelada",
            Valor = 1000m,
            DataVencimento = DateTime.UtcNow.Date.AddDays(-10),
            Status = StatusParcela.Cancelado
        };

        await context.ParcelasFinanceiras.AddRangeAsync(parcelaPaga, parcelaCancelada);
        await context.SaveChangesAsync();

        var parcelas = (await service.ObterParcelasAsync(projeto.Id, null, null, null)).ToList();

        var resPaga = parcelas.First(p => p.Id == parcelaPaga.Id);
        resPaga.Status.Should().Be(StatusParcela.Pago);
        resPaga.StatusNome.Should().Be("Pago");

        var resCancelada = parcelas.First(p => p.Id == parcelaCancelada.Id);
        resCancelada.Status.Should().Be(StatusParcela.Cancelado);
        resCancelada.StatusNome.Should().Be("Cancelado");
    }

    [Fact]
    public async Task ObterParcelasAsync_With_Filters_Should_Filter_Accurately()
    {
        var (service, context, _) = CreateService();

        var proj1 = new Projeto { Id = Guid.NewGuid(), Nome = "P1" };
        var proj2 = new Projeto { Id = Guid.NewGuid(), Nome = "P2" };
        await context.Projetos.AddRangeAsync(proj1, proj2);

        var dataRef = DateTime.UtcNow.Date;

        var p1 = new ParcelaFinanceira
        {
            Id = Guid.NewGuid(),
            ProjetoId = proj1.Id,
            Descricao = "P1 Parcela",
            Valor = 1000m,
            DataVencimento = dataRef.AddDays(5),
            Status = StatusParcela.Pendente
        };

        var p2 = new ParcelaFinanceira
        {
            Id = Guid.NewGuid(),
            ProjetoId = proj2.Id,
            Descricao = "P2 Parcela",
            Valor = 2000m,
            DataVencimento = dataRef.AddDays(25),
            Status = StatusParcela.Pendente
        };

        await context.ParcelasFinanceiras.AddRangeAsync(p1, p2);
        await context.SaveChangesAsync();

        // Filter by Project
        var resProj1 = await service.ObterParcelasAsync(proj1.Id, null, null, null);
        resProj1.Should().HaveCount(1);
        resProj1.First().Id.Should().Be(p1.Id);

        // Filter by Date Range
        var resDateRange = await service.ObterParcelasAsync(null, null, dataRef, dataRef.AddDays(10));
        resDateRange.Should().HaveCount(1);
        resDateRange.First().Id.Should().Be(p1.Id);

        // Filter by Status (effective)
        var resStatus = await service.ObterParcelasAsync(null, StatusParcela.Pendente, null, null);
        resStatus.Should().HaveCount(2);
    }

    [Fact]
    public async Task ObterParcelaPorIdAsync_When_Not_Found_Should_Return_Null()
    {
        var (service, _, _) = CreateService();

        var result = await service.ObterParcelaPorIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    #endregion

    #region Validação de Comandos - Contratos e Parcelas

    [Fact]
    public async Task CriarContratoAsync_Should_Generate_Contract_And_Installments()
    {
        var (service, context, _) = CreateService();

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

        // Check ObterContratoPorProjetoIdAsync
        var contratoDb = await service.ObterContratoPorProjetoIdAsync(projeto.Id);
        contratoDb.Should().NotBeNull();
        contratoDb!.Id.Should().Be(resultado.Id);
    }

    [Fact]
    public async Task ObterContratoPorProjetoIdAsync_When_NotFound_Should_Return_Null()
    {
        var (service, _, _) = CreateService();

        var result = await service.ObterContratoPorProjetoIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Theory]
    [InlineData(0, 3, 30)]      // ValorTotal <= 0
    [InlineData(-100, 3, 30)]   // ValorTotal < 0
    [InlineData(1000, 0, 30)]   // NumeroParcelas == 0 (division by zero prevention!)
    [InlineData(1000, -1, 30)]  // NumeroParcelas < 0
    [InlineData(1000, 150, 30)] // NumeroParcelas > 120
    [InlineData(1000, 3, 0)]    // IntervaloDias <= 0
    [InlineData(1000, 3, -10)]  // IntervaloDias < 0
    public async Task CriarContratoAsync_With_Invalid_Parameters_Should_Throw_ArgumentException(
        decimal valorTotal, int numeroParcelas, int intervaloDias)
    {
        var (service, _, _) = CreateService();

        var command = new CriarContratoCommand(
            Guid.NewGuid(),
            valorTotal,
            numeroParcelas,
            DateTime.UtcNow.AddDays(10),
            intervaloDias,
            null,
            null
        );

        var act = () => service.CriarContratoAsync(command);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CriarContratoAsync_With_Empty_ProjectId_Should_Throw_ArgumentException()
    {
        var (service, _, _) = CreateService();

        var command = new CriarContratoCommand(
            Guid.Empty,
            1000m,
            1,
            DateTime.UtcNow.AddDays(10),
            30,
            null,
            null
        );

        var act = () => service.CriarContratoAsync(command);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*ProjetoId é obrigatório.*");
    }

    [Fact]
    public async Task CriarContratoAsync_With_NonExistent_ProjectId_Should_Throw_KeyNotFoundException()
    {
        var (service, _, _) = CreateService();

        var command = new CriarContratoCommand(
            Guid.NewGuid(),
            30000m,
            3,
            DateTime.UtcNow,
            30,
            null,
            null
        );

        var act = () => service.CriarContratoAsync(command);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task RegistrarParcelaAsync_Should_Create_Individual_Installment()
    {
        var (service, context, _) = CreateService();

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

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "Parcela", 100, 1, 1)] // Empty ProjetoId
    [InlineData("11111111-1111-1111-1111-111111111111", "", 100, 1, 1)]        // Empty Descricao
    [InlineData("11111111-1111-1111-1111-111111111111", "   ", 100, 1, 1)]     // Whitespace Descricao
    [InlineData("11111111-1111-1111-1111-111111111111", "Parcela", 0, 1, 1)]    // Valor <= 0
    [InlineData("11111111-1111-1111-1111-111111111111", "Parcela", -50, 1, 1)]  // Valor < 0
    [InlineData("11111111-1111-1111-1111-111111111111", "Parcela", 100, 0, 1)]   // NumeroParcela <= 0
    [InlineData("11111111-1111-1111-1111-111111111111", "Parcela", 100, 1, 0)]   // TotalParcelas <= 0
    public async Task RegistrarParcelaAsync_With_Invalid_Command_Should_Throw_ArgumentException(
        string projetoIdStr, string descricao, decimal valor, int numeroParcela, int totalParcelas)
    {
        var (service, _, _) = CreateService();

        var command = new CriarParcelaCommand(
            Guid.Parse(projetoIdStr),
            null,
            numeroParcela,
            totalParcelas,
            descricao,
            valor,
            DateTime.UtcNow.AddDays(5),
            null
        );

        var act = () => service.RegistrarParcelaAsync(command);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AtualizarParcelaAsync_Should_Update_Fields()
    {
        var (service, context, _) = CreateService();

        var projeto = new Projeto { Id = Guid.NewGuid(), Nome = "Projeto Update Parcela" };
        await context.Projetos.AddAsync(projeto);

        var parcela = new ParcelaFinanceira
        {
            Id = Guid.NewGuid(),
            ProjetoId = projeto.Id,
            Descricao = "Original",
            Valor = 1000m,
            NumeroParcela = 1,
            TotalParcelas = 1,
            DataVencimento = DateTime.UtcNow.AddDays(5),
            Status = StatusParcela.Pendente
        };
        await context.ParcelasFinanceiras.AddAsync(parcela);
        await context.SaveChangesAsync();

        var updateCmd = new AtualizarParcelaCommand(
            "Descricao Atualizada",
            1500m,
            DateTime.UtcNow.AddDays(10),
            StatusParcela.Pendente,
            null,
            null,
            "Obs atualizada",
            null
        );

        var result = await service.AtualizarParcelaAsync(parcela.Id, updateCmd);

        result.Descricao.Should().Be("Descricao Atualizada");
        result.Valor.Should().Be(1500m);
        result.Observacoes.Should().Be("Obs atualizada");
    }

    [Theory]
    [InlineData("", 100)]       // Empty Descricao
    [InlineData("   ", 100)]    // Whitespace Descricao
    [InlineData("Parcela", 0)]   // Valor <= 0
    [InlineData("Parcela", -10)] // Valor < 0
    public async Task AtualizarParcelaAsync_With_Invalid_Command_Should_Throw_ArgumentException(
        string descricao, decimal valor)
    {
        var (service, _, _) = CreateService();

        var command = new AtualizarParcelaCommand(
            descricao,
            valor,
            DateTime.UtcNow.AddDays(5),
            StatusParcela.Pendente,
            null,
            null,
            null,
            null
        );

        var act = () => service.AtualizarParcelaAsync(Guid.NewGuid(), command);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AtualizarParcelaAsync_When_Not_Found_Should_Throw_KeyNotFoundException()
    {
        var (service, _, _) = CreateService();

        var command = new AtualizarParcelaCommand(
            "Parcela",
            100m,
            DateTime.UtcNow,
            StatusParcela.Pendente,
            null,
            null,
            null,
            null
        );

        var act = () => service.AtualizarParcelaAsync(Guid.NewGuid(), command);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DarBaixaParcelaAsync_Should_Mark_Installment_As_Paid()
    {
        var (service, context, _) = CreateService();

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
    public async Task DarBaixaParcelaAsync_With_Future_PaymentDate_Should_Throw_ArgumentException()
    {
        var (service, _, _) = CreateService();

        var command = new DarBaixaParcelaCommand(
            DataPagamento: DateTime.UtcNow.AddDays(10), // Far in future
            FormaPagamento: FormaPagamento.Pix,
            Observacoes: null,
            ComprovanteUrl: null
        );

        var act = () => service.DarBaixaParcelaAsync(Guid.NewGuid(), command);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Data de pagamento não pode ser futura.*");
    }

    [Fact]
    public async Task DarBaixaParcelaAsync_When_NotFound_Should_Throw_KeyNotFoundException()
    {
        var (service, _, _) = CreateService();

        var command = new DarBaixaParcelaCommand(DateTime.UtcNow, FormaPagamento.Pix, null, null);

        var act = () => service.DarBaixaParcelaAsync(Guid.NewGuid(), command);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ExcluirParcelaAsync_Should_Delete_Parcela()
    {
        var (service, context, _) = CreateService();

        var projeto = new Projeto { Id = Guid.NewGuid(), Nome = "P1" };
        await context.Projetos.AddAsync(projeto);

        var parcela = new ParcelaFinanceira
        {
            Id = Guid.NewGuid(),
            ProjetoId = projeto.Id,
            Descricao = "A Deletar",
            Valor = 100m,
            DataVencimento = DateTime.UtcNow
        };
        await context.ParcelasFinanceiras.AddAsync(parcela);
        await context.SaveChangesAsync();

        await service.ExcluirParcelaAsync(parcela.Id);

        var dbParcela = await service.ObterParcelaPorIdAsync(parcela.Id);
        dbParcela.Should().BeNull();
    }

    [Fact]
    public async Task ExcluirParcelaAsync_When_Not_Found_Should_Throw_KeyNotFoundException()
    {
        var (service, _, _) = CreateService();

        var act = () => service.ExcluirParcelaAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Despesas CRUD e Validação

    [Fact]
    public async Task Despesa_CRUD_Operations_Should_Work_Correctly()
    {
        var (service, context, _) = CreateService();

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
    public async Task ObterDespesasAsync_With_Filters_Should_Filter_Accurately()
    {
        var (service, context, _) = CreateService();

        var proj1 = new Projeto { Id = Guid.NewGuid(), Nome = "P1" };
        var proj2 = new Projeto { Id = Guid.NewGuid(), Nome = "P2" };
        await context.Projetos.AddRangeAsync(proj1, proj2);

        var dataRef = DateTime.UtcNow.Date;

        var d1 = new DespesaProjeto
        {
            Id = Guid.NewGuid(),
            ProjetoId = proj1.Id,
            Descricao = "Plotagem",
            Valor = 300m,
            DataDespesa = dataRef,
            Categoria = CategoriaDespesa.PlotagemImpressao
        };

        var d2 = new DespesaProjeto
        {
            Id = Guid.NewGuid(),
            ProjetoId = proj2.Id,
            Descricao = "Uber",
            Valor = 80m,
            DataDespesa = dataRef.AddDays(5),
            Categoria = CategoriaDespesa.DeslocamentoVisita
        };

        await context.DespesasProjetos.AddRangeAsync(d1, d2);
        await context.SaveChangesAsync();

        var resProj = await service.ObterDespesasAsync(proj1.Id, null, null, null);
        resProj.Should().HaveCount(1);
        resProj.First().Id.Should().Be(d1.Id);

        var resCat = await service.ObterDespesasAsync(null, CategoriaDespesa.DeslocamentoVisita, null, null);
        resCat.Should().HaveCount(1);
        resCat.First().Id.Should().Be(d2.Id);

        var resDate = await service.ObterDespesasAsync(null, null, dataRef, dataRef.AddDays(1));
        resDate.Should().HaveCount(1);
        resDate.First().Id.Should().Be(d1.Id);
    }

    [Theory]
    [InlineData("11111111-1111-1111-1111-111111111111", "", 100)]         // Empty Descricao
    [InlineData("11111111-1111-1111-1111-111111111111", "   ", 100)]      // Whitespace Descricao
    [InlineData("11111111-1111-1111-1111-111111111111", "Plotagem", 0)]     // Valor <= 0
    [InlineData("11111111-1111-1111-1111-111111111111", "Plotagem", -10)]   // Valor < 0
    public async Task CriarDespesaAsync_With_Invalid_Parameters_Should_Throw_ArgumentException(
        string projetoIdStr, string descricao, decimal valor)
    {
        var (service, _, _) = CreateService();

        var command = new CriarDespesaCommand(
            Guid.Parse(projetoIdStr),
            descricao,
            valor,
            DateTime.UtcNow,
            CategoriaDespesa.PlotagemImpressao,
            null,
            null
        );

        var act = () => service.CriarDespesaAsync(command);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CriarDespesaAsync_SemProjeto_DeveCriarDespesaGeralComSucesso()
    {
        var (service, _, _) = CreateService();

        var command = new CriarDespesaCommand(
            null,
            "Aluguel Escritório",
            3500m,
            DateTime.UtcNow,
            CategoriaDespesa.Outros,
            "Despesa geral",
            null
        );

        var res = await service.CriarDespesaAsync(command);

        res.Should().NotBeNull();
        res.ProjetoId.Should().BeNull();
        res.Descricao.Should().Be("Aluguel Escritório");
        res.Valor.Should().Be(3500m);
    }

    [Theory]
    [InlineData("", 100)]         // Empty Descricao
    [InlineData("   ", 100)]      // Whitespace Descricao
    [InlineData("Plotagem", 0)]     // Valor <= 0
    [InlineData("Plotagem", -10)]   // Valor < 0
    public async Task AtualizarDespesaAsync_With_Invalid_Parameters_Should_Throw_ArgumentException(
        string descricao, decimal valor)
    {
        var (service, _, _) = CreateService();

        var command = new AtualizarDespesaCommand(
            descricao,
            valor,
            DateTime.UtcNow,
            CategoriaDespesa.PlotagemImpressao,
            null,
            null
        );

        var act = () => service.AtualizarDespesaAsync(Guid.NewGuid(), command);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AtualizarDespesaAsync_When_Not_Found_Should_Throw_KeyNotFoundException()
    {
        var (service, _, _) = CreateService();

        var command = new AtualizarDespesaCommand(
            "Despesa",
            100m,
            DateTime.UtcNow,
            CategoriaDespesa.Outros,
            null,
            null
        );

        var act = () => service.AtualizarDespesaAsync(Guid.NewGuid(), command);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ExcluirDespesaAsync_When_Not_Found_Should_Throw_KeyNotFoundException()
    {
        var (service, _, _) = CreateService();

        var act = () => service.ExcluirDespesaAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Upload Seguro de Comprovante e Exclusão

    [Fact]
    public async Task UploadComprovanteAsync_When_File_Null_Should_Throw_ArgumentException()
    {
        var (service, _, _) = CreateService();
        var command = new UploadComprovanteCommand(null);

        var act = () => service.UploadComprovanteAsync(command);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Nenhum arquivo enviado.*");
    }

    [Fact]
    public async Task UploadComprovanteAsync_When_File_Empty_Should_Throw_ArgumentException()
    {
        var (service, _, _) = CreateService();
        var fileMock = CreateMockFormFile("recibo.pdf", "application/pdf", length: 0);
        var command = new UploadComprovanteCommand(fileMock.Object);

        var act = () => service.UploadComprovanteAsync(command);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Nenhum arquivo enviado.*");
    }

    [Fact]
    public async Task UploadComprovanteAsync_When_File_Exceeds_20MB_Should_Throw_ArgumentException()
    {
        var (service, _, _) = CreateService();
        long twentyOneMb = (21L * 1024 * 1024);
        var fileMock = CreateMockFormFile("recibo.pdf", "application/pdf", length: twentyOneMb);
        var command = new UploadComprovanteCommand(fileMock.Object);

        var act = () => service.UploadComprovanteAsync(command);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*limite máximo permitido de 20 MB.*");
    }

    [Theory]
    [InlineData("script.exe", "application/octet-stream")]
    [InlineData("documento.zip", "application/zip")]
    [InlineData("index.html", "text/html")]
    [InlineData("arquivo.txt", "text/plain")]
    [InlineData("script.sh", "application/x-sh")]
    public async Task UploadComprovanteAsync_When_Unsupported_Extension_Should_Throw_ArgumentException(
        string fileName, string contentType)
    {
        var (service, _, _) = CreateService();
        var fileMock = CreateMockFormFile(fileName, contentType, length: 1024);
        var command = new UploadComprovanteCommand(fileMock.Object);

        var act = () => service.UploadComprovanteAsync(command);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Extensão de arquivo não permitida*");
    }

    [Fact]
    public async Task UploadComprovanteAsync_When_Unsupported_ContentType_Should_Throw_ArgumentException()
    {
        var (service, _, _) = CreateService();
        // File extension looks like PDF, but mime type is executable
        var fileMock = CreateMockFormFile("recibo.pdf", "application/x-msdownload", length: 1024);
        var command = new UploadComprovanteCommand(fileMock.Object);

        var act = () => service.UploadComprovanteAsync(command);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Tipo de conteúdo*");
    }

    [Fact]
    public async Task UploadComprovanteAsync_When_Storage_Unavailable_Should_Throw_InvalidOperationException()
    {
        var (service, _, _) = CreateService(withoutStorage: true);
        var fileMock = CreateMockFormFile("recibo.pdf", "application/pdf", length: 1024);
        var command = new UploadComprovanteCommand(fileMock.Object);

        var act = () => service.UploadComprovanteAsync(command);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Serviço de armazenamento não configurado.*");
    }

    [Theory]
    [InlineData("recibo.pdf", "application/pdf", ".pdf")]
    [InlineData("foto.png", "image/png", ".png")]
    [InlineData("foto.jpg", "image/jpeg", ".jpg")]
    [InlineData("foto.jpeg", "image/jpeg", ".jpeg")]
    public async Task UploadComprovanteAsync_With_Valid_File_Should_Use_Safe_Key_And_Upload(
        string clientFileName, string contentType, string expectedExtension)
    {
        string? capturedKey = null;
        var storageMock = new Mock<IStorageService>();
        storageMock.Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), contentType))
            .Callback<Stream, string, string>((_, key, _) => capturedKey = key)
            .ReturnsAsync("https://s3.amazonaws.com/generated-url");

        var (service, _, _) = CreateService(storageMock: storageMock);
        var fileMock = CreateMockFormFile(clientFileName, contentType, length: 2048);
        var command = new UploadComprovanteCommand(fileMock.Object);

        var result = await service.UploadComprovanteAsync(command);

        result.Should().NotBeNull();
        result.Url.Should().Be("https://s3.amazonaws.com/generated-url");
        result.Nome.Should().Be(clientFileName);

        // Security check: Client filename was NOT used as key, server generated a safe GUID key
        capturedKey.Should().NotBeNull();
        capturedKey.Should().NotBe(clientFileName);
        capturedKey.Should().EndWith(expectedExtension);
        var keyWithoutExt = Path.GetFileNameWithoutExtension(capturedKey);
        Guid.TryParse(keyWithoutExt, out _).Should().BeTrue();
    }

    [Fact]
    public async Task ExcluirComprovanteAsync_Should_Delegate_To_Storage_DeleteAsync()
    {
        var storageMock = new Mock<IStorageService>();
        var (service, _, _) = CreateService(storageMock: storageMock);

        var url = "https://s3.amazonaws.com/recibo.pdf";
        await service.ExcluirComprovanteAsync(url);

        storageMock.Verify(s => s.DeleteAsync(url), Times.Once);
    }

    [Fact]
    public async Task ExcluirComprovanteAsync_When_Url_Null_Or_Whitespace_Should_Return_Safely()
    {
        var storageMock = new Mock<IStorageService>();
        var (service, _, _) = CreateService(storageMock: storageMock);

        await service.ExcluirComprovanteAsync("");
        await service.ExcluirComprovanteAsync("   ");

        storageMock.Verify(s => s.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion
}
