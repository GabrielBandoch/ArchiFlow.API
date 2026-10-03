using ArchiFlow.Application.Fornecedores.Commands;
using ArchiFlow.Application.Fornecedores.Services;
using ArchiFlow.Domain.Fornecedores;
using ArchiFlow.Domain.Shared;
using FluentAssertions;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Fornecedores;

public class FornecedorServiceTests
{
    private readonly Mock<IFornecedorRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FornecedorService _service;

    public FornecedorServiceTests()
    {
        _repositoryMock = new Mock<IFornecedorRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new FornecedorService(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ObterTodosAsync_SemFiltro_DeveRetornarTodosFornecedores()
    {
        var fornecedores = new List<Fornecedor>
        {
            new Fornecedor { Id = Guid.NewGuid(), Nome = "Lumina Iluminação", Especialidade = "Iluminacao", AvaliacaoMedia = 4.9m },
            new Fornecedor { Id = Guid.NewGuid(), Nome = "Joinville Aços", Especialidade = "Estrutural", AvaliacaoMedia = 4.7m }
        };
        _repositoryMock.Setup(r => r.ObterTodosComRelacionamentosAsync()).ReturnsAsync(fornecedores);

        var result = await _service.ObterTodosAsync(null);

        result.Should().HaveCount(2);
        result.First().Nome.Should().Be("Lumina Iluminação");
    }

    [Fact]
    public async Task ObterTodosAsync_ComFiltroEspecialidade_DeveChamarObterPorEspecialidade()
    {
        var fornecedores = new List<Fornecedor>
        {
            new Fornecedor { Id = Guid.NewGuid(), Nome = "Lumina Iluminação", Especialidade = "Iluminacao", AvaliacaoMedia = 4.9m }
        };
        _repositoryMock.Setup(r => r.ObterPorEspecialidadeAsync("Iluminacao")).ReturnsAsync(fornecedores);

        var result = await _service.ObterTodosAsync("Iluminacao");

        result.Should().HaveCount(1);
        _repositoryMock.Verify(r => r.ObterPorEspecialidadeAsync("Iluminacao"), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_ComDadosValidos_DeveCadastrarERetornarDto()
    {
        var command = new CriarFornecedorCommand
        {
            Nome = "Marcenaria Joinville",
            Especialidade = "Marcenaria",
            Email = "contato@marcenaria.com",
            Telefone = "4799999999",
            Cidade = "Joinville",
            Estado = "SC",
            Descricao = "Móveis planejados de alto padrão"
        };

        var result = await _service.CriarAsync(command);

        result.Nome.Should().Be("Marcenaria Joinville");
        result.Especialidade.Should().Be("Marcenaria");
        result.Ativo.Should().BeTrue();
        _repositoryMock.Verify(r => r.Create(It.IsAny<Fornecedor>()), Times.Once);
    }

    [Fact]
    public async Task AdicionarAvaliacaoAsync_ComFornecedorExistente_DeveAdicionarERecalcular()
    {
        var fornecedorId = Guid.NewGuid();
        var fornecedor = new Fornecedor { Id = fornecedorId, Nome = "Pisos Brasil" };
        _repositoryMock.Setup(r => r.GetById(fornecedorId)).ReturnsAsync(fornecedor);

        var command = new AdicionarAvaliacaoCommand
        {
            FornecedorId = fornecedorId,
            Nota = 5,
            Comentario = "Excelente acabamento e pontualidade na entrega.",
            AutorNome = "Arquiteto Titular"
        };

        var result = await _service.AdicionarAvaliacaoAsync(command);

        result.Nota.Should().Be(5);
        result.Comentario.Should().Be("Excelente acabamento e pontualidade na entrega.");
        _repositoryMock.Verify(r => r.AdicionarAvaliacaoAsync(It.IsAny<AvaliacaoFornecedor>()), Times.Once);
    }

    [Fact]
    public async Task VincularProjetoAsync_ComDadosValidos_DeveSalvarVinculo()
    {
        var fornecedorId = Guid.NewGuid();
        var projetoId = Guid.NewGuid();
        var fornecedor = new Fornecedor { Id = fornecedorId, Nome = "Vidraçaria Cristal" };
        _repositoryMock.Setup(r => r.GetById(fornecedorId)).ReturnsAsync(fornecedor);

        var command = new VincularProjetoCommand
        {
            FornecedorId = fornecedorId,
            ProjetoId = projetoId,
            FuncaoNoProjeto = "Fornecimento de Esquadrias e Vidros"
        };

        var result = await _service.VincularProjetoAsync(command);

        result.FornecedorId.Should().Be(fornecedorId);
        result.ProjetoId.Should().Be(projetoId);
        result.FuncaoNoProjeto.Should().Be("Fornecimento de Esquadrias e Vidros");
        _repositoryMock.Verify(r => r.AdicionarVinculoProjetoAsync(It.IsAny<ProjetoFornecedor>()), Times.Once);
    }
}
