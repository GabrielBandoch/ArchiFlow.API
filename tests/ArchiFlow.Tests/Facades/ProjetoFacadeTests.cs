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
    public async Task AdicionarTarefa_DeveChamarService()
    {
        var command = new AdicionarTarefaCommand(Guid.NewGuid(), "Planta");
        var dto = new TarefaEtapaDto(Guid.NewGuid(), command.EtapaId, "Planta", false, null);
        _serviceMock.Setup(s => s.AdicionarTarefa(command)).ReturnsAsync(dto);

        var result = await _sut.AdicionarTarefa(command);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.AdicionarTarefa(command), Times.Once);
    }

    [Fact]
    public async Task AlternarTarefa_DeveChamarService()
    {
        var id = Guid.NewGuid();
        var dto = new TarefaEtapaDto(id, Guid.NewGuid(), "Planta", true, DateTime.UtcNow);
        _serviceMock.Setup(s => s.AlternarTarefa(id)).ReturnsAsync(dto);

        var result = await _sut.AlternarTarefa(id);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.AlternarTarefa(id), Times.Once);
    }

    [Fact]
    public async Task RemoverTarefa_DeveChamarService()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.RemoverTarefa(id)).Returns(Task.CompletedTask);

        await _sut.RemoverTarefa(id);

        _serviceMock.Verify(s => s.RemoverTarefa(id), Times.Once);
    }

    [Fact]
    public async Task ObterTemplates_DeveChamarService()
    {
        var templates = new List<TemplateProjetoDto>();
        _serviceMock.Setup(s => s.ObterTemplates()).ReturnsAsync(templates);

        var result = await _sut.ObterTemplates();

        result.Should().BeSameAs(templates);
        _serviceMock.Verify(s => s.ObterTemplates(), Times.Once);
    }

    [Fact]
    public async Task ObterTemplatePorId_DeveChamarService()
    {
        var id = Guid.NewGuid();
        var dto = new TemplateProjetoDto(id, "res", "Res", "Desc", "home", true, new List<TemplateEtapaDto>());
        _serviceMock.Setup(s => s.ObterTemplatePorId(id)).ReturnsAsync(dto);

        var result = await _sut.ObterTemplatePorId(id);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.ObterTemplatePorId(id), Times.Once);
    }

    [Fact]
    public async Task CriarTemplate_DeveChamarService()
    {
        var command = new CriarTemplateProjetoCommand("res", "Res", "Desc", "home", new List<CriarTemplateEtapaItemCommand>());
        var dto = new TemplateProjetoDto(Guid.NewGuid(), "res", "Res", "Desc", "home", true, new List<TemplateEtapaDto>());
        _serviceMock.Setup(s => s.CriarTemplate(command)).ReturnsAsync(dto);

        var result = await _sut.CriarTemplate(command);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.CriarTemplate(command), Times.Once);
    }

    [Fact]
    public async Task AtualizarTemplate_DeveChamarService()
    {
        var command = new AtualizarTemplateProjetoCommand(Guid.NewGuid(), "Res", "Desc", "home", new List<CriarTemplateEtapaItemCommand>());
        var dto = new TemplateProjetoDto(command.Id, "res", "Res", "Desc", "home", true, new List<TemplateEtapaDto>());
        _serviceMock.Setup(s => s.AtualizarTemplate(command)).ReturnsAsync(dto);

        var result = await _sut.AtualizarTemplate(command);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.AtualizarTemplate(command), Times.Once);
    }

    [Fact]
    public async Task ExcluirTemplate_DeveChamarService()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.ExcluirTemplate(id)).Returns(Task.CompletedTask);

        await _sut.ExcluirTemplate(id);

        _serviceMock.Verify(s => s.ExcluirTemplate(id), Times.Once);
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
