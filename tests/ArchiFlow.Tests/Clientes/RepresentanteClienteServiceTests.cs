using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArchiFlow.Application.Clientes.Commands;
using ArchiFlow.Application.Clientes.Services;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Empresa;
using ArchiFlow.Domain.Shared;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Clientes;

public class RepresentanteClienteServiceTests
{
    private readonly Mock<IRepresentanteClienteRepository> _repRepoMock = new();
    private readonly Mock<IClienteRepository> _clienteRepoMock = new();
    private readonly Mock<IDadosEmpresaRepository> _empresaRepoMock = new();
    private readonly Mock<IEmailService> _emailMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ILogger<RepresentanteClienteService>> _loggerMock = new();
    private readonly RepresentanteClienteService _service;

    public RepresentanteClienteServiceTests()
    {
        _service = new RepresentanteClienteService(
            _repRepoMock.Object,
            _clienteRepoMock.Object,
            _empresaRepoMock.Object,
            _emailMock.Object,
            _uowMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ConvidarRepresentanteAsync_WhenValid_CreatesRepresentativeAndSendsEmail()
    {
        var clienteId = Guid.NewGuid();
        var cliente = new Cliente
        {
            Id = clienteId,
            Nome = "Construtora Alfa",
            Email = "contato@alfa.com"
        };
        _clienteRepoMock.Setup(c => c.GetById(clienteId)).ReturnsAsync(cliente);
        _repRepoMock.Setup(r => r.ObterPorEmailAsync("diretor@alfa.com")).ReturnsAsync((RepresentanteCliente?)null);

        var command = new ConvidarRepresentanteCommand("Diretor Carlos", "diretor@alfa.com", "(11) 9999-8888", "Diretor");

        var result = await _service.ConvidarRepresentanteAsync(clienteId, command, "http://localhost:4200");

        result.Nome.Should().Be("Diretor Carlos");
        result.Email.Should().Be("diretor@alfa.com");

        _repRepoMock.Verify(r => r.AdicionarAsync(It.Is<RepresentanteCliente>(rep =>
            rep.ClienteId == clienteId &&
            rep.Email == "diretor@alfa.com" &&
            !string.IsNullOrEmpty(rep.ConviteTokenHash)
        )), Times.Once);

        _emailMock.Verify(e => e.SendEmailAsync(
            "diretor@alfa.com",
            It.Is<string>(s => s.Contains("Convite")),
            It.Is<string>(body => body.Contains("ativar-conta"))
        ), Times.Once);
    }

    [Fact]
    public async Task ConvidarRepresentanteAsync_WhenEnviarEmailConviteIsFalse_CreatesRepWithoutEmail()
    {
        var clienteId = Guid.NewGuid();
        var cliente = new Cliente
        {
            Id = clienteId,
            Nome = "Construtora Alfa",
            Email = "contato@alfa.com"
        };
        _clienteRepoMock.Setup(c => c.GetById(clienteId)).ReturnsAsync(cliente);
        _repRepoMock.Setup(r => r.ObterPorEmailAsync("financeiro@alfa.com")).ReturnsAsync((RepresentanteCliente?)null);

        var command = new ConvidarRepresentanteCommand("Financeiro Ana", "financeiro@alfa.com", null, "Financeiro", EnviarEmailConvite: false);

        var result = await _service.ConvidarRepresentanteAsync(clienteId, command, "http://localhost:4200");

        result.Nome.Should().Be("Financeiro Ana");
        result.Email.Should().Be("financeiro@alfa.com");

        _repRepoMock.Verify(r => r.AdicionarAsync(It.Is<RepresentanteCliente>(rep =>
            rep.ClienteId == clienteId &&
            rep.Email == "financeiro@alfa.com" &&
            string.IsNullOrEmpty(rep.ConviteTokenHash)
        )), Times.Once);

        _emailMock.Verify(e => e.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Never);
    }

    [Fact]
    public async Task ValidarTokenConviteAsync_WhenValid_ReturnsClientAndRepInfo()
    {
        var rawToken = "invite-token-abc";
        using var sha = SHA256.Create();
        var hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();

        var cliente = new Cliente { Id = Guid.NewGuid(), Nome = "Empresa Beta" };
        var rep = new RepresentanteCliente
        {
            Id = Guid.NewGuid(),
            ClienteId = cliente.Id,
            Nome = "Ana Souza",
            Email = "ana@beta.com",
            ConviteTokenHash = hash,
            ConviteExpiraEm = DateTime.UtcNow.AddDays(1),
            Cliente = cliente
        };

        _repRepoMock.Setup(r => r.ObterPorTokenConviteHashAsync(hash)).ReturnsAsync(rep);

        var resultado = await _service.ValidarTokenConviteAsync(rawToken);

        resultado.Valido.Should().BeTrue();
        resultado.Nome.Should().Be("Ana Souza");
        resultado.Email.Should().Be("ana@beta.com");
    }

    [Fact]
    public async Task AtivarContaAsync_WhenTokenValid_SetsPasswordAndActivates()
    {
        var rawToken = "valid-token-activate";
        using var sha = SHA256.Create();
        var hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();

        var rep = new RepresentanteCliente
        {
            Id = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            Nome = "Ana Souza",
            Email = "ana@beta.com",
            ConviteTokenHash = hash,
            ConviteExpiraEm = DateTime.UtcNow.AddDays(1),
            Ativo = false
        };

        _repRepoMock.Setup(r => r.ObterPorTokenConviteHashAsync(hash)).ReturnsAsync(rep);

        var command = new AtivarContaRepresentanteCommand(rawToken, "SenhaSegura@2026");
        await _service.AtivarContaAsync(command);

        rep.Ativo.Should().BeTrue();
        rep.ConviteTokenHash.Should().BeNull();
        BCrypt.Net.BCrypt.Verify("SenhaSegura@2026", rep.SenhaHash).Should().BeTrue();
        _repRepoMock.Verify(r => r.AtualizarAsync(rep), Times.Once);
    }
}
