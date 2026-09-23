using ArchiFlow.Application.Honorarios.DTOs;
using ArchiFlow.Application.Honorarios.Factories;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Projetos.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArchiFlow.Application.Honorarios.Services;

public class CalculadoraHonorariosService : ICalculadoraHonorariosService
{
    private const decimal MetroQuadradoBasePadrao = 95.00m;
    private const decimal ValorHoraBasePadrao = 42.86m;
    private const decimal CustosDiretosPadrao = 1312.92m;
    private const decimal CustoFixoRateadoPadrao = 347.64m;
    private const decimal AliquotaImpostoPadrao = 0.06m;

    private readonly IHonorarioStrategyFactory _strategyFactory;

    public CalculadoraHonorariosService(IHonorarioStrategyFactory strategyFactory)
    {
        _strategyFactory = strategyFactory;
    }

    public SimulacaoResultadoDto Calcular(SimulacaoParametrosDto parametros)
    {
        if (parametros.MetragemQuadrada <= 0)
        {
            throw new ArgumentException("A metragem quadrada deve ser maior que zero.");
        }

        var m2Base = parametros.ValorMetroQuadradoBase.HasValue && parametros.ValorMetroQuadradoBase.Value > 0
            ? parametros.ValorMetroQuadradoBase.Value
            : MetroQuadradoBasePadrao;

        var valorHoraBase = parametros.ValorHoraBase.HasValue && parametros.ValorHoraBase.Value > 0
            ? parametros.ValorHoraBase.Value
            : ValorHoraBasePadrao;

        var metragem = Math.Round(parametros.MetragemQuadrada, 2);

        var etapasDisponiveis = ObterEtapasPadrao(parametros.EtapasInclusas);
        var percentualInclusoTotal = etapasDisponiveis.Where(e => e.Incluso).Sum(e => e.Percentual);

        if (percentualInclusoTotal <= 0)
        {
            percentualInclusoTotal = 100m;
            etapasDisponiveis = etapasDisponiveis.Select(e => e with { Incluso = true }).ToList();
        }

        var fatorEscopo = percentualInclusoTotal / 100.0m;

        var tipologiaStrategy = _strategyFactory.ObterStrategy(parametros.TipoProjeto);
        var horasBase = tipologiaStrategy.CalcularHorasEstimadas(metragem, fatorEscopo);
        var custoProducaoHoras = Math.Round(horasBase * valorHoraBase, 2);

        var padraoStrategy = _strategyFactory.ObterPadraoStrategy(parametros.PadraoImovel);
        var valorFatorPadrao = padraoStrategy.CalcularFatorPadrao(custoProducaoHoras);
        var custoProducaoAjustado = custoProducaoHoras + valorFatorPadrao;

        var valorFatorTipologia = tipologiaStrategy.CalcularFatorTipologia(custoProducaoHoras);

        var custosDiretosAjustados = Math.Round(CustosDiretosPadrao * fatorEscopo, 2);
        var custoFixoRateado = CustoFixoRateadoPadrao;
        var custoOperacionalTotal = Math.Round(custoProducaoAjustado + custosDiretosAjustados + custoFixoRateado, 2);

        var valorImposto = Math.Round(custoOperacionalTotal * AliquotaImpostoPadrao, 2);
        var valorTotalSugerido = Math.Round(custoOperacionalTotal + valorImposto, 2);
        var valorMetroQuadrado = Math.Round(valorTotalSugerido / metragem, 2);

        var etapasCalculadas = new List<ItemEtapaSimulacaoDto>();
        foreach (var etapa in etapasDisponiveis)
        {
            var valorEtapa = etapa.Incluso
                ? Math.Round(valorTotalSugerido * (etapa.Percentual / percentualInclusoTotal), 2)
                : 0m;
            var horasEtapa = etapa.Incluso
                ? Math.Round(horasBase * (etapa.Percentual / percentualInclusoTotal), 0)
                : 0m;

            etapasCalculadas.Add(etapa with
            {
                Valor = valorEtapa,
                HorasEstimadas = horasEtapa
            });
        }

        var memoria = new MemoriaCalculoDto(
            metragem,
            m2Base,
            custoProducaoHoras,
            padraoStrategy.Descricao,
            padraoStrategy.Multiplicador,
            valorFatorPadrao,
            tipologiaStrategy.Descricao,
            tipologiaStrategy.Multiplicador,
            valorFatorTipologia,
            percentualInclusoTotal,
            valorFatorPadrao,
            horasBase,
            valorHoraBase,
            custosDiretosAjustados,
            custoFixoRateado,
            custoOperacionalTotal,
            AliquotaImpostoPadrao * 100m,
            valorImposto
        );

        return new SimulacaoResultadoDto(
            metragem,
            parametros.TipoProjeto,
            tipologiaStrategy.NomeTipologia,
            parametros.PadraoImovel,
            padraoStrategy.NomePadrao,
            valorTotalSugerido,
            valorMetroQuadrado,
            horasBase,
            etapasCalculadas,
            memoria
        );
    }

    private static List<ItemEtapaSimulacaoDto> ObterEtapasPadrao(List<string>? inclusas)
    {
        var todas = new List<ItemEtapaSimulacaoDto>
        {
            new("Estudo Preliminar & Moodboard", "Levantamento, briefing, diagnóstico de necessidades e partido conceitual.", true, 20m, 0m, 0m, 1),
            new("Anteprojeto & Modelagem 3D", "Plantas de layout humanizadas e volumetria tridimensional realista.", true, 25m, 0m, 0m, 2),
            new("Projeto Executivo & Detalhamento", "Caderno técnico com paginações, iluminação, pontos e marcenaria.", true, 35m, 0m, 0m, 3),
            new("Projeto Legal & Aprovação", "Pranchas técnicas para aprovação em condomínio ou prefeitura.", true, 10m, 0m, 0m, 4),
            new("Entrega Técnica, Caderno & Vistoria", "Emissão de ART/RRT, caderno final encadernado e vistoria técnica.", true, 10m, 0m, 0m, 5)
        };

        if (inclusas is { Count: > 0 })
        {
            return todas.Select(e => e with { Incluso = inclusas.Exists(i => string.Equals(i, e.Nome, StringComparison.OrdinalIgnoreCase)) }).ToList();
        }

        return todas;
    }
}
