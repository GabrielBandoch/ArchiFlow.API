using ArchiFlow.Application.Fornecedores.Commands;
using ArchiFlow.Application.Fornecedores.Services;
using ArchiFlow.Domain.Fornecedores;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Projetos.Enum;
using ArchiFlow.Domain.Shared;
using FluentAssertions;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Fornecedores;

public class FornecedorServiceTests
{
    private readonly Mock<IFornecedorRepository> _repositoryMock;
    private readonly Mock<IProjetoRepository> _projetoRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FornecedorService _service;

    public FornecedorServiceTests()
    {
        _repositoryMock = new Mock<IFornecedorRepository>();
        _projetoRepositoryMock = new Mock<IProjetoRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new FornecedorService(_repositoryMock.Object, _projetoRepositoryMock.Object, _unitOfWorkMock.Object);
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
    public async Task CriarAsync_ComDadosValidos_DeveCadastrarComAvaliacaoZeroEComitar()
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
        result.AvaliacaoMedia.Should().Be(0.0m);
        result.TotalAvaliacoes.Should().Be(0);
        result.Ativo.Should().BeTrue();
        _repositoryMock.Verify(r => r.Create(It.Is<Fornecedor>(f => f.AvaliacaoMedia == 0.0m && f.TotalAvaliacoes == 0)), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AdicionarAvaliacaoAsync_ComFornecedorExistenteSemProjeto_DeveAdicionarERecalcularEComitar()
    {
        var fornecedorId = Guid.NewGuid();
        var fornecedor = new Fornecedor { Id = fornecedorId, Nome = "Pisos Brasil" };
        _repositoryMock.Setup(r => r.ObterPorIdComRelacionamentosAsync(fornecedorId)).ReturnsAsync(fornecedor);

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
        fornecedor.AvaliacaoMedia.Should().Be(5.0m);
        fornecedor.TotalAvaliacoes.Should().Be(1);
        _repositoryMock.Verify(r => r.AdicionarAvaliacaoAsync(It.IsAny<AvaliacaoFornecedor>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AdicionarAvaliacaoAsync_ComProjetoValido_DeveValidarProjetoEComitar()
    {
        var fornecedorId = Guid.NewGuid();
        var projetoId = Guid.NewGuid();
        var fornecedor = new Fornecedor { Id = fornecedorId, Nome = "Pisos Brasil" };
        var projeto = new Projeto { Id = projetoId, Nome = "Reforma Comercial" };

        _repositoryMock.Setup(r => r.ObterPorIdComRelacionamentosAsync(fornecedorId)).ReturnsAsync(fornecedor);
        _projetoRepositoryMock.Setup(p => p.GetById(projetoId)).ReturnsAsync(projeto);

        var command = new AdicionarAvaliacaoCommand
        {
            FornecedorId = fornecedorId,
            ProjetoId = projetoId,
            Nota = 4,
            Comentario = "Bom serviço",
            AutorNome = "Engenheiro"
        };

        var result = await _service.AdicionarAvaliacaoAsync(command);

        result.Nota.Should().Be(4);
        result.ProjetoId.Should().Be(projetoId);
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AdicionarAvaliacaoAsync_ComProjetoInexistente_DeveLancarKeyNotFoundException()
    {
        var fornecedorId = Guid.NewGuid();
        var projetoId = Guid.NewGuid();
        var fornecedor = new Fornecedor { Id = fornecedorId, Nome = "Pisos Brasil" };

        _repositoryMock.Setup(r => r.ObterPorIdComRelacionamentosAsync(fornecedorId)).ReturnsAsync(fornecedor);
        _projetoRepositoryMock.Setup(p => p.GetById(projetoId)).ReturnsAsync((Projeto?)null);

        var command = new AdicionarAvaliacaoCommand
        {
            FornecedorId = fornecedorId,
            ProjetoId = projetoId,
            Nota = 4,
            Comentario = "Bom serviço"
        };

        var act = () => _service.AdicionarAvaliacaoAsync(command);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{projetoId}*");
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AdicionarAvaliacaoAsync_QuandoFornecedorNaoExiste_DeveLancarKeyNotFoundException()
    {
        var cmd = new AdicionarAvaliacaoCommand { FornecedorId = Guid.NewGuid(), Comentario = "Ok" };
        _repositoryMock.Setup(r => r.ObterPorIdComRelacionamentosAsync(cmd.FornecedorId)).ReturnsAsync((Fornecedor?)null);

        var act = () => _service.AdicionarAvaliacaoAsync(cmd);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VincularProjetoAsync_ComDadosValidos_DeveSalvarVinculoComProjetoNomeEComitar()
    {
        var fornecedorId = Guid.NewGuid();
        var projetoId = Guid.NewGuid();
        var fornecedor = new Fornecedor { Id = fornecedorId, Nome = "Vidraçaria Cristal" };
        var projeto = new Projeto { Id = projetoId, Nome = "Residência Alphaville" };

        _repositoryMock.Setup(r => r.GetById(fornecedorId)).ReturnsAsync(fornecedor);
        _projetoRepositoryMock.Setup(p => p.GetById(projetoId)).ReturnsAsync(projeto);

        var command = new VincularProjetoCommand
        {
            FornecedorId = fornecedorId,
            ProjetoId = projetoId,
            FuncaoNoProjeto = "Fornecimento de Esquadrias e Vidros"
        };

        var result = await _service.VincularProjetoAsync(command);

        result.FornecedorId.Should().Be(fornecedorId);
        result.ProjetoId.Should().Be(projetoId);
        result.FornecedorNome.Should().Be("Vidraçaria Cristal");
        result.ProjetoNome.Should().Be("Residência Alphaville");
        result.FuncaoNoProjeto.Should().Be("Fornecimento de Esquadrias e Vidros");
        _repositoryMock.Verify(r => r.AdicionarVinculoProjetoAsync(It.IsAny<ProjetoFornecedor>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VincularProjetoAsync_QuandoFornecedorNaoExiste_DeveLancarKeyNotFoundException()
    {
        var cmd = new VincularProjetoCommand { FornecedorId = Guid.NewGuid(), ProjetoId = Guid.NewGuid() };
        _repositoryMock.Setup(r => r.GetById(cmd.FornecedorId)).ReturnsAsync((Fornecedor?)null);

        var act = () => _service.VincularProjetoAsync(cmd);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VincularProjetoAsync_QuandoProjetoNaoExiste_DeveLancarKeyNotFoundException()
    {
        var fornecedorId = Guid.NewGuid();
        var projetoId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetById(fornecedorId)).ReturnsAsync(new Fornecedor { Id = fornecedorId });
        _projetoRepositoryMock.Setup(p => p.GetById(projetoId)).ReturnsAsync((Projeto?)null);

        var cmd = new VincularProjetoCommand { FornecedorId = fornecedorId, ProjetoId = projetoId };
        var act = () => _service.VincularProjetoAsync(cmd);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{projetoId}*");
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DesvincularProjetoAsync_QuandoVinculoExiste_DeveRemoverEComitar()
    {
        var vinculoId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.RemoverVinculoProjetoAsync(vinculoId)).ReturnsAsync(true);

        var result = await _service.DesvincularProjetoAsync(vinculoId);

        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.RemoverVinculoProjetoAsync(vinculoId), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DesvincularProjetoAsync_QuandoVinculoNaoExiste_DeveRetornarFalseENaoComitar()
    {
        var vinculoId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.RemoverVinculoProjetoAsync(vinculoId)).ReturnsAsync(false);

        var result = await _service.DesvincularProjetoAsync(vinculoId);

        result.Should().BeFalse();
        _repositoryMock.Verify(r => r.RemoverVinculoProjetoAsync(vinculoId), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObterFornecedoresDoProjetoAsync_QuandoProjetoNaoExiste_DeveLancarKeyNotFoundException()
    {
        var projetoId = Guid.NewGuid();
        _projetoRepositoryMock.Setup(p => p.GetById(projetoId)).ReturnsAsync((Projeto?)null);

        var act = () => _service.ObterFornecedoresDoProjetoAsync(projetoId);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{projetoId}*");
    }

    [Fact]
    public async Task ObterFornecedoresDoProjetoAsync_QuandoProjetoExiste_DeveRetornarListaComNomes()
    {
        var projetoId = Guid.NewGuid();
        var projeto = new Projeto { Id = projetoId, Nome = "Edifício Horizonte" };
        var vinculos = new List<ProjetoFornecedor>
        {
            new ProjetoFornecedor
            {
                Id = Guid.NewGuid(),
                ProjetoId = projetoId,
                Fornecedor = new Fornecedor { Nome = "Fornecedor Alpha" },
                FuncaoNoProjeto = "Estrutura"
            }
        };

        _projetoRepositoryMock.Setup(p => p.GetById(projetoId)).ReturnsAsync(projeto);
        _repositoryMock.Setup(r => r.ObterFornecedoresDoProjetoAsync(projetoId)).ReturnsAsync(vinculos);

        var result = await _service.ObterFornecedoresDoProjetoAsync(projetoId);

        result.Should().HaveCount(1);
        result.First().FornecedorNome.Should().Be("Fornecedor Alpha");
        result.First().ProjetoNome.Should().Be("Edifício Horizonte");
    }

    [Fact]
    public async Task ObterPorIdAsync_QuandoExiste_DeveRetornarDtoComProjetosAtivosCalculados()
    {
        var id = Guid.NewGuid();
        var fornecedor = new Fornecedor
        {
            Id = id,
            Nome = "Fornecedor Completo",
            Especialidade = "Marcenaria",
            AvaliacaoMedia = 4.5m,
            TotalAvaliacoes = 2,
            ProjetosVinculados = new List<ProjetoFornecedor>
            {
                new() { ProjetoId = Guid.NewGuid(), Projeto = new Projeto { Nome = "Proj Ativo 1", Status = StatusProjeto.Desenvolvimento } },
                new() { ProjetoId = Guid.NewGuid(), Projeto = new Projeto { Nome = "Proj Ativo 2", Status = StatusProjeto.Execucao } },
                new() { ProjetoId = Guid.NewGuid(), Projeto = new Projeto { Nome = "Proj Concluido", Status = StatusProjeto.Concluido } },
                new() { ProjetoId = Guid.NewGuid(), Projeto = new Projeto { Nome = "Proj Cancelado", Status = StatusProjeto.Cancelado } }
            }
        };
        _repositoryMock.Setup(r => r.ObterPorIdComRelacionamentosAsync(id)).ReturnsAsync(fornecedor);

        var result = await _service.ObterPorIdAsync(id);

        result.Should().NotBeNull();
        result!.Nome.Should().Be("Fornecedor Completo");
        result.TotalProjetosAtivos.Should().Be(2);
        result.ProjetosVinculados.Should().HaveCount(4);
        result.ProjetosVinculados.First().ProjetoNome.Should().Be("Proj Ativo 1");
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
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
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
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExcluirAsync_QuandoNaoExiste_DeveRetornarFalse()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetById(id)).ReturnsAsync((Fornecedor?)null);

        var result = await _service.ExcluirAsync(id);

        result.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void RecalcularMedia_ComAvaliacoesNulasOuVazias_DeveDefinirMediaZero()
    {
        var fornecedor = new Fornecedor
        {
            AvaliacaoMedia = 5.0m,
            TotalAvaliacoes = 3,
            Avaliacoes = new List<AvaliacaoFornecedor>()
        };

        fornecedor.RecalcularMedia();

        fornecedor.AvaliacaoMedia.Should().Be(0.0m);
        fornecedor.TotalAvaliacoes.Should().Be(0);
    }
}
