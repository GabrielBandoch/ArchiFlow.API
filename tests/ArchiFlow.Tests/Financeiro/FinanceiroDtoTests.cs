using ArchiFlow.Application.Financeiro.Commands;
using ArchiFlow.Application.Financeiro.DTOs;
using ArchiFlow.Domain.Financeiro;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace ArchiFlow.Tests.Financeiro;

public class FinanceiroDtoTests
{
    [Fact]
    public void Dtos_Should_Initialize_And_Hold_Values_Correctly()
    {
        var id = Guid.NewGuid();
        var projId = Guid.NewGuid();
        var agora = DateTime.UtcNow;

        var parcelaDto = new ParcelaFinanceiraDto(
            id, projId, "Residencial A", null, null, null,
            1, 3, "Entrada", 5000m, agora, agora,
            StatusParcela.Pago, "Pago", FormaPagamento.Pix, "Pix", "Obs", "url", agora
        );

        parcelaDto.Id.Should().Be(id);
        parcelaDto.Valor.Should().Be(5000m);
        parcelaDto.Status.Should().Be(StatusParcela.Pago);
        parcelaDto.FormaPagamento.Should().Be(FormaPagamento.Pix);

        var contratoDto = new ContratoFinanceiroDto(
            id, projId, "Residencial A", 15000m, "3x sem juros", "Obs", agora,
            new List<ParcelaFinanceiraDto> { parcelaDto }
        );

        contratoDto.ValorTotal.Should().Be(15000m);
        contratoDto.Parcelas.Should().HaveCount(1);

        var despesaDto = new DespesaProjetoDto(
            id, projId, "Residencial A", "Plotagens A1", 450m, agora,
            CategoriaDespesa.PlotagemImpressao, "PlotagemImpressao", "Obs", null, agora
        );

        despesaDto.Valor.Should().Be(450m);
        despesaDto.Categoria.Should().Be(CategoriaDespesa.PlotagemImpressao);

        var receitaMes = new ReceitaMesDto("JAN", 1, 2026, 12000m, 15000m, 2000m);
        receitaMes.Mes.Should().Be("JAN");
        receitaMes.ValorRecebido.Should().Be(12000m);

        var alerta = new AlertaFinanceiroDto(id, projId, "Título", "Sub", 3000m, agora, StatusParcela.Atrasado, 3, true);
        alerta.EmAtraso.Should().BeTrue();
        alerta.DiasDiferenca.Should().Be(3);
    }
}
