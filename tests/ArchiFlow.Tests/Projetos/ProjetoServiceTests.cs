using ArchiFlow.Application.Projetos.Commands;
using ArchiFlow.Application.Projetos.Services;
using ArchiFlow.Domain.Projetos.Enum;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Tests.Common;
using FluentAssertions;
using Xunit;

namespace ArchiFlow.Tests.Projetos;

public class ProjetoServiceTests
{
    private readonly ProjetoService _sut;

    public ProjetoServiceTests()
    {
        var ctx = TestDbContextFactory.Create();
        var mapper = MappingFixture.Create();
        _sut = new ProjetoService(ctx, mapper);
    }

    [Fact]
    public async Task ObterTodosSemProjetos()
    {
        var resultado = await _sut.ObterTodos();

        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ObterTodosComProjetos()
    {
        await _sut.Criar(CriarComando("Projeto A"));
        await _sut.Criar(CriarComando("Projeto B"));

        var resultado = await _sut.ObterTodos();

        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task CriarProjeto()
    {
        var command = CriarComando("Residência Silva");

        var resultado = await _sut.Criar(command);

        resultado.Id.Should().NotBeEmpty();
        resultado.Nome.Should().Be("Residência Silva");
    }

    [Fact]
    public async Task CriarIniciaBriefing()
    {
        var command = CriarComando("Casa Moderna");

        var resultado = await _sut.Criar(command);

        resultado.Status.Should().Be(StatusProjetoEnum.Briefing);
    }

    [Fact]
    public async Task CriarIniciaProgressoZero()
    {
        var command = CriarComando("Escritório Corporativo");

        var resultado = await _sut.Criar(command);

        resultado.ProgressoPercentual.Should().Be(0);
    }

    [Fact]
    public async Task CriarIdsDiferentes()
    {
        var p1 = await _sut.Criar(CriarComando("Projeto 1"));
        var p2 = await _sut.Criar(CriarComando("Projeto 2"));

        p1.Id.Should().NotBe(p2.Id);
    }

    [Fact]
    public async Task ObterPorIdExistente()
    {
        var criado = await _sut.Criar(CriarComando("Loja Centro"));

        var resultado = await _sut.ObterPorId(criado.Id);

        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(criado.Id);
        resultado.Nome.Should().Be("Loja Centro");
    }

    [Fact]
    public async Task ObterPorIdInexistente()
    {
        var resultado = await _sut.ObterPorId(Guid.NewGuid());

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task AtualizarStatus()
    {
        var projeto = await _sut.Criar(CriarComando("Casa Duplex"));
        var command = new AtualizarStatusProjetoCommand(projeto.Id, StatusProjetoEnum.Desenvolvimento);

        var atualizado = await _sut.AtualizarStatus(command);

        atualizado.Status.Should().Be(StatusProjetoEnum.Desenvolvimento);
    }

    [Fact]
    public async Task AtualizarStatusInexistente()
    {
        var command = new AtualizarStatusProjetoCommand(Guid.NewGuid(), StatusProjetoEnum.Concluido);

        var act = async () => await _sut.AtualizarStatus(command);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CriarEtapa()
    {
        var projeto = await _sut.Criar(CriarComando("Interiores Premium"));
        var command = new CriarEtapaCommand(projeto.Id, "Briefing", "Levantamento inicial", 1);

        var etapa = await _sut.CriarEtapa(command);

        etapa.Id.Should().NotBeEmpty();
        etapa.ProjetoId.Should().Be(projeto.Id);
        etapa.Nome.Should().Be("Briefing");
        etapa.Status.Should().Be(StatusEtapaEnum.Pendente);
    }

    [Fact]
    public async Task ConcluirEtapa()
    {
        var projeto = await _sut.Criar(CriarComando("Apartamento Duplex"));
        var etapa = await _sut.CriarEtapa(
            new CriarEtapaCommand(projeto.Id, "Estudo Preliminar", "", 1));

        var command = new AtualizarStatusEtapaCommand(etapa.Id, StatusEtapaEnum.Concluida);

        var atualizada = await _sut.AtualizarStatusEtapa(command);

        atualizada.Status.Should().Be(StatusEtapaEnum.Concluida);
        atualizada.DataConclusao.Should().NotBeNull();
    }

    [Fact]
    public async Task AtualizarEtapaInexistente()
    {
        var command = new AtualizarStatusEtapaCommand(Guid.NewGuid(), StatusEtapaEnum.Concluida);

        var act = async () => await _sut.AtualizarStatusEtapa(command);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ProgressoParcial()
    {
        var projeto = await _sut.Criar(CriarComando("Residência 4 Etapas"));

        var e1 = await _sut.CriarEtapa(new CriarEtapaCommand(projeto.Id, "E1", "", 1));
        var e2 = await _sut.CriarEtapa(new CriarEtapaCommand(projeto.Id, "E2", "", 2));
        await _sut.CriarEtapa(new CriarEtapaCommand(projeto.Id, "E3", "", 3));
        await _sut.CriarEtapa(new CriarEtapaCommand(projeto.Id, "E4", "", 4));

        await _sut.AtualizarStatusEtapa(new AtualizarStatusEtapaCommand(e1.Id, StatusEtapaEnum.Concluida));
        await _sut.AtualizarStatusEtapa(new AtualizarStatusEtapaCommand(e2.Id, StatusEtapaEnum.Concluida));

        var projetoAtualizado = await _sut.ObterPorId(projeto.Id);

        projetoAtualizado!.ProgressoPercentual.Should().Be(50);
    }

    [Fact]
    public async Task ProgressoSemEtapas()
    {
        var projeto = await _sut.Criar(CriarComando("Projeto Sem Etapas"));

        var resultado = await _sut.ObterPorId(projeto.Id);

        resultado!.ProgressoPercentual.Should().Be(0);
    }

    [Fact]
    public async Task ProgressoCompleto()
    {
        var projeto = await _sut.Criar(CriarComando("Projeto Finalizado"));

        var e1 = await _sut.CriarEtapa(new CriarEtapaCommand(projeto.Id, "E1", "", 1));
        var e2 = await _sut.CriarEtapa(new CriarEtapaCommand(projeto.Id, "E2", "", 2));

        await _sut.AtualizarStatusEtapa(new AtualizarStatusEtapaCommand(e1.Id, StatusEtapaEnum.Concluida));
        await _sut.AtualizarStatusEtapa(new AtualizarStatusEtapaCommand(e2.Id, StatusEtapaEnum.Concluida));

        var resultado = await _sut.ObterPorId(projeto.Id);

        resultado!.ProgressoPercentual.Should().Be(100);
    }

    [Fact]
    public async Task ExcluirProjeto()
    {
        var projeto = await _sut.Criar(CriarComando("Para Deletar"));

        await _sut.Excluir(projeto.Id);
        var resultado = await _sut.ObterPorId(projeto.Id);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ExcluirInexistente()
    {
        var act = async () => await _sut.Excluir(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    private static CriarProjetoCommand CriarComando(string nome) =>
        new(
            Nome: nome,
            Descricao: "Descrição de teste",
            Tipo: TipoProjetoEnum.Residencial,
            DataInicio: DateTime.UtcNow,
            DataPrevistaEntrega: DateTime.UtcNow.AddMonths(6),
            MetragemTotal: 150,
            ClienteId: Guid.NewGuid()
        );
}