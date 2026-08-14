using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Application.Leads.Commands;
using ArchiFlow.Application.Leads.DTOs;
using ArchiFlow.Application.Leads.Facades;
using ArchiFlow.Domain.Leads.Enum;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Facades;

public class LeadFacadeTests
{
    private readonly Mock<ILeadService> _serviceMock;
    private readonly LeadFacade _sut;

    public LeadFacadeTests()
    {
        _serviceMock = new Mock<ILeadService>();
        _sut         = new LeadFacade(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_DeveDelegarParaService()
    {
        var dtos = new List<LeadDto>();
        _serviceMock.Setup(s => s.GetAll()).ReturnsAsync(dtos);

        var result = await _sut.GetAll();

        result.Should().BeSameAs(dtos);
        _serviceMock.Verify(s => s.GetAll(), Times.Once);
    }

    [Fact]
    public async Task GetById_DeveDelegarParaService()
    {
        var id = Guid.NewGuid();
        var dto = new LeadDto(id, "Nome", "email@test.com", "99999-9999", null, "Site", null, StatusLead.Novo, "Novo", DateTime.UtcNow, null, new List<HistoricoContatoLeadDto>());
        _serviceMock.Setup(s => s.GetById(id)).ReturnsAsync(dto);

        var result = await _sut.GetById(id);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.GetById(id), Times.Once);
    }

    [Fact]
    public async Task Create_DeveDelegarParaService()
    {
        var command = new CriarLeadCommand("Nome", "email@test.com", "99999-9999", null);
        var dto = new LeadDto(Guid.NewGuid(), "Nome", "email@test.com", "99999-9999", null, null, null, StatusLead.Novo, "Novo", DateTime.UtcNow, null, new List<HistoricoContatoLeadDto>());
        _serviceMock.Setup(s => s.Create(command)).ReturnsAsync(dto);

        var result = await _sut.Create(command);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.Create(command), Times.Once);
    }

    [Fact]
    public async Task Update_DeveDelegarParaService()
    {
        var command = new AtualizarLeadCommand(Guid.NewGuid(), "Nome", "email@test.com", "99999-9999", null);
        var dto = new LeadDto(command.Id, "Nome", "email@test.com", "99999-9999", null, null, null, StatusLead.Novo, "Novo", DateTime.UtcNow, null, new List<HistoricoContatoLeadDto>());
        _serviceMock.Setup(s => s.Update(command)).ReturnsAsync(dto);

        var result = await _sut.Update(command);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.Update(command), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_DeveDelegarParaService()
    {
        var command = new AtualizarStatusLeadCommand(Guid.NewGuid(), StatusLead.EmContato, null);
        var dto = new LeadDto(command.Id, "Nome", "email@test.com", "99999-9999", null, null, null, StatusLead.EmContato, "Em Contato", DateTime.UtcNow, null, new List<HistoricoContatoLeadDto>());
        _serviceMock.Setup(s => s.UpdateStatus(command)).ReturnsAsync(dto);

        var result = await _sut.UpdateStatus(command);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.UpdateStatus(command), Times.Once);
    }

    [Fact]
    public async Task RegisterContact_DeveDelegarParaService()
    {
        var command = new RegistrarContatoLeadCommand(Guid.NewGuid(), "WhatsApp", "Resumo");
        var dto = new HistoricoContatoLeadDto(Guid.NewGuid(), command.LeadId, DateTime.UtcNow, "WhatsApp", "Resumo");
        _serviceMock.Setup(s => s.RegisterContact(command)).ReturnsAsync(dto);

        var result = await _sut.RegisterContact(command);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.RegisterContact(command), Times.Once);
    }

    [Fact]
    public async Task Delete_DeveDelegarParaService()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.Delete(id)).Returns(Task.CompletedTask);

        await _sut.Delete(id);

        _serviceMock.Verify(s => s.Delete(id), Times.Once);
    }
}
