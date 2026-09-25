using ArchiFlow.Application.Honorarios.Commands;
using ArchiFlow.Application.Honorarios.DTOs;
using ArchiFlow.Application.Honorarios.Services;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Honorarios;
using ArchiFlow.Domain.Leads;
using ArchiFlow.Domain.Projetos.Enum;
using ArchiFlow.Domain.Shared;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Services;

public class PropostaHonorarioServiceTests
{
    private readonly Mock<IPropostaHonorarioRepository> _mockRepo;
    private readonly Mock<IClienteRepository> _mockClienteRepo;
    private readonly Mock<ILeadRepository> _mockLeadRepo;
    private readonly Mock<ICalculadoraHonorariosService> _mockCalculadora;
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly PropostaHonorarioService _service;

    public PropostaHonorarioServiceTests()
    {
        _mockRepo = new Mock<IPropostaHonorarioRepository>();
        _mockClienteRepo = new Mock<IClienteRepository>();
        _mockLeadRepo = new Mock<ILeadRepository>();
        _mockCalculadora = new Mock<ICalculadoraHonorariosService>();
        _mockUow = new Mock<IUnitOfWork>();

        _service = new PropostaHonorarioService(
            _mockRepo.Object,
            _mockClienteRepo.Object,
            _mockLeadRepo.Object,
            _mockCalculadora.Object,
            _mockUow.Object
        );
    }

    [Fact]
    public async Task GetAll_Should_Return_Mapped_Propostas()
    {
        var propostas = new List<PropostaHonorario>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Titulo = "Proposta Casa Lago",
                Codigo = "PROP-2026-0001",
                MetragemQuadrada = 150m,
                ValorTotalSugerido = 18450m,
                ValorFinalAjustado = 18450m,
                Status = StatusProposta.Rascunho,
                CriadoEm = DateTime.UtcNow
            }
        };

        _mockRepo.Setup(r => r.GetAllWithItens()).ReturnsAsync(propostas);

        var result = await _service.GetAll();

        result.Should().HaveCount(1);
        result.Should().Contain(p => p.Titulo == "Proposta Casa Lago");
    }

    [Fact]
    public async Task GetById_Should_Return_Null_When_Not_Found()
    {
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdWithItens(id)).ReturnsAsync((PropostaHonorario?)null);

        var result = await _service.GetById(id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Criar_Should_Throw_When_Titulo_Or_Metragem_Invalid()
    {
        var cmdSemTitulo = new CriarPropostaCommand("", null, null, null, null, TipoProjeto.Residencial, PadraoImovel.Medio, 100m, null, null, null, null, null);
        var actSemTitulo = () => _service.Criar(cmdSemTitulo);
        await actSemTitulo.Should().ThrowAsync<ArgumentException>();

        var cmdMetragemZero = new CriarPropostaCommand("Título", null, null, null, null, TipoProjeto.Residencial, PadraoImovel.Medio, 0m, null, null, null, null, null);
        var actMetragemZero = () => _service.Criar(cmdMetragemZero);
        await actMetragemZero.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Criar_Should_Save_And_Return_Proposta_When_Valid()
    {
        var clienteId = Guid.NewGuid();
        var leadId = Guid.NewGuid();

        _mockClienteRepo.Setup(r => r.GetById(clienteId)).ReturnsAsync(new Cliente { Id = clienteId, Nome = "Carlos Cliente" });
        _mockLeadRepo.Setup(r => r.GetById(leadId)).ReturnsAsync(new Lead { Id = leadId, Nome = "Lead Interessado" });
        _mockRepo.Setup(r => r.GerarProximoCodigo()).ReturnsAsync("PROP-2026-0001");

        var simulacao = new SimulacaoResultadoDto(
            150m,
            TipoProjeto.Residencial,
            "Residencial",
            PadraoImovel.Medio,
            "Médio",
            18450m,
            123m,
            120m,
            new List<ItemEtapaSimulacaoDto>
            {
                new("Estudo Preliminar", "Desc", true, 20m, 3690m, 24m, 1)
            },
            new MemoriaCalculoDto(150m, 95m, 14250m, "Médio", 1.0m, 0m, "Residencial", 1.1m, 1425m, 65m, 2775m, 120m, 150m)
        );

        _mockCalculadora.Setup(c => c.Calcular(It.IsAny<SimulacaoParametrosDto>())).Returns(simulacao);
        _mockRepo.Setup(r => r.Create(It.IsAny<PropostaHonorario>())).ReturnsAsync((PropostaHonorario p) => p);
        _mockUow.Setup(u => u.Commit(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var cmd = new CriarPropostaCommand(
            "Proposta Completa",
            clienteId,
            null,
            leadId,
            null,
            TipoProjeto.Residencial,
            PadraoImovel.Medio,
            150m,
            150m,
            95m,
            18000m,
            new List<string> { "Estudo Preliminar" },
            "Observações da proposta"
        );

        var result = await _service.Criar(cmd);

        result.Should().NotBeNull();
        result.Titulo.Should().Be("Proposta Completa");
        result.Codigo.Should().Be("PROP-2026-0001");
        result.ClienteNome.Should().Be("Carlos Cliente");
        result.LeadNome.Should().Be("Lead Interessado");
        result.ValorFinalAjustado.Should().Be(18000m);
        result.ItensEtapa.Should().HaveCount(1);
    }

    [Fact]
    public async Task AtualizarStatus_Should_Update_Status_When_Found()
    {
        var id = Guid.NewGuid();
        var proposta = new PropostaHonorario
        {
            Id = id,
            Titulo = "Proposta",
            Codigo = "PROP-001",
            Status = StatusProposta.Rascunho
        };

        _mockRepo.Setup(r => r.GetByIdWithItens(id)).ReturnsAsync(proposta);
        _mockUow.Setup(u => u.Commit(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.AtualizarStatus(id, new AtualizarStatusPropostaCommand(StatusProposta.Aprovada));

        result.Status.Should().Be(StatusProposta.Aprovada);
        result.StatusNome.Should().Be("Aprovada");
    }

    [Fact]
    public async Task AjustarValor_Should_Update_ValorFinal_When_Valid()
    {
        var id = Guid.NewGuid();
        var proposta = new PropostaHonorario
        {
            Id = id,
            Titulo = "Proposta",
            Codigo = "PROP-001",
            ValorFinalAjustado = 18450m
        };

        _mockRepo.Setup(r => r.GetByIdWithItens(id)).ReturnsAsync(proposta);
        _mockUow.Setup(u => u.Commit(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.AjustarValor(id, new AjustarValorPropostaCommand(17500m, "Desconto comercial"));

        result.ValorFinalAjustado.Should().Be(17500m);
        result.Observacoes.Should().Be("Desconto comercial");
    }

    [Fact]
    public async Task Excluir_Should_Return_False_When_Not_Found()
    {
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.Exists(id)).ReturnsAsync(false);

        var result = await _service.Excluir(id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Excluir_Should_Delete_And_Commit_When_Exists()
    {
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.Exists(id)).ReturnsAsync(true);
        _mockRepo.Setup(r => r.Delete(id)).Returns(Task.CompletedTask);
        _mockUow.Setup(u => u.Commit(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.Excluir(id);

        result.Should().BeTrue();
        _mockRepo.Verify(r => r.Delete(id), Times.Once);
    }
}
