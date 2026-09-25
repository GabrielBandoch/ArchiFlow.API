using ArchiFlow.Application.Honorarios.Builders;
using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Projetos.Enum;
using FluentAssertions;
using System;
using Xunit;

namespace ArchiFlow.Tests.Honorarios;

public class PropostaHonorarioBuilderTests
{
    [Fact]
    public void Builder_Should_Build_Valid_PropostaHonorario()
    {
        var clienteId = Guid.NewGuid();

        var proposta = PropostaHonorarioBuilder.Criar()
            .ComIdentificacao("Proposta Residencial Moderna", "PROP-2026-0001")
            .ComTipologia(TipoProjeto.Residencial, PadraoImovel.AltoPadrao, 180m)
            .ComTaxasBase(100m, 160m)
            .ComMemoriaCalculo(18000m, 5400m, 1800m, 0m, 25200m, 90m)
            .ComValorFinalAjustado(25000m)
            .ComCliente(clienteId, "Cliente VIP")
            .ComObservacoes("Pagamento em 3x")
            .ComStatus(StatusProposta.Rascunho)
            .AdicionarItemEtapa("Estudo Preliminar", "Briefing e conceito", true, 20m, 5000m, 20m, 1)
            .AdicionarItemEtapa("Anteprojeto", "Plantas e 3D", true, 30m, 7500m, 30m, 2)
            .Build();

        proposta.Should().NotBeNull();
        proposta.Titulo.Should().Be("Proposta Residencial Moderna");
        proposta.Codigo.Should().Be("PROP-2026-0001");
        proposta.TipoProjeto.Should().Be(TipoProjeto.Residencial);
        proposta.PadraoImovel.Should().Be(PadraoImovel.AltoPadrao);
        proposta.MetragemQuadrada.Should().Be(180m);
        proposta.ValorMetroQuadradoBase.Should().Be(100m);
        proposta.ValorHoraBase.Should().Be(160m);
        proposta.ValorTotalSugerido.Should().Be(25200m);
        proposta.ValorFinalAjustado.Should().Be(25000m);
        proposta.ClienteId.Should().Be(clienteId);
        proposta.ClienteNome.Should().Be("Cliente VIP");
        proposta.Observacoes.Should().Be("Pagamento em 3x");
        proposta.Status.Should().Be(StatusProposta.Rascunho);
        proposta.ItensEtapa.Should().HaveCount(2);
    }

    [Fact]
    public void Builder_Should_Throw_When_Titulo_Is_Empty()
    {
        var builder = PropostaHonorarioBuilder.Criar()
            .ComIdentificacao("", "PROP-2026-0001")
            .ComTipologia(TipoProjeto.Residencial, PadraoImovel.Medio, 100m)
            .ComValorFinalAjustado(10000m);

        var act = () => builder.Build();
        act.Should().Throw<ArgumentException>().WithMessage("*título*");
    }

    [Fact]
    public void Builder_Should_Throw_When_Metragem_Is_Zero_Or_Negative()
    {
        var builder = PropostaHonorarioBuilder.Criar()
            .ComIdentificacao("Título Válido", "PROP-2026-0001")
            .ComTipologia(TipoProjeto.Residencial, PadraoImovel.Medio, 0m)
            .ComValorFinalAjustado(10000m);

        var act = () => builder.Build();
        act.Should().Throw<ArgumentException>().WithMessage("*metragem*");
    }

    [Fact]
    public void Builder_Should_Throw_When_ValorFinalAjustado_Is_Zero_Or_Negative()
    {
        var builder = PropostaHonorarioBuilder.Criar()
            .ComIdentificacao("Título Válido", "PROP-2026-0001")
            .ComTipologia(TipoProjeto.Residencial, PadraoImovel.Medio, 100m)
            .ComValorFinalAjustado(0m);

        var act = () => builder.Build();
        act.Should().Throw<ArgumentException>().WithMessage("*valor final*");
    }
}
