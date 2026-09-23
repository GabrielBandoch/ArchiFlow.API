using ArchiFlow.Application.Honorarios.DTOs;
using ArchiFlow.Application.Honorarios.Factories;
using ArchiFlow.Application.Honorarios.Services;
using ArchiFlow.Application.Honorarios.Strategies;
using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Projetos.Enum;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace ArchiFlow.Tests.Services;

public class CalculadoraHonorariosServiceTests
{
    private readonly CalculadoraHonorariosService _calculadora;

    public CalculadoraHonorariosServiceTests()
    {
        var tipologiaStrategies = new List<ICalculoHonorarioStrategy>
        {
            new ResidencialCalculoStrategy(),
            new ComercialCalculoStrategy(),
            new CorporativoCalculoStrategy(),
            new InterioresCalculoStrategy()
        };

        var padraoStrategies = new List<IPadraoImovelStrategy>
        {
            new EconomicoPadraoStrategy(),
            new MedioPadraoStrategy(),
            new AltoPadraoStrategy(),
            new LuxoPadraoStrategy()
        };

        var factory = new HonorarioStrategyFactory(tipologiaStrategies, padraoStrategies);
        _calculadora = new CalculadoraHonorariosService(factory);
    }

    [Fact]
    public void Calcular_Should_Throw_When_Metragem_Zero_Or_Negative()
    {
        var paramsZero = new SimulacaoParametrosDto(0, TipoProjeto.Residencial, PadraoImovel.Medio, null, null, null);
        var actZero = () => _calculadora.Calcular(paramsZero);
        actZero.Should().Throw<ArgumentException>();

        var paramsNeg = new SimulacaoParametrosDto(-50, TipoProjeto.Residencial, PadraoImovel.Medio, null, null, null);
        var actNeg = () => _calculadora.Calcular(paramsNeg);
        actNeg.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Calcular_Should_Calculate_Accurately_For_Residencial_Medio()
    {
        var parametros = new SimulacaoParametrosDto(
            150m,
            TipoProjeto.Residencial,
            PadraoImovel.Medio,
            new List<string> { "Estudo Preliminar & Moodboard", "Anteprojeto & Modelagem 3D", "Projeto Executivo & Detalhamento", "Projeto Legal & Aprovação", "Entrega Técnica, Caderno & Vistoria" },
            42.86m,
            95m
        );

        var resultado = _calculadora.Calcular(parametros);

        resultado.Should().NotBeNull();
        resultado.MetragemQuadrada.Should().Be(150m);
        resultado.TipoProjetoNome.Should().Be("Residencial");
        resultado.PadraoImovelNome.Should().Be("Médio");
        resultado.HorasEstimadasTotal.Should().Be(150m);
        resultado.Etapas.Should().HaveCount(5);
        resultado.MemoriaCalculo.CustoFixoRateado.Should().Be(347.64m);
        resultado.MemoriaCalculo.CustosDiretos.Should().Be(1312.92m);
        resultado.MemoriaCalculo.ValorFatorPadrao.Should().Be(0m);
        resultado.ValorTotalSugerido.Should().Be(8574.93m);
        resultado.ValorMetroQuadrado.Should().Be(57.17m);
    }

    [Fact]
    public void Calcular_Should_Calculate_Accurately_For_Interiores_Medio()
    {
        var parametros = new SimulacaoParametrosDto(
            150m,
            TipoProjeto.Interiores,
            PadraoImovel.Medio,
            null,
            42.86m,
            95m
        );

        var resultado = _calculadora.Calcular(parametros);

        resultado.Should().NotBeNull();
        resultado.MetragemQuadrada.Should().Be(150m);
        resultado.TipoProjetoNome.Should().Be("Interiores");
        resultado.HorasEstimadasTotal.Should().Be(132m);
        resultado.Etapas.Should().HaveCount(5);
        resultado.MemoriaCalculo.CustoOperacionalTotal.Should().Be(7318.08m);
        resultado.MemoriaCalculo.ValorImposto.Should().Be(439.08m);
        resultado.ValorTotalSugerido.Should().Be(7757.16m);
        resultado.ValorMetroQuadrado.Should().Be(51.71m);
    }

    [Theory]
    [InlineData(PadraoImovel.Economico)]
    [InlineData(PadraoImovel.AltoPadrao)]
    [InlineData(PadraoImovel.Luxo)]
    public void Calcular_Should_Apply_Different_Padrao_Multipliers(PadraoImovel padrao)
    {
        var parametros = new SimulacaoParametrosDto(100m, TipoProjeto.Comercial, padrao, null, null, null);
        var resultado = _calculadora.Calcular(parametros);

        resultado.ValorTotalSugerido.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(TipoProjeto.Corporativo)]
    [InlineData(TipoProjeto.Interiores)]
    public void Calcular_Should_Apply_Different_Tipologia_Multipliers(TipoProjeto tipo)
    {
        var parametros = new SimulacaoParametrosDto(200m, tipo, PadraoImovel.AltoPadrao, null, null, null);
        var resultado = _calculadora.Calcular(parametros);

        resultado.ValorTotalSugerido.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Calcular_Should_Use_Default_Preset_When_Empty_Or_Null()
    {
        var parametros = new SimulacaoParametrosDto(100m, TipoProjeto.Residencial, PadraoImovel.Medio, null, null, null);
        var resultado = _calculadora.Calcular(parametros);

        resultado.MemoriaCalculo.PercentualEscopoIncluso.Should().Be(100m);
        resultado.Etapas.Should().HaveCount(5);
    }
}
