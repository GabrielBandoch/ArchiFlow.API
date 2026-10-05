using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.Application.Agenda.Services;
using ArchiFlow.Domain.Agenda;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Leads;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Usuarios;
using FluentAssertions;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Agenda;

public class AgendaValidationServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock = new();
    private readonly Mock<IProjetoRepository> _projetoRepoMock = new();
    private readonly Mock<IClienteRepository> _clienteRepoMock = new();
    private readonly Mock<ILeadRepository> _leadRepoMock = new();
    private readonly AgendaValidationService _service;

    public AgendaValidationServiceTests()
    {
        _service = new AgendaValidationService(
            _usuarioRepoMock.Object,
            _projetoRepoMock.Object,
            _clienteRepoMock.Object,
            _leadRepoMock.Object);
    }

    [Fact]
    public async Task ValidarEntidadesRelacionadasAsync_QuandoNenhumIdInformado_NaoDisparaExcecao()
    {
        var escritorioId = Guid.NewGuid();

        var act = () => _service.ValidarEntidadesRelacionadasAsync(escritorioId, null, null, null, null);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidarEntidadesRelacionadasAsync_QuandoUsuarioPertenceAoEscritorio_Sucesso()
    {
        var escritorioId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var usuario = new Usuario
        {
            Id = usuarioId,
            Nome = "Arquiteto Responsavel",
            EscritorioId = escritorioId
        };
        _usuarioRepoMock.Setup(r => r.GetById(usuarioId)).ReturnsAsync(usuario);

        var act = () => _service.ValidarEntidadesRelacionadasAsync(escritorioId, usuarioId, null, null, null);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidarEntidadesRelacionadasAsync_QuandoUsuarioNaoEncontrado_LancaInvalidOperationException()
    {
        var escritorioId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        _usuarioRepoMock.Setup(r => r.GetById(usuarioId)).ReturnsAsync((Usuario?)null);

        var act = () => _service.ValidarEntidadesRelacionadasAsync(escritorioId, usuarioId, null, null, null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{usuarioId}*não foi encontrado*");
    }

    [Fact]
    public async Task ValidarEntidadesRelacionadasAsync_QuandoUsuarioPertenceAOutroEscritorio_LancaUnauthorizedAccessException()
    {
        var escritorioId = Guid.NewGuid();
        var outroEscritorioId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var usuario = new Usuario
        {
            Id = usuarioId,
            Nome = "Invasor",
            EscritorioId = outroEscritorioId
        };
        _usuarioRepoMock.Setup(r => r.GetById(usuarioId)).ReturnsAsync(usuario);

        var act = () => _service.ValidarEntidadesRelacionadasAsync(escritorioId, usuarioId, null, null, null);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invasor*pertence a outro escritório*");
    }

    [Fact]
    public async Task ValidarEntidadesRelacionadasAsync_QuandoProjetoNaoEncontrado_LancaInvalidOperationException()
    {
        var escritorioId = Guid.NewGuid();
        var projetoId = Guid.NewGuid();
        _projetoRepoMock.Setup(r => r.GetById(projetoId)).ReturnsAsync((Projeto?)null);

        var act = () => _service.ValidarEntidadesRelacionadasAsync(escritorioId, null, projetoId, null, null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{projetoId}*não foi encontrado*");
    }

    [Fact]
    public async Task ValidarEntidadesRelacionadasAsync_QuandoClienteNaoEncontrado_LancaInvalidOperationException()
    {
        var escritorioId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        _clienteRepoMock.Setup(r => r.GetById(clienteId)).ReturnsAsync((Cliente?)null);

        var act = () => _service.ValidarEntidadesRelacionadasAsync(escritorioId, null, null, clienteId, null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{clienteId}*não foi encontrado*");
    }

    [Fact]
    public async Task ValidarEntidadesRelacionadasAsync_QuandoLeadNaoEncontrado_LancaInvalidOperationException()
    {
        var escritorioId = Guid.NewGuid();
        var leadId = Guid.NewGuid();
        _leadRepoMock.Setup(r => r.GetById(leadId)).ReturnsAsync((Lead?)null);

        var act = () => _service.ValidarEntidadesRelacionadasAsync(escritorioId, null, null, null, leadId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{leadId}*não foi encontrado*");
    }

    [Fact]
    public async Task ValidarEntidadesRelacionadasAsync_QuandoTodasEntidadesValidas_Sucesso()
    {
        var escritorioId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var projetoId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var leadId = Guid.NewGuid();

        _usuarioRepoMock.Setup(r => r.GetById(usuarioId)).ReturnsAsync(new Usuario { Id = usuarioId, EscritorioId = escritorioId, Nome = "Arq" });
        _projetoRepoMock.Setup(r => r.GetById(projetoId)).ReturnsAsync(new Projeto { Id = projetoId, Nome = "Proj Alpha" });
        _clienteRepoMock.Setup(r => r.GetById(clienteId)).ReturnsAsync(new Cliente { Id = clienteId, Nome = "Cliente Beta" });
        _leadRepoMock.Setup(r => r.GetById(leadId)).ReturnsAsync(new Lead { Id = leadId, Nome = "Lead Gama" });

        var act = () => _service.ValidarEntidadesRelacionadasAsync(escritorioId, usuarioId, projetoId, clienteId, leadId);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ObterNomesRelacionadosAsync_DeveRetornarNomesDasEntidades()
    {
        var usuarioId = Guid.NewGuid();
        var projetoId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var leadId = Guid.NewGuid();

        _usuarioRepoMock.Setup(r => r.GetById(usuarioId)).ReturnsAsync(new Usuario { Id = usuarioId, Nome = "Mariana Arquiteta" });
        _projetoRepoMock.Setup(r => r.GetById(projetoId)).ReturnsAsync(new Projeto { Id = projetoId, Nome = "Mansao Alphaville" });
        _clienteRepoMock.Setup(r => r.GetById(clienteId)).ReturnsAsync(new Cliente { Id = clienteId, Nome = "Roberto Dias" });
        _leadRepoMock.Setup(r => r.GetById(leadId)).ReturnsAsync(new Lead { Id = leadId, Nome = "Lead Corporativo" });

        var (nomeProjeto, nomeCliente, nomeLead, nomeUsuario) = await _service.ObterNomesRelacionadosAsync(projetoId, clienteId, leadId, usuarioId);

        nomeProjeto.Should().Be("Mansao Alphaville");
        nomeCliente.Should().Be("Roberto Dias");
        nomeLead.Should().Be("Lead Corporativo");
        nomeUsuario.Should().Be("Mariana Arquiteta");
    }

    [Fact]
    public async Task ObterNomesEmLoteAsync_QuandoVazio_RetornaBatchVazio()
    {
        var batch = await _service.ObterNomesEmLoteAsync(new List<Compromisso>());

        batch.Projetos.Should().BeEmpty();
        batch.Clientes.Should().BeEmpty();
        batch.Leads.Should().BeEmpty();
        batch.Usuarios.Should().BeEmpty();
    }

    [Fact]
    public async Task ObterNomesEmLoteAsync_ComCompromissos_PreencheBatchCorretamente()
    {
        var pid = Guid.NewGuid();
        var cid = Guid.NewGuid();
        var lid = Guid.NewGuid();
        var uid = Guid.NewGuid();

        _projetoRepoMock.Setup(r => r.GetById(pid)).ReturnsAsync(new Projeto { Id = pid, Nome = "Residencial Sul" });
        _clienteRepoMock.Setup(r => r.GetById(cid)).ReturnsAsync(new Cliente { Id = cid, Nome = "Carlos Cliente" });
        _leadRepoMock.Setup(r => r.GetById(lid)).ReturnsAsync(new Lead { Id = lid, Nome = "Luciana Lead" });
        _usuarioRepoMock.Setup(r => r.GetById(uid)).ReturnsAsync(new Usuario { Id = uid, Nome = "Felipe Usuario" });

        var compromissos = new List<Compromisso>
        {
            new() { Id = Guid.NewGuid(), ProjetoId = pid, ClienteId = cid, LeadId = lid, UsuarioId = uid },
            new() { Id = Guid.NewGuid(), ProjetoId = pid }
        };

        var batch = await _service.ObterNomesEmLoteAsync(compromissos);

        batch.Projetos.Should().ContainKey(pid).WhoseValue.Should().Be("Residencial Sul");
        batch.Clientes.Should().ContainKey(cid).WhoseValue.Should().Be("Carlos Cliente");
        batch.Leads.Should().ContainKey(lid).WhoseValue.Should().Be("Luciana Lead");
        batch.Usuarios.Should().ContainKey(uid).WhoseValue.Should().Be("Felipe Usuario");
    }
}
