using ArchiFlow.Application.Honorarios.Factories;
using ArchiFlow.Application.Honorarios.Strategies;
using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Projetos.Enum;
using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace ArchiFlow.Tests.Honorarios;

public class HonorarioStrategiesTests
{
    [Fact]
    public void ResidencialStrategy_Should_Have_Expected_Properties_And_Calculations()
    {
        var strategy = new ResidencialCalculoStrategy();

        strategy.Tipo.Should().Be(TipoProjeto.Residencial);
        strategy.NomeTipologia.Should().Be("Residencial");
        strategy.Multiplicador.Should().Be(1.10m);
        strategy.Descricao.Should().Contain("1.1");

        strategy.CalcularFatorTipologia(10000m).Should().Be(1000m);
        strategy.CalcularHorasEstimadas(100m, 1.0m).Should().Be(100m);
        strategy.AjustarValorEscopo(10000m, 1.0m).Should().Be(10000m);
        strategy.AjustarValorEscopo(10000m, 0.5m).Should().Be(7500m);
    }

    [Fact]
    public void ComercialStrategy_Should_Have_Expected_Properties_And_Calculations()
    {
        var strategy = new ComercialCalculoStrategy();

        strategy.Tipo.Should().Be(TipoProjeto.Comercial);
        strategy.NomeTipologia.Should().Be("Comercial");
        strategy.Multiplicador.Should().Be(1.20m);
        strategy.CalcularFatorTipologia(10000m).Should().Be(2000m);
        strategy.CalcularHorasEstimadas(100m, 1.0m).Should().Be(85m);
    }

    [Fact]
    public void CorporativoStrategy_Should_Have_Expected_Properties_And_Calculations()
    {
        var strategy = new CorporativoCalculoStrategy();

        strategy.Tipo.Should().Be(TipoProjeto.Corporativo);
        strategy.NomeTipologia.Should().Be("Corporativo");
        strategy.Multiplicador.Should().Be(1.30m);
        strategy.CalcularFatorTipologia(10000m).Should().Be(3000m);
        strategy.CalcularHorasEstimadas(100m, 1.0m).Should().Be(90m);
    }

    [Fact]
    public void InterioresStrategy_Should_Have_Expected_Properties_And_Calculations()
    {
        var strategy = new InterioresCalculoStrategy();

        strategy.Tipo.Should().Be(TipoProjeto.Interiores);
        strategy.NomeTipologia.Should().Be("Interiores");
        strategy.Multiplicador.Should().Be(1.15m);
        strategy.CalcularFatorTipologia(10000m).Should().Be(1500m);
        strategy.CalcularHorasEstimadas(100m, 1.0m).Should().Be(88m);
    }

    [Theory]
    [InlineData(PadraoImovel.Economico, 0.80, -2000)]
    [InlineData(PadraoImovel.Medio, 1.00, 0)]
    [InlineData(PadraoImovel.AltoPadrao, 1.30, 3000)]
    [InlineData(PadraoImovel.Luxo, 1.60, 6000)]
    public void PadraoImovelStrategies_Should_Calculate_Factor_Accurately(
        PadraoImovel padrao, 
        decimal multiplicadorEsperado, 
        decimal valorFatorEsperado)
    {
        IPadraoImovelStrategy strategy = padrao switch
        {
            PadraoImovel.Economico => new EconomicoPadraoStrategy(),
            PadraoImovel.Medio => new MedioPadraoStrategy(),
            PadraoImovel.AltoPadrao => new AltoPadraoStrategy(),
            PadraoImovel.Luxo => new LuxoPadraoStrategy(),
            _ => new MedioPadraoStrategy()
        };

        strategy.Padrao.Should().Be(padrao);
        strategy.Multiplicador.Should().Be(multiplicadorEsperado);
        strategy.CalcularFatorPadrao(10000m).Should().Be(valorFatorEsperado);
    }

    [Fact]
    public void HonorarioStrategyFactory_Should_Resolve_Strategies_Correctly()
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

        factory.ObterStrategy(TipoProjeto.Comercial).Should().BeOfType<ComercialCalculoStrategy>();
        factory.ObterStrategy(TipoProjeto.Corporativo).Should().BeOfType<CorporativoCalculoStrategy>();
        factory.ObterStrategy(TipoProjeto.Interiores).Should().BeOfType<InterioresCalculoStrategy>();
        factory.ObterStrategy(TipoProjeto.Residencial).Should().BeOfType<ResidencialCalculoStrategy>();

        factory.ObterPadraoStrategy(PadraoImovel.Economico).Should().BeOfType<EconomicoPadraoStrategy>();
        factory.ObterPadraoStrategy(PadraoImovel.Medio).Should().BeOfType<MedioPadraoStrategy>();
        factory.ObterPadraoStrategy(PadraoImovel.AltoPadrao).Should().BeOfType<AltoPadraoStrategy>();
        factory.ObterPadraoStrategy(PadraoImovel.Luxo).Should().BeOfType<LuxoPadraoStrategy>();
    }
}
