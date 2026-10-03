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

    [Fact]
    public async Task ObterPorCategoriaAsync_QuandoJaExisteNoBanco_DeveRetornarDireto()
    {
        var existentes = new List<OpcaoConfiguracao>
        {
            new OpcaoConfiguracao { Id = Guid.NewGuid(), Categoria = "TipoProjeto", Chave = "Comercial", Rotulo = "Comercial" }
        };
        _repositoryMock.Setup(r => r.ObterPorCategoriaAsync("TipoProjeto")).ReturnsAsync(existentes);

        var result = await _service.ObterPorCategoriaAsync("TipoProjeto");

        result.Should().HaveCount(1);
        _repositoryMock.Verify(r => r.Create(It.IsAny<OpcaoConfiguracao>()), Times.Never);
    }

    [Theory]
    [InlineData("FormaPagamento")]
    [InlineData("CanalContatoLead")]
    [InlineData("TipoProjeto")]
    [InlineData("RoleEquipe")]
    [InlineData("Inexistente")]
    public async Task ObterPorCategoriaAsync_TodasCategorias_DevemRetornarDefaults(string categoria)
    {
        _repositoryMock.Setup(r => r.ObterPorCategoriaAsync(categoria)).ReturnsAsync(new List<OpcaoConfiguracao>());

        var result = await _service.ObterPorCategoriaAsync(categoria);

        if (categoria != "Inexistente")
        {
            result.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task ObterTodasAgrupadasAsync_DeveRetornarDicionario()
    {
        var dic = new Dictionary<string, IEnumerable<OpcaoConfiguracao>>
        {
            ["TipoProjeto"] = new[] { new OpcaoConfiguracao { Categoria = "TipoProjeto", Chave = "Residencial", Rotulo = "Residencial" } }
        };
        _repositoryMock.Setup(r => r.ObterTodasAgrupadasAsync()).ReturnsAsync(dic);

        var result = await _service.ObterTodasAgrupadasAsync();

        result.Should().ContainKey("TipoProjeto");
        result["TipoProjeto"].Should().HaveCount(1);
    }

    [Fact]
    public async Task SalvarOpcaoAsync_ParaOpcaoExistente_DeveAtualizarERetornar()
    {
        var id = Guid.NewGuid();
        var existente = new OpcaoConfiguracao { Id = id, Categoria = "TipoProjeto", Chave = "Residencial", Rotulo = "Antigo" };
        _repositoryMock.Setup(r => r.GetById(id)).ReturnsAsync(existente);

        var command = new SalvarOpcaoConfiguracaoCommand
        {
            Id = id,
            Categoria = "TipoProjeto",
            Chave = "Residencial",
            Rotulo = "Novo Rótulo",
            SubRotulo = "Sub",
            Icone = "home",
            Cor = "#fff",
            CorFundo = "#000",
            Ordem = 2,
            DadosExtrasJson = "{}"
        };

        var result = await _service.SalvarOpcaoAsync(command);

        result.Rotulo.Should().Be("Novo Rótulo");
        _repositoryMock.Verify(r => r.Update(existente), Times.Once);
    }
}
