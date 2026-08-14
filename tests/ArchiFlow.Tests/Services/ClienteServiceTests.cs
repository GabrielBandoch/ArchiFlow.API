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
    private readonly ClienteService           _sut;

    public ClienteServiceTests()
    {
        _clienteRepoMock = new Mock<IClienteRepository>();
        _leadRepoMock    = new Mock<ILeadRepository>();
        _projetoRepoMock = new Mock<IProjetoRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _uowMock         = new Mock<IUnitOfWork>();
        _configMock      = new Mock<IConfiguration>();

        _sut = new ClienteService(
            _clienteRepoMock.Object,
            _leadRepoMock.Object,
            _projetoRepoMock.Object,
            _emailServiceMock.Object,
            _uowMock.Object,
            _configMock.Object
        );
    }

    [Fact]
    public async Task ConvertLead_QuandoDadosSaoValidos_DeveConverterComSucesso()
    {
        // Arrange
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

        var command = new ConvertLeadToClienteCommand(leadId, "12345678901", "123456", "Rua de Teste");

        // Act
        var result = await _sut.ConvertLead(command);

        // Assert
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
    public async Task ConvertLead_QuandoEmailJaCadastradoParaOutroCliente_DeveLancarArgumentException()
    {
        // Arrange
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

        // Act
        Func<Task> act = () => _sut.ConvertLead(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Este e-mail já está cadastrado para outro cliente*");
    }

    [Fact]
    public async Task UpdatePortalAccess_DeveAtualizarStatusComSucesso()
    {
        // Arrange
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

        // Act
        var result = await _sut.UpdatePortalAccess(command);

        // Assert
        result.Ativo.Should().BeFalse();
        cliente.Ativo.Should().BeFalse();
        _clienteRepoMock.Verify(r => r.Update(cliente), Times.Once);
        _uowMock.Verify(u => u.Commit(default), Times.Once);
    }
}
