using ArchiFlow.Application.Projetos.Commands;
using ArchiFlow.Application.Projetos.DTOs;
using ArchiFlow.Application.Projetos.Facades;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Projetos.Enum;
using FluentAssertions;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Facades;

public class ProjetoFacadeTests
{
    private readonly Mock<IProjetoService> _serviceMock;
    private readonly ProjetoFacade _sut;

    public ProjetoFacadeTests()
    {
        _serviceMock = new Mock<IProjetoService>();
        _sut         = new ProjetoFacade(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_DeveChamarService()
    {
        var dtos = new List<ProjetoDto>();
        _serviceMock.Setup(s => s.GetAll()).ReturnsAsync(dtos);

        var result = await _sut.GetAll();

        result.Should().BeSameAs(dtos);
        _serviceMock.Verify(s => s.GetAll(), Times.Once);
    }

    [Fact]
    public async Task GetById_DeveChamarService()
    {
        var id = Guid.NewGuid();
        var dto = new ProjetoDto(id, "P1", "", StatusProjeto.Briefing, "Briefing", TipoProjeto.Residencial, "Residencial", DateTime.UtcNow, null, 100, Guid.NewGuid(), DateTime.UtcNow, null, new List<EtapaProjetoDto>(), 0);
        _serviceMock.Setup(s => s.GetById(id)).ReturnsAsync(dto);

        var result = await _sut.GetById(id);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.GetById(id), Times.Once);
    }

    [Fact]
    public async Task Create_DeveChamarService()
    {
        var command = new CriarProjetoCommand("Nome", "Desc", TipoProjeto.Residencial, DateTime.UtcNow, null, 150, Guid.NewGuid());
        var dto = new ProjetoDto(Guid.NewGuid(), "Nome", "Desc", StatusProjeto.Briefing, "Briefing", TipoProjeto.Residencial, "Residencial", DateTime.UtcNow, null, 150, Guid.NewGuid(), DateTime.UtcNow, null, new List<EtapaProjetoDto>(), 0);
        _serviceMock.Setup(s => s.Create(command)).ReturnsAsync(dto);

        var result = await _sut.Create(command);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.Create(command), Times.Once);
    }

    [Fact]
    public async Task Update_DeveChamarService()
    {
        var command = new AtualizarProjetoCommand(Guid.NewGuid(), "Nome", "Desc", TipoProjeto.Residencial, StatusProjeto.Briefing, DateTime.UtcNow, null, 150);
        var dto = new ProjetoDto(command.Id, "Nome", "Desc", StatusProjeto.Briefing, "Briefing", TipoProjeto.Residencial, "Residencial", DateTime.UtcNow, null, 150, Guid.NewGuid(), DateTime.UtcNow, null, new List<EtapaProjetoDto>(), 0);
        _serviceMock.Setup(s => s.Update(command)).ReturnsAsync(dto);

        var result = await _sut.Update(command);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.Update(command), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_DeveChamarService()
    {
        var command = new AtualizarStatusProjetoCommand(Guid.NewGuid(), StatusProjeto.Desenvolvimento);
        var dto = new ProjetoDto(command.Id, "Nome", "Desc", StatusProjeto.Desenvolvimento, "Desenvolvimento", TipoProjeto.Residencial, "Residencial", DateTime.UtcNow, null, 150, Guid.NewGuid(), DateTime.UtcNow, null, new List<EtapaProjetoDto>(), 0);
        _serviceMock.Setup(s => s.UpdateStatus(command)).ReturnsAsync(dto);

        var result = await _sut.UpdateStatus(command);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.UpdateStatus(command), Times.Once);
    }

    [Fact]
    public async Task CreateEtapa_DeveChamarService()
    {
        var command = new CriarEtapaCommand(Guid.NewGuid(), "Nome", "Desc", 1);
        var dto = new EtapaProjetoDto(Guid.NewGuid(), command.ProjetoId, "Nome", "Desc", StatusEtapa.Pendente, "Pendente", 1, null);
        _serviceMock.Setup(s => s.CreateEtapa(command)).ReturnsAsync(dto);

        var result = await _sut.CreateEtapa(command);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.CreateEtapa(command), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusEtapa_DeveChamarService()
    {
        var command = new AtualizarStatusEtapaCommand(Guid.NewGuid(), StatusEtapa.Concluida);
        var dto = new EtapaProjetoDto(command.EtapaId, Guid.NewGuid(), "Nome", "Desc", StatusEtapa.Concluida, "Concluida", 1, DateTime.UtcNow);
        _serviceMock.Setup(s => s.UpdateStatusEtapa(command)).ReturnsAsync(dto);

        var result = await _sut.UpdateStatusEtapa(command);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.UpdateStatusEtapa(command), Times.Once);
    }

    [Fact]
    public async Task Delete_DeveChamarService()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.Delete(id)).Returns(Task.CompletedTask);

        await _sut.Delete(id);

        _serviceMock.Verify(s => s.Delete(id), Times.Once);
    }
}
