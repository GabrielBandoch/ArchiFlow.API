using System.Collections.Generic;

namespace ArchiFlow.Application.Financeiro.DTOs;

public record PainelFinanceiroDto(
    decimal TotalPrevisto,
    decimal TotalRecebido,
    decimal TotalPendente,
    decimal TotalAtrasado,
    decimal TotalDespesas,
    decimal SaldoLiquido,
    decimal VariacaoPercentualMesAnterior,
    List<ReceitaMesDto> ReceitasPorMes,
    List<AlertaFinanceiroDto> Alertas,
    List<ParcelaFinanceiraDto> ParcelasRecentes
);
