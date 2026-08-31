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
    private readonly ProjetoService _sut;

    public ProjetoServiceTests()
    {
        var ctx                = TestDbContextFactory.Create();
        var repository         = new ProjetoRepository(ctx);
        var templateRepository = new TemplateProjetoRepository(ctx);
        var unitOfWork         = new UnitOfWork(ctx);
        var mapper             = MappingFixture.Create();

        _sut = new ProjetoService(repository, unitOfWork, mapper, null, templateRepository);
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

        resultado.Status.Should().Be(StatusProjeto.Briefing);
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
        var command = new AtualizarStatusProjetoCommand(projeto.Id, StatusProjeto.Desenvolvimento);

        var atualizado = await _sut.UpdateStatus(command);

        atualizado.Status.Should().Be(StatusProjeto.Desenvolvimento);
    }



    [Fact]
    public async Task AtualizarStatus_ComIdInexistente_DeveLancarKeyNotFoundException()
    {
        var command = new AtualizarStatusProjetoCommand(Guid.NewGuid(), StatusProjeto.Concluido);

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
        etapa.Status.Should().Be(StatusEtapa.Pendente);
    }

    [Fact]
    public async Task AtualizarStatusEtapa_ParaConcluida_DeveRegistrarDataConclusao()
    {
        var projeto = await _sut.Create(CriarComando("Apartamento Duplex"));
        var etapa   = await _sut.CreateEtapa(
            new CriarEtapaCommand(projeto.Id, "Estudo Preliminar", "", 1));
        var command = new AtualizarStatusEtapaCommand(etapa.Id, StatusEtapa.Concluida);

        var atualizada = await _sut.UpdateStatusEtapa(command);

        atualizada.Status.Should().Be(StatusEtapa.Concluida);
        atualizada.DataConclusao.Should().NotBeNull();
    }

    [Fact]
    public async Task AtualizarStatusEtapa_ComIdInexistente_DeveLancarKeyNotFoundException()
    {
        var command = new AtualizarStatusEtapaCommand(Guid.NewGuid(), StatusEtapa.Concluida);

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

        await _sut.UpdateStatusEtapa(new AtualizarStatusEtapaCommand(e1.Id, StatusEtapa.Concluida));
        await _sut.UpdateStatusEtapa(new AtualizarStatusEtapaCommand(e2.Id, StatusEtapa.Concluida));
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

        await _sut.UpdateStatusEtapa(new AtualizarStatusEtapaCommand(e1.Id, StatusEtapa.Concluida));
        await _sut.UpdateStatusEtapa(new AtualizarStatusEtapaCommand(e2.Id, StatusEtapa.Concluida));
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

    [Fact]
    public async Task AdicionarTarefa_ComDadosValidos_DevePersistirTarefaNaEtapa()
    {
        var projeto = await _sut.Create(CriarComando("Projeto com Tarefas"));
        var etapa = await _sut.CreateEtapa(new CriarEtapaCommand(projeto.Id, "Etapa 1", "Briefing", 1));

        var tarefa = await _sut.AdicionarTarefa(new AdicionarTarefaCommand(etapa.Id, "Fazer levantamento fotográfico"));

        tarefa.Id.Should().NotBeEmpty();
        tarefa.Titulo.Should().Be("Fazer levantamento fotográfico");
        tarefa.Concluida.Should().BeFalse();

        var projAtualizado = await _sut.GetById(projeto.Id);
        projAtualizado!.Etapas.First().Tarefas.Should().HaveCount(1);
    }

    [Fact]
    public async Task AlternarTarefa_DeveInverterStatusDeConclusao()
    {
        var projeto = await _sut.Create(CriarComando("Projeto Toggle"));
        var etapa = await _sut.CreateEtapa(new CriarEtapaCommand(projeto.Id, "Etapa 1", "Briefing", 1));
        var tarefa = await _sut.AdicionarTarefa(new AdicionarTarefaCommand(etapa.Id, "Item 1"));

        var alternada = await _sut.AlternarTarefa(tarefa.Id);
        alternada.Concluida.Should().BeTrue();

        var alternadaDeNovo = await _sut.AlternarTarefa(tarefa.Id);
        alternadaDeNovo.Concluida.Should().BeFalse();
    }

    [Fact]
    public async Task RemoverTarefa_DeveExcluirTarefaDoBanco()
    {
        var projeto = await _sut.Create(CriarComando("Projeto Remove"));
        var etapa = await _sut.CreateEtapa(new CriarEtapaCommand(projeto.Id, "Etapa 1", "Briefing", 1));
        var tarefa = await _sut.AdicionarTarefa(new AdicionarTarefaCommand(etapa.Id, "Item Para Remover"));

        await _sut.RemoverTarefa(tarefa.Id);

        var projAtualizado = await _sut.GetById(projeto.Id);
        projAtualizado!.Etapas.First().Tarefas.Should().BeEmpty();
    }

    [Fact]
    public async Task AtualizarTemplate_DeveAtualizarDadosEEtapas()
    {
        var templateCriado = await _sut.CriarTemplate(new CriarTemplateProjetoCommand(
            "paisagismo-v1",
            "Paisagismo Inicial",
            "Descricao",
            "yard",
            new List<CriarTemplateEtapaItemCommand>
            {
                new("Estudo", "Desc", 1, new List<string> { "Planta de massas" })
            }
        ));

        var atualizado = await _sut.AtualizarTemplate(new AtualizarTemplateProjetoCommand(
            templateCriado.Id,
            "Paisagismo Completo",
            "Descricao Atualizada",
            "park",
            new List<CriarTemplateEtapaItemCommand>
            {
                new("Estudo", "Desc", 1, new List<string> { "Planta de massas" }),
                new("Executivo", "Plantas e Cortes", 2, new List<string> { "Memorial botânico", "Iluminação" })
            }
        ));

        atualizado.Nome.Should().Be("Paisagismo Completo");
        atualizado.Icone.Should().Be("park");
        atualizado.Etapas.Should().HaveCount(2);
        atualizado.Etapas.Last().Tarefas.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExcluirTemplate_DeveRemoverTemplate()
    {
        var templateCriado = await _sut.CriarTemplate(new CriarTemplateProjetoCommand(
            "temp-delete",
            "Template Exclusao",
            "Descricao",
            "home"
        ));

        await _sut.ExcluirTemplate(templateCriado.Id);

        var obtido = await _sut.ObterTemplatePorId(templateCriado.Id);
        obtido.Should().BeNull();
    }

    [Fact]
    public async Task ObterTemplates_DeveRetornarTodosTemplatesAtivos()
    {
        await _sut.CriarTemplate(new CriarTemplateProjetoCommand("temp-1", "Template 1", "Desc", "home"));
        await _sut.CriarTemplate(new CriarTemplateProjetoCommand("temp-2", "Template 2", "Desc", "chair"));

        var templates = await _sut.ObterTemplates();

        templates.Should().HaveCountGreaterThanOrEqualTo(2);
        templates.Should().Contain(t => t.Codigo == "temp-1");
        templates.Should().Contain(t => t.Codigo == "temp-2");
    }

    [Fact]
    public async Task Update_DeveAtualizarDadosDoProjeto()
    {
        var criado = await _sut.Create(CriarComando("Projeto Original"));

        var atualizado = await _sut.Update(new AtualizarProjetoCommand(
            criado.Id,
            "Projeto Modificado",
            "Nova Descricao",
            TipoProjeto.Comercial,
            StatusProjeto.Desenvolvimento,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(12),
            300
        ));

        atualizado.Nome.Should().Be("Projeto Modificado");
        atualizado.Descricao.Should().Be("Nova Descricao");
        atualizado.Tipo.Should().Be(TipoProjeto.Comercial);
        atualizado.MetragemTotal.Should().Be(300);
    }

    [Fact]
    public async Task RemoverTarefa_DeveRemoverTarefaExistente()
    {
        var proj = await _sut.Create(CriarComando("Projeto com Tarefa"));
        var etapa = await _sut.CreateEtapa(new CriarEtapaCommand(proj.Id, "Etapa Teste", "Desc", 1));
        var tarefa = await _sut.AdicionarTarefa(new AdicionarTarefaCommand(etapa.Id, "Tarefa a ser removida"));

        await _sut.RemoverTarefa(tarefa.Id);

        var projAtualizado = await _sut.GetById(proj.Id);
        projAtualizado!.Etapas.First().Tarefas.Should().BeEmpty();
    }

    private static CriarProjetoCommand CriarComando(string nome) =>
        new(
            Nome:                nome,
            Descricao:           "Descrição de teste",
            Tipo:                TipoProjeto.Residencial,
            DataInicio:          DateTime.UtcNow,
            DataPrevistaEntrega: DateTime.UtcNow.AddMonths(6),
            MetragemTotal:       150,
            ClienteId:           Guid.NewGuid()
        );
}
