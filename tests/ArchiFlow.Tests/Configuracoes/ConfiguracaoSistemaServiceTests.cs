using ArchiFlow.Application.Configuracoes.DTOs;
using ArchiFlow.Application.Configuracoes.Services;
using ArchiFlow.Domain.Shared;
using FluentAssertions;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Configuracoes;

public class ConfiguracaoSistemaServiceTests
{
    private readonly Mock<IOpcaoConfiguracaoRepository> _repositoryMock;
    private readonly ConfiguracaoSistemaService _service;

    public ConfiguracaoSistemaServiceTests()
    {
        _repositoryMock = new Mock<IOpcaoConfiguracaoRepository>();
        _service = new ConfiguracaoSistemaService(_repositoryMock.Object);
    }

    [Fact]
    public async Task ObterPorCategoriaAsync_QuandoNaoExisteNoBanco_DeveCriarDefaultsERetornar()
    {
        _repositoryMock.Setup(r => r.ObterPorCategoriaAsync("TipoCompromisso"))
            .ReturnsAsync(new List<OpcaoConfiguracao>());

        var result = await _service.ObterPorCategoriaAsync("TipoCompromisso");

        result.Should().NotBeEmpty();
        result.Should().Contain(o => o.Chave == "ReuniaoCliente");
        _repositoryMock.Verify(r => r.Create(It.IsAny<OpcaoConfiguracao>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task SalvarOpcaoAsync_ParaNovaOpcao_DeveCadastrarERetornar()
    {
        var command = new SalvarOpcaoConfiguracaoCommand
        {
            Categoria = "TipoProjeto",
            Chave = "Hospitalar",
            Rotulo = "Arquitetura Hospitalar",
            Ordem = 5
        };

        var result = await _service.SalvarOpcaoAsync(command);

        result.Rotulo.Should().Be("Arquitetura Hospitalar");
        result.Chave.Should().Be("Hospitalar");
        _repositoryMock.Verify(r => r.Create(It.IsAny<OpcaoConfiguracao>()), Times.Once);
    }
}
