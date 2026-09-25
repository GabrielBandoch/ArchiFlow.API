namespace ArchiFlow.Application.Honorarios.DTOs;

public record MemoriaCalculoDto(
    decimal MetragemQuadrada,
    decimal ValorMetroQuadradoBase,
    decimal ValorBase,
    string FatorPadraoDescricao,
    decimal FatorPadraoMultiplicador,
    decimal ValorFatorPadrao,
    string FatorTipologiaDescricao,
    decimal FatorTipologiaMultiplicador,
    decimal ValorFatorTipologia,
    decimal PercentualEscopoIncluso,
    decimal ValorEscopo,
    decimal HorasEstimadasTotal,
    decimal ValorHoraEstimado,
    decimal CustosDiretos = 1312.92m,
    decimal CustoFixoRateado = 347.64m,
    decimal CustoOperacionalTotal = 0m,
    decimal PercentualImposto = 6.0m,
    decimal ValorImposto = 0m
);

