using ArchiFlow.Application.Leads.Commands;
using ArchiFlow.Application.Leads.Services;
using ArchiFlow.Domain.Leads;
using ArchiFlow.Domain.Leads.Enum;
using ArchiFlow.Infrastructure.Repositories;
using ArchiFlow.Infrastructure.Repositories.Leads;
using ArchiFlow.Infrastructure.Data;
using ArchiFlow.Tests.Common;
using FluentAssertions;
using Xunit;
using ArchiFlow.Application.Interfaces.Services;

namespace ArchiFlow.Tests.Leads;

public class LeadServiceTests
{
    private readonly ArchiFlowDbContext _ctx;
    private readonly LeadService _sut;

    public LeadServiceTests()
    {
        _ctx           = TestDbContextFactory.Create();
        var repository = new LeadRepository(_ctx);
        var unitOfWork = new UnitOfWork(_ctx);
        var mapper     = MappingFixture.Create();

        _sut = new LeadService(repository, unitOfWork, mapper);
    }

    [Fact]
    public async Task ObterTodos_QuandoNaoHaLeads_DeveRetornarListaVazia()
    {
        var resultado = await _sut.GetAll();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ObterTodos_QuandoExistemDoisLeads_DeveRetornarAmbos()
    {
        await _sut.Create(CriarComando("Lead A", "leada@test.com"));
        await _sut.Create(CriarComando("Lead B", "leadb@test.com"));

        var resultado = await _sut.GetAll();
        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task Criar_ComDadosValidos_DeveRetornarLeadComIdGerado()
    {
        var command = CriarComando("Carlos Silva", "carlos@test.com");

        var resultado = await _sut.Create(command);

        resultado.Id.Should().NotBeEmpty();
        resultado.Nome.Should().Be("Carlos Silva");
        resultado.Email.Should().Be("carlos@test.com");
    }

    [Fact]
    public async Task Criar_DeveIniciarComStatusNovo()
    {
        var command = CriarComando("Amanda Santos", "amanda@test.com");

        var resultado = await _sut.Create(command);

        resultado.Status.Should().Be(StatusLead.Novo);
    }

    [Fact]
    public async Task ObterPorId_ComIdExistente_DeveRetornarLeadCorreto()
    {
        var criado = await _sut.Create(CriarComando("Felipe Dias", "felipe@test.com"));

        var resultado = await _sut.GetById(criado.Id);

        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(criado.Id);
        resultado.Nome.Should().Be("Felipe Dias");
    }

    [Fact]
    public async Task ObterPorId_ComIdInexistente_DeveRetornarNull()
    {
        var resultado = await _sut.GetById(Guid.NewGuid());
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Atualizar_ComDadosValidos_DeveAlterarPropriedades()
    {
        var origem = new OrigemLead { Id = Guid.NewGuid(), Descricao = "Instagram", Ativo = true };
        _ctx.OrigensLead.Add(origem);
        await _ctx.SaveChangesAsync();

        var lead = await _sut.Create(CriarComando("Original", "original@test.com", origem.Id));
        var command = new AtualizarLeadCommand(lead.Id, "Atualizado", "atualizado@test.com", "99999-9999", origem.Id);

        var atualizado = await _sut.Update(command);

        atualizado.Nome.Should().Be("Atualizado");
        atualizado.Email.Should().Be("atualizado@test.com");
        atualizado.Telefone.Should().Be("99999-9999");
        atualizado.OrigemId.Should().Be(origem.Id);
        atualizado.Origem.Should().Be("Instagram");
        atualizado.AtualizadoEm.Should().NotBeNull();
    }

    [Fact]
    public async Task Atualizar_ComIdInexistente_DeveLancarKeyNotFoundException()
    {
        var command = new AtualizarLeadCommand(Guid.NewGuid(), "Inexistente", "inexistente@test.com", null, null);

        var act = async () => await _sut.Update(command);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateStatus_ComStatusValido_DeveAlterarCorretamente()
    {
        var lead = await _sut.Create(CriarComando("Lead Status", "status@test.com"));
        var command = new AtualizarStatusLeadCommand(lead.Id, StatusLead.Negociando, null);

        var atualizado = await _sut.UpdateStatus(command);

        atualizado.Status.Should().Be(StatusLead.Negociando);
        atualizado.AtualizadoEm.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateStatus_ParaPerdido_DeveSalvarMotivoPerda()
    {
        var lead = await _sut.Create(CriarComando("Lead Perdido", "perdido@test.com"));
        var command = new AtualizarStatusLeadCommand(lead.Id, StatusLead.Perdido, "Preço muito alto");

        var atualizado = await _sut.UpdateStatus(command);

        atualizado.Status.Should().Be(StatusLead.Perdido);
        atualizado.MotivoPerda.Should().Be("Preço muito alto");
        atualizado.AtualizadoEm.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateStatus_ComIdInexistente_DeveLancarKeyNotFoundException()
    {
        var command = new AtualizarStatusLeadCommand(Guid.NewGuid(), StatusLead.Negociando, null);

        var act = async () => await _sut.UpdateStatus(command);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task RegistrarContato_ComDadosValidos_DeveAdicionarHistorico()
    {
        var lead = await _sut.Create(CriarComando("Lead Contato", "contato@test.com"));
        var command = new RegistrarContatoLeadCommand(lead.Id, "WhatsApp", "Conversa inicial sobre o projeto");

        var historico = await _sut.RegisterContact(command);

        historico.Id.Should().NotBeEmpty();
        historico.LeadId.Should().Be(lead.Id);
        historico.Canal.Should().Be("WhatsApp");
        historico.Resumo.Should().Be("Conversa inicial sobre o projeto");

        var leadAtualizado = await _sut.GetById(lead.Id);
        leadAtualizado!.HistoricoContatos.Should().HaveCount(1);
    }

    [Fact]
    public async Task RegistrarContato_ComIdInexistente_DeveLancarKeyNotFoundException()
    {
        var command = new RegistrarContatoLeadCommand(Guid.NewGuid(), "Email", "Teste");

        var act = async () => await _sut.RegisterContact(command);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Excluir_ComIdExistente_DeveRemoverLead()
    {
        var lead = await _sut.Create(CriarComando("Deletar", "deletar@test.com"));

        await _sut.Delete(lead.Id);
        var resultado = await _sut.GetById(lead.Id);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Excluir_ComIdInexistente_DeveLancarKeyNotFoundException()
    {
        var act = async () => await _sut.Delete(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Criar_ComEmailDuplicado_DeveLancarArgumentException()
    {
        await _sut.Create(CriarComando("Lead Existente", "duplicado@test.com"));
        var command = CriarComando("Lead Novo", "duplicado@test.com");

        var act = async () => await _sut.Create(command);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Este e-mail já está cadastrado para outro lead.");
    }

    [Fact]
    public async Task Atualizar_ComEmailDuplicado_DeveLancarArgumentException()
    {
        var lead1 = await _sut.Create(CriarComando("Lead 1", "email1@test.com"));
        var lead2 = await _sut.Create(CriarComando("Lead 2", "email2@test.com"));

        var command = new AtualizarLeadCommand(lead2.Id, "Lead 2 Atualizado", "email1@test.com", "99999-9999", null);

        var act = async () => await _sut.Update(command);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Este e-mail já está cadastrado para outro lead.");
    }

    private static CriarLeadCommand CriarComando(string nome, string email, Guid? origemId = null) =>
        new(
            Nome:     nome,
            Email:    email,
            Telefone: "98765-4321",
            OrigemId: origemId
        );
}
