using ArchiFlow.Application.Projetos.Commands;
using ArchiFlow.Application.Projetos.Services;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Projetos.Enum;
using ArchiFlow.Infrastructure.Repositories;
using ArchiFlow.Infrastructure.Repositories.Projetos;
using ArchiFlow.Tests.Common;
using FluentAssertions;
using Xunit;
using ArchiFlow.Application.Interfaces.Services;

namespace ArchiFlow.Tests.Projetos;

public class ProjetoServiceTests
{
    private readonly IProjetoService _sut;

    public ProjetoServiceTests()
    {
        var ctx        = TestDbContextFactory.Create();
        var repository = new ProjetoRepository(ctx);
        var unitOfWork = new UnitOfWork(ctx);
        var mapper     = MappingFixture.Create();

        _sut = new ProjetoService(repository, unitOfWork, mapper);
    }

    [Fact]
    public async Task ObterTodos_QuandoNaoHaProjetos_DeveRetornarListaVazia()
    {
        var resultado = await _sut.GetAll();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ObterTodos_QuandoExistemDoisProjetos_DeveRetornarAmbos()
    {
        await _sut.Create(CriarComando("Projeto A"));
        await _sut.Create(CriarComando("Projeto B"));

        var resultado = await _sut.GetAll();
        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task Criar_ComDadosValidos_DeveRetornarProjetoComIdGerado()
    {
        var command = CriarComando("Residência Silva");

        var resultado = await _sut.Create(command);

        resultado.Id.Should().NotBeEmpty();
        resultado.Nome.Should().Be("Residência Silva");
    }

    [Fact]
    public async Task Criar_DeveIniciarComStatusBriefing()
    {
        var command = CriarComando("Casa Moderna");

        var resultado = await _sut.Create(command);

        resultado.Status.Should().Be(StatusProjetoEnum.Briefing);
    }

    [Fact]
    public async Task Criar_DeveIniciarComProgressoZero()
    {
        var command = CriarComando("Escritório Corporativo");

        var resultado = await _sut.Create(command);

        resultado.ProgressoPercentual.Should().Be(0);
    }

    [Fact]
    public async Task Criar_DoisProjetos_DevemTerIdsDistintos()
    {
        var p1 = await _sut.Create(CriarComando("Projeto 1"));
        var p2 = await _sut.Create(CriarComando("Projeto 2"));

        p1.Id.Should().NotBe(p2.Id);
    }

    [Fact]
    public async Task ObterPorId_ComIdExistente_DeveRetornarProjetoCorreto()
    {
        var criado = await _sut.Create(CriarComando("Loja Centro"));

        var resultado = await _sut.GetById(criado.Id);

        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(criado.Id);
        resultado.Nome.Should().Be("Loja Centro");
    }

    [Fact]
    public async Task ObterPorId_ComIdInexistente_DeveRetornarNull()
    {
        var resultado = await _sut.GetById(Guid.NewGuid());
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task UpdateStatus_ComStatusValido_DeveAlterarCorretamente()
    {
        var projeto = await _sut.Create(CriarComando("Casa Duplex"));
        var command = new AtualizarStatusProjetoCommand(projeto.Id, StatusProjetoEnum.Desenvolvimento);

        var atualizado = await _sut.UpdateStatus(command);

        atualizado.Status.Should().Be(StatusProjetoEnum.Desenvolvimento);
    }

    [Fact]
    public async Task AtualizarStatus_ComIdInexistente_DeveLancarKeyNotFoundException()
    {
        var command = new AtualizarStatusProjetoCommand(Guid.NewGuid(), StatusProjetoEnum.Concluido);

        var act = async () => await _sut.UpdateStatus(command);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CriarEtapa_DeveAdicionarEtapaComStatusPendente()
    {
        var projeto = await _sut.Create(CriarComando("Interiores Premium"));
        var command = new CriarEtapaCommand(projeto.Id, "Briefing", "Levantamento inicial", 1);

        var etapa = await _sut.CreateEtapa(command);

        etapa.Id.Should().NotBeEmpty();
        etapa.ProjetoId.Should().Be(projeto.Id);
        etapa.Nome.Should().Be("Briefing");
        etapa.Status.Should().Be(StatusEtapaEnum.Pendente);
    }

    [Fact]
    public async Task AtualizarStatusEtapa_ParaConcluida_DeveRegistrarDataConclusao()
    {
        var projeto = await _sut.Create(CriarComando("Apartamento Duplex"));
        var etapa   = await _sut.CreateEtapa(
            new CriarEtapaCommand(projeto.Id, "Estudo Preliminar", "", 1));
        var command = new AtualizarStatusEtapaCommand(etapa.Id, StatusEtapaEnum.Concluida);

        var atualizada = await _sut.UpdateStatusEtapa(command);

        atualizada.Status.Should().Be(StatusEtapaEnum.Concluida);
        atualizada.DataConclusao.Should().NotBeNull();
    }

    [Fact]
    public async Task AtualizarStatusEtapa_ComIdInexistente_DeveLancarKeyNotFoundException()
    {
        var command = new AtualizarStatusEtapaCommand(Guid.NewGuid(), StatusEtapaEnum.Concluida);

        var act = async () => await _sut.UpdateStatusEtapa(command);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Progresso_ComDuasEtapasConcluidasDeQuatro_DeveRetornar50Porcento()
    {
        var projeto = await _sut.Create(CriarComando("Residência 4 Etapas"));
        var e1 = await _sut.CreateEtapa(new CriarEtapaCommand(projeto.Id, "E1", "", 1));
        var e2 = await _sut.CreateEtapa(new CriarEtapaCommand(projeto.Id, "E2", "", 2));
        await _sut.CreateEtapa(new CriarEtapaCommand(projeto.Id, "E3", "", 3));
        await _sut.CreateEtapa(new CriarEtapaCommand(projeto.Id, "E4", "", 4));

        await _sut.UpdateStatusEtapa(new AtualizarStatusEtapaCommand(e1.Id, StatusEtapaEnum.Concluida));
        await _sut.UpdateStatusEtapa(new AtualizarStatusEtapaCommand(e2.Id, StatusEtapaEnum.Concluida));
        var projetoAtualizado = await _sut.GetById(projeto.Id);

        projetoAtualizado!.ProgressoPercentual.Should().Be(50);
    }

    [Fact]
    public async Task Progresso_SemEtapas_DeveRetornarZero()
    {
        var projeto = await _sut.Create(CriarComando("Projeto Sem Etapas"));

        var resultado = await _sut.GetById(projeto.Id);

        resultado!.ProgressoPercentual.Should().Be(0);
    }

    [Fact]
    public async Task Progresso_ComTodasEtapasConcluidas_DeveRetornar100Porcento()
    {
        var projeto = await _sut.Create(CriarComando("Projeto Finalizado"));
        var e1 = await _sut.CreateEtapa(new CriarEtapaCommand(projeto.Id, "E1", "", 1));
        var e2 = await _sut.CreateEtapa(new CriarEtapaCommand(projeto.Id, "E2", "", 2));

        await _sut.UpdateStatusEtapa(new AtualizarStatusEtapaCommand(e1.Id, StatusEtapaEnum.Concluida));
        await _sut.UpdateStatusEtapa(new AtualizarStatusEtapaCommand(e2.Id, StatusEtapaEnum.Concluida));
        var resultado = await _sut.GetById(projeto.Id);

        resultado!.ProgressoPercentual.Should().Be(100);
    }

    [Fact]
    public async Task Excluir_ComIdExistente_DeveRemoverProjeto()
    {
        var projeto = await _sut.Create(CriarComando("Para Deletar"));

        await _sut.Delete(projeto.Id);
        var resultado = await _sut.GetById(projeto.Id);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Excluir_ComIdInexistente_DeveLancarKeyNotFoundException()
    {
        var act = async () => await _sut.Delete(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    private static CriarProjetoCommand CriarComando(string nome) =>
        new(
            Nome:                nome,
            Descricao:           "Descrição de teste",
            Tipo:                TipoProjetoEnum.Residencial,
            DataInicio:          DateTime.UtcNow,
            DataPrevistaEntrega: DateTime.UtcNow.AddMonths(6),
            MetragemTotal:       150,
            ClienteId:           Guid.NewGuid()
        );
}
