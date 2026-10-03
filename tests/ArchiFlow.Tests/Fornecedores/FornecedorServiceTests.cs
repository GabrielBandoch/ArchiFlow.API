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

    [Fact]
    public async Task ObterPorIdAsync_QuandoExiste_DeveRetornarDto()
    {
        var id = Guid.NewGuid();
        var fornecedor = new Fornecedor { Id = id, Nome = "Fornecedor Teste", Especialidade = "Geral" };
        _repositoryMock.Setup(r => r.ObterPorIdComRelacionamentosAsync(id)).ReturnsAsync(fornecedor);

        var result = await _service.ObterPorIdAsync(id);

        result.Should().NotBeNull();
        result!.Nome.Should().Be("Fornecedor Teste");
    }

    [Fact]
    public async Task ObterPorIdAsync_QuandoNaoExiste_DeveRetornarNull()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.ObterPorIdComRelacionamentosAsync(id)).ReturnsAsync((Fornecedor?)null);

        var result = await _service.ObterPorIdAsync(id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AtualizarAsync_QuandoExiste_DeveAtualizarERetornarDto()
    {
        var id = Guid.NewGuid();
        var fornecedor = new Fornecedor { Id = id, Nome = "Antigo", Especialidade = "Antiga" };
        _repositoryMock.Setup(r => r.ObterPorIdComRelacionamentosAsync(id)).ReturnsAsync(fornecedor);

        var cmd = new AtualizarFornecedorCommand
        {
            Id = id,
            Nome = "Novo Nome",
            Especialidade = "Nova Esp",
            Ativo = true
        };

        var result = await _service.AtualizarAsync(cmd);

        result.Should().NotBeNull();
        result.Nome.Should().Be("Novo Nome");
        _repositoryMock.Verify(r => r.Update(fornecedor), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_QuandoNaoExiste_DeveLancarKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.ObterPorIdComRelacionamentosAsync(id)).ReturnsAsync((Fornecedor?)null);

        var cmd = new AtualizarFornecedorCommand { Id = id, Nome = "Novo", Especialidade = "Esp" };
        var act = () => _service.AtualizarAsync(cmd);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ExcluirAsync_QuandoExiste_DeveRemoverERetornarTrue()
    {
        var id = Guid.NewGuid();
        var fornecedor = new Fornecedor { Id = id };
        _repositoryMock.Setup(r => r.GetById(id)).ReturnsAsync(fornecedor);

        var result = await _service.ExcluirAsync(id);

        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.Delete(id), Times.Once);
    }

    [Fact]
    public async Task ExcluirAsync_QuandoNaoExiste_DeveRetornarFalse()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetById(id)).ReturnsAsync((Fornecedor?)null);

        var result = await _service.ExcluirAsync(id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DesvincularProjetoAsync_DeveDelegarParaRepositoryERetornarTrue()
    {
        var vinculoId = Guid.NewGuid();

        var result = await _service.DesvincularProjetoAsync(vinculoId);

        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.RemoverVinculoProjetoAsync(vinculoId), Times.Once);
    }

    [Fact]
    public async Task ObterFornecedoresDoProjetoAsync_DeveRetornarLista()
    {
        var projetoId = Guid.NewGuid();
        var vinculos = new List<ProjetoFornecedor>
        {
            new ProjetoFornecedor { Id = Guid.NewGuid(), ProjetoId = projetoId, Fornecedor = new Fornecedor { Nome = "F1" } }
        };
        _repositoryMock.Setup(r => r.ObterFornecedoresDoProjetoAsync(projetoId)).ReturnsAsync(vinculos);

        var result = await _service.ObterFornecedoresDoProjetoAsync(projetoId);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task AdicionarAvaliacaoAsync_QuandoFornecedorNaoExiste_DeveLancarKeyNotFoundException()
    {
        var cmd = new AdicionarAvaliacaoCommand { FornecedorId = Guid.NewGuid(), Comentario = "Ok" };
        _repositoryMock.Setup(r => r.GetById(cmd.FornecedorId)).ReturnsAsync((Fornecedor?)null);

        var act = () => _service.AdicionarAvaliacaoAsync(cmd);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task VincularProjetoAsync_QuandoFornecedorNaoExiste_DeveLancarKeyNotFoundException()
    {
        var cmd = new VincularProjetoCommand { FornecedorId = Guid.NewGuid(), ProjetoId = Guid.NewGuid() };
        _repositoryMock.Setup(r => r.GetById(cmd.FornecedorId)).ReturnsAsync((Fornecedor?)null);

        var act = () => _service.VincularProjetoAsync(cmd);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
