using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Leads.Commands;
using ArchiFlow.Application.Leads.DTOs;
using ArchiFlow.Domain.Leads.Enum;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Controllers;

public class LeadsControllerTests
{
    private readonly Mock<ILeadFacade> _facadeMock;
    private readonly LeadsController _sut;

    public LeadsControllerTests()
    {
        _facadeMock = new Mock<ILeadFacade>();
        _sut        = new LeadsController(_facadeMock.Object);
    }

    [Fact]
    public async Task GetAll_DeveRetornarOkComLeads()
    {
        var leads = new List<LeadDto>();
        _facadeMock.Setup(f => f.GetAll()).ReturnsAsync(leads);

        var result = await _sut.GetAll();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(leads);
    }

    [Fact]
    public async Task GetById_QuandoLeadExiste_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var dto = new LeadDto(id, "Nome", "email@test.com", "99999-9999", null, "Site", null, StatusLead.Novo, "Novo", DateTime.UtcNow, null, new List<HistoricoContatoLeadDto>());
        _facadeMock.Setup(f => f.GetById(id)).ReturnsAsync(dto);

        var result = await _sut.GetById(id);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetById_QuandoLeadNaoExiste_DeveRetornarNotFound()
    {
        var id = Guid.NewGuid();
        _facadeMock.Setup(f => f.GetById(id)).ReturnsAsync((LeadDto?)null);

        var result = await _sut.GetById(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_DeveRetornarCreated()
    {
        var command = new CriarLeadCommand("Nome", "email@test.com", "99999-9999", null);
        var dto = new LeadDto(Guid.NewGuid(), "Nome", "email@test.com", "99999-9999", null, null, null, StatusLead.Novo, "Novo", DateTime.UtcNow, null, new List<HistoricoContatoLeadDto>());
        _facadeMock.Setup(f => f.Create(command)).ReturnsAsync(dto);

        var result = await _sut.Create(command);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(LeadsController.GetById));
        createdResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Update_ComIdConsistente_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var command = new AtualizarLeadCommand(id, "Nome", "email@test.com", "99999-9999", null);
        var dto = new LeadDto(id, "Nome", "email@test.com", "99999-9999", null, null, null, StatusLead.Novo, "Novo", DateTime.UtcNow, null, new List<HistoricoContatoLeadDto>());
        _facadeMock.Setup(f => f.Update(command)).ReturnsAsync(dto);

        var result = await _sut.Update(id, command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Update_ComIdInconsistente_DeveRetornarBadRequest()
    {
        var command = new AtualizarLeadCommand(Guid.NewGuid(), "Nome", "email@test.com", "99999-9999", null);

        var result = await _sut.Update(Guid.NewGuid(), command);

        var badResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badResult.Value.Should().Be("ID inconsistente.");
    }

    [Fact]
    public async Task UpdateStatus_ComIdConsistente_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var command = new AtualizarStatusLeadCommand(id, StatusLead.EmContato, null);
        var dto = new LeadDto(id, "Nome", "email@test.com", "99999-9999", null, null, null, StatusLead.EmContato, "Em Contato", DateTime.UtcNow, null, new List<HistoricoContatoLeadDto>());
        _facadeMock.Setup(f => f.UpdateStatus(command)).ReturnsAsync(dto);

        var result = await _sut.UpdateStatus(id, command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task UpdateStatus_ComIdInconsistente_DeveRetornarBadRequest()
    {
        var command = new AtualizarStatusLeadCommand(Guid.NewGuid(), StatusLead.EmContato, null);

        var result = await _sut.UpdateStatus(Guid.NewGuid(), command);

        var badResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badResult.Value.Should().Be("ID inconsistente.");
    }

    [Fact]
    public async Task RegisterContact_ComIdConsistente_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var command = new RegistrarContatoLeadCommand(id, "WhatsApp", "Resumo");
        var dto = new HistoricoContatoLeadDto(Guid.NewGuid(), id, DateTime.UtcNow, "WhatsApp", "Resumo");
        _facadeMock.Setup(f => f.RegisterContact(command)).ReturnsAsync(dto);

        var result = await _sut.RegisterContact(id, command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task RegisterContact_ComIdInconsistente_DeveRetornarBadRequest()
    {
        var command = new RegistrarContatoLeadCommand(Guid.NewGuid(), "WhatsApp", "Resumo");

        var result = await _sut.RegisterContact(Guid.NewGuid(), command);

        var badResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badResult.Value.Should().Be("ID inconsistente.");
    }

    [Fact]
    public async Task Delete_DeveRetornarNoContent()
    {
        var id = Guid.NewGuid();
        _facadeMock.Setup(f => f.Delete(id)).Returns(Task.CompletedTask);

        var result = await _sut.Delete(id);

        result.Should().BeOfType<NoContentResult>();
        _facadeMock.Verify(f => f.Delete(id), Times.Once);
    }
}
