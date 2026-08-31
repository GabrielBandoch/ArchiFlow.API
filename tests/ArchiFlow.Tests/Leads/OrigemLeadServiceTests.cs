using ArchiFlow.Application.Leads.Commands;
using ArchiFlow.Application.Leads.Services;
using ArchiFlow.Domain.Leads;
using ArchiFlow.Infrastructure.Repositories;
using ArchiFlow.Infrastructure.Repositories.Leads;
using ArchiFlow.Infrastructure.Data;
using ArchiFlow.Tests.Common;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Leads;

public class OrigemLeadServiceTests
{
    private readonly ArchiFlowDbContext _ctx;
    private readonly OrigemLeadService _sut;

    public OrigemLeadServiceTests()
    {
        _ctx = TestDbContextFactory.Create();
        var repository = new OrigemLeadRepository(_ctx);
        var unitOfWork = new UnitOfWork(_ctx);

        _sut = new OrigemLeadService(repository, unitOfWork);
    }

    [Fact]
    public async Task GetAll_DeveRetornarTodasAsOrigensOrdenadas()
    {
        _ctx.OrigensLead.AddRange(
            new OrigemLead { Id = Guid.NewGuid(), Descricao = "Instagram", Ativo = true },
            new OrigemLead { Id = Guid.NewGuid(), Descricao = "Site", Ativo = false },
            new OrigemLead { Id = Guid.NewGuid(), Descricao = "E-mail", Ativo = true }
        );
        await _ctx.SaveChangesAsync();

        var resultado = await _sut.GetAll();

        resultado.Should().HaveCount(3);
        resultado.Select(o => o.Descricao).Should().ContainInOrder("E-mail", "Instagram", "Site");
    }

    [Fact]
    public async Task GetAllActive_DeveRetornarApenasOrigensAtivas()
    {
        _ctx.OrigensLead.AddRange(
            new OrigemLead { Id = Guid.NewGuid(), Descricao = "Instagram", Ativo = true },
            new OrigemLead { Id = Guid.NewGuid(), Descricao = "Site", Ativo = false }
        );
        await _ctx.SaveChangesAsync();

        var resultado = await _sut.GetAllActive();

        resultado.Should().HaveCount(1);
        resultado.First().Descricao.Should().Be("Instagram");
    }

    [Fact]
    public async Task Create_ComDescricaoValida_DeveCriarOrigem()
    {
        var command = new CriarOrigemLeadCommand("Recomendação");

        var resultado = await _sut.Create(command);

        resultado.Id.Should().NotBeEmpty();
        resultado.Descricao.Should().Be("Recomendação");
        resultado.Ativo.Should().BeTrue();

        var noBanco = await _ctx.OrigensLead.FindAsync(resultado.Id);
        noBanco.Should().NotBeNull();
        noBanco!.Descricao.Should().Be("Recomendação");
    }

    [Fact]
    public async Task Create_ComDescricaoVazia_DeveLancarArgumentException()
    {
        var command = new CriarOrigemLeadCommand("  ");

        var act = async () => await _sut.Create(command);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Update_ComDadosValidos_DeveAtualizarDescricao()
    {
        var origem = new OrigemLead { Id = Guid.NewGuid(), Descricao = "Insta", Ativo = true };
        _ctx.OrigensLead.Add(origem);
        await _ctx.SaveChangesAsync();

        var command = new AtualizarOrigemLeadCommand(origem.Id, "Instagram");

        var resultado = await _sut.Update(command);

        resultado.Descricao.Should().Be("Instagram");

        var noBanco = await _ctx.OrigensLead.FindAsync(origem.Id);
        noBanco!.Descricao.Should().Be("Instagram");
    }

    [Fact]
    public async Task Desativar_ComIdExistente_DeveFazerSoftDelete()
    {
        var origem = new OrigemLead { Id = Guid.NewGuid(), Descricao = "Antigo", Ativo = true };
        _ctx.OrigensLead.Add(origem);
        await _ctx.SaveChangesAsync();

        var resultado = await _sut.Desativar(origem.Id);

        resultado.Ativo.Should().BeFalse();

        var noBanco = await _ctx.OrigensLead.FindAsync(origem.Id);
        noBanco.Should().NotBeNull();
        noBanco!.Ativo.Should().BeFalse();
    }

    [Fact]
    public async Task Reativar_ComIdInativo_DeveReativarOrigem()
    {
        var origem = new OrigemLead { Id = Guid.NewGuid(), Descricao = "Inativo", Ativo = false };
        _ctx.OrigensLead.Add(origem);
        await _ctx.SaveChangesAsync();

        var resultado = await _sut.Reativar(origem.Id);

        resultado.Ativo.Should().BeTrue();

        var noBanco = await _ctx.OrigensLead.FindAsync(origem.Id);
        noBanco.Should().NotBeNull();
        noBanco!.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task GetById_QuandoExiste_DeveRetornarDto()
    {
        var origem = new OrigemLead { Id = Guid.NewGuid(), Descricao = "Indicação", Ativo = true };
        _ctx.OrigensLead.Add(origem);
        await _ctx.SaveChangesAsync();

        var resultado = await _sut.GetById(origem.Id);

        resultado.Should().NotBeNull();
        resultado!.Descricao.Should().Be("Indicação");
    }

    [Fact]
    public async Task GetById_QuandoNaoExiste_DeveRetornarNull()
    {
        var resultado = await _sut.GetById(Guid.NewGuid());
        resultado.Should().BeNull();
    }
}
