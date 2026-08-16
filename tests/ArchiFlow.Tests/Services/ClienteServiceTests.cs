using ArchiFlow.Application.Clientes.Commands;
using ArchiFlow.Application.Clientes.Services;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Leads;
using ArchiFlow.Domain.Leads.Enum;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Shared;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Services;

public class ClienteServiceTests
{
    private readonly Mock<IClienteRepository> _clienteRepoMock;
    private readonly Mock<ILeadRepository>    _leadRepoMock;
    private readonly Mock<IProjetoRepository> _projetoRepoMock;
    private readonly Mock<IEmailService>      _emailServiceMock;
    private readonly Mock<IUnitOfWork>        _uowMock;
    private readonly Mock<IConfiguration>     _configMock;
    private readonly Mock<ILogger<ClienteService>> _loggerMock;
    private readonly ClienteService           _sut;

    public ClienteServiceTests()
    {
        _clienteRepoMock = new Mock<IClienteRepository>();
        _leadRepoMock    = new Mock<ILeadRepository>();
        _projetoRepoMock = new Mock<IProjetoRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _uowMock         = new Mock<IUnitOfWork>();
        _configMock      = new Mock<IConfiguration>();
        _loggerMock      = new Mock<ILogger<ClienteService>>();

        _sut = new ClienteService(
            _clienteRepoMock.Object,
            _leadRepoMock.Object,
            _projetoRepoMock.Object,
            _emailServiceMock.Object,
            _uowMock.Object,
            _configMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GetAll_DeveRetornarTodosOsClientesComProjetos()
    {
        var id = Guid.NewGuid();
        var clientes = new List<Cliente>
        {
            new Cliente { Id = id, Nome = "Cliente A", Email = "a@test.com", Ativo = true }
        };
        _clienteRepoMock.Setup(r => r.GetAll()).ReturnsAsync(clientes);
        _projetoRepoMock.Setup(r => r.GetByClienteId(id)).ReturnsAsync(new List<Projeto>());

        var result = await _sut.GetAll();

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetById_QuandoExiste_DeveRetornarDto()
    {
        var id = Guid.NewGuid();
        var cliente = new Cliente { Id = id, Nome = "Cliente A", Email = "a@test.com", Ativo = true };
        _clienteRepoMock.Setup(r => r.GetById(id)).ReturnsAsync(cliente);
        _projetoRepoMock.Setup(r => r.GetByClienteId(id)).ReturnsAsync(new List<Projeto>());

        var result = await _sut.GetById(id);

        result.Should().NotBeNull();
        result!.Nome.Should().Be("Cliente A");
    }

    [Fact]
    public async Task GetById_QuandoNaoExiste_DeveRetornarNull()
    {
        _clienteRepoMock.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);

        var result = await _sut.GetById(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task ConvertLead_QuandoDadosSaoValidos_DeveConverterComSucesso()
    {
        var leadId = Guid.NewGuid();
        var origemId = Guid.NewGuid();
        var lead = new Lead
        {
            Id = leadId,
            Nome = "Cliente de Teste",
            Email = "teste@cliente.com",
            Telefone = "123456",
            OrigemId = origemId,
            Status = StatusLead.Novo
        };

        _leadRepoMock.Setup(r => r.GetByIdWithHistorico(leadId)).ReturnsAsync(lead);
        _clienteRepoMock.Setup(r => r.GetByEmail(lead.Email)).ReturnsAsync((Cliente?)null);

        var command = new ConvertLeadToClienteCommand(leadId, "12345678901", "123456", "Rua de Teste", "http://foto.jpg");

        var result = await _sut.ConvertLead(command);

        result.Should().NotBeNull();
        result.SenhaTemporaria.Should().StartWith("Arch@");
        result.Cliente.Nome.Should().Be(lead.Nome);
        result.Cliente.Email.Should().Be(lead.Email);
        result.Cliente.Ativo.Should().BeTrue();

        lead.Status.Should().Be(StatusLead.Convertido);

        _clienteRepoMock.Verify(r => r.Create(It.Is<Cliente>(c =>
            c.Nome == lead.Nome &&
            c.Email == lead.Email &&
            c.LeadId == leadId &&
            c.CpfCnpj == "12345678901" &&
            c.Endereco == "Rua de Teste" &&
            c.SenhaPortal != null
        )), Times.Once);

        _emailServiceMock.Verify(m => m.SendEmailAsync(
            lead.Email,
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Once);

        _uowMock.Verify(u => u.Commit(default), Times.Once);
    }

    [Fact]
    public async Task ConvertLead_QuandoEmailServiceLancaExcecao_NaoDeveFalharConversao()
    {
        var leadId = Guid.NewGuid();
        var lead = new Lead
        {
            Id = leadId,
            Nome = "Cliente Teste",
            Email = "teste@cliente.com",
            OrigemId = Guid.NewGuid()
        };

        _leadRepoMock.Setup(r => r.GetByIdWithHistorico(leadId)).ReturnsAsync(lead);
        _clienteRepoMock.Setup(r => r.GetByEmail(lead.Email)).ReturnsAsync((Cliente?)null);
        _emailServiceMock.Setup(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Falha no servidor SMTP"));

        var command = new ConvertLeadToClienteCommand(leadId);

        var result = await _sut.ConvertLead(command);

        result.Should().NotBeNull();
        result.Cliente.Nome.Should().Be(lead.Nome);
    }

    [Fact]
    public async Task ConvertLead_QuandoLeadNaoEncontrado_DeveLancarKeyNotFoundException()
    {
        _leadRepoMock.Setup(r => r.GetByIdWithHistorico(It.IsAny<Guid>())).ReturnsAsync((Lead?)null);

        var command = new ConvertLeadToClienteCommand(Guid.NewGuid());

        Func<Task> act = () => _sut.ConvertLead(command);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ConvertLead_QuandoEmailJaCadastradoParaOutroCliente_DeveLancarArgumentException()
    {
        var leadId = Guid.NewGuid();
        var lead = new Lead
        {
            Id = leadId,
            Nome = "Cliente de Teste",
            Email = "duplicado@cliente.com",
            OrigemId = Guid.NewGuid()
        };

        _leadRepoMock.Setup(r => r.GetByIdWithHistorico(leadId)).ReturnsAsync(lead);
        _clienteRepoMock.Setup(r => r.GetByEmail(lead.Email)).ReturnsAsync(new Cliente { Id = Guid.NewGuid(), Email = lead.Email });

        var command = new ConvertLeadToClienteCommand(leadId);

        Func<Task> act = () => _sut.ConvertLead(command);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Este e-mail já está cadastrado para outro cliente*");
    }

    [Fact]
    public async Task Update_QuandoDadosValidos_DeveAtualizarCliente()
    {
        var id = Guid.NewGuid();
        var cliente = new Cliente { Id = id, Nome = "Antigo", Email = "antigo@teste.com" };
        _clienteRepoMock.Setup(r => r.GetById(id)).ReturnsAsync(cliente);
        _clienteRepoMock.Setup(r => r.GetByEmail("novo@teste.com")).ReturnsAsync((Cliente?)null);
        _projetoRepoMock.Setup(r => r.GetByClienteId(id)).ReturnsAsync(new List<Projeto>());

        var command = new AtualizarClienteCommand(id, "Novo Nome", "novo@teste.com", "999", "111", "Rua", "http://foto.png");

        var result = await _sut.Update(command);

        result.Nome.Should().Be("Novo Nome");
        result.Email.Should().Be("novo@teste.com");
        _uowMock.Verify(u => u.Commit(default), Times.Once);
    }

    [Fact]
    public async Task Update_QuandoFotoUrlForDelete_DeveLimparFoto()
    {
        var id = Guid.NewGuid();
        var cliente = new Cliente { Id = id, Nome = "Nome", Email = "email@teste.com", FotoUrl = "http://foto.png" };
        _clienteRepoMock.Setup(r => r.GetById(id)).ReturnsAsync(cliente);
        _clienteRepoMock.Setup(r => r.GetByEmail("email@teste.com")).ReturnsAsync(cliente);
        _projetoRepoMock.Setup(r => r.GetByClienteId(id)).ReturnsAsync(new List<Projeto>());

        var command = new AtualizarClienteCommand(id, "Nome", "email@teste.com", null, null, null, "DELETE");

        var result = await _sut.Update(command);

        result.FotoUrl.Should().BeNull();
    }

    [Fact]
    public async Task Update_QuandoClienteNaoEncontrado_DeveLancarKeyNotFoundException()
    {
        _clienteRepoMock.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);

        var command = new AtualizarClienteCommand(Guid.NewGuid(), "Nome", "email@teste.com", null, null, null, null);

        Func<Task> act = () => _sut.Update(command);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Update_QuandoEmailVazio_DeveLancarArgumentException()
    {
        var id = Guid.NewGuid();
        var cliente = new Cliente { Id = id, Nome = "Nome", Email = "email@teste.com" };
        _clienteRepoMock.Setup(r => r.GetById(id)).ReturnsAsync(cliente);

        var command = new AtualizarClienteCommand(id, "Nome", "  ", null, null, null, null);

        Func<Task> act = () => _sut.Update(command);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*E-mail é obrigatório.*");
    }

    [Fact]
    public async Task Update_QuandoEmailJaCadastradoParaOutroCliente_DeveLancarArgumentException()
    {
        var id = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var cliente = new Cliente { Id = id, Nome = "Nome", Email = "email@teste.com" };
        var otherCliente = new Cliente { Id = otherId, Email = "outro@teste.com" };
        _clienteRepoMock.Setup(r => r.GetById(id)).ReturnsAsync(cliente);
        _clienteRepoMock.Setup(r => r.GetByEmail("outro@teste.com")).ReturnsAsync(otherCliente);

        var command = new AtualizarClienteCommand(id, "Nome", "outro@teste.com", null, null, null, null);

        Func<Task> act = () => _sut.Update(command);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Este e-mail já está cadastrado para outro cliente.*");
    }

    [Fact]
    public async Task UpdatePortalAccess_DeveAtualizarStatusComSucesso()
    {
        var clienteId = Guid.NewGuid();
        var cliente = new Cliente
        {
            Id = clienteId,
            Nome = "Cliente",
            Email = "cliente@teste.com",
            Ativo = true
        };

        _clienteRepoMock.Setup(r => r.GetById(clienteId)).ReturnsAsync(cliente);
        _projetoRepoMock.Setup(r => r.GetByClienteId(clienteId)).ReturnsAsync(new List<Projeto>());

        var command = new AtualizarPortalAccessCommand(clienteId, false);

        var result = await _sut.UpdatePortalAccess(command);

        result.Ativo.Should().BeFalse();
        cliente.Ativo.Should().BeFalse();
        _clienteRepoMock.Verify(r => r.Update(cliente), Times.Once);
        _uowMock.Verify(u => u.Commit(default), Times.Once);
    }

    [Fact]
    public async Task UpdatePortalAccess_QuandoClienteNaoEncontrado_DeveLancarKeyNotFoundException()
    {
        _clienteRepoMock.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);

        var command = new AtualizarPortalAccessCommand(Guid.NewGuid(), true);

        Func<Task> act = () => _sut.UpdatePortalAccess(command);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
