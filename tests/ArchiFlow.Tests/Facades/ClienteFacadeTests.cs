using ArchiFlow.Application.Clientes.Commands;
using ArchiFlow.Application.Clientes.DTOs;
using ArchiFlow.Application.Clientes.Facades;
using ArchiFlow.Application.Interfaces.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Facades;

public class ClienteFacadeTests
{
    private readonly Mock<IClienteService> _serviceMock;
    private readonly ClienteFacade _sut;

    public ClienteFacadeTests()
    {
        _serviceMock = new Mock<IClienteService>();
        _sut = new ClienteFacade(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_DeveDelegarParaService()
    {
        var dtos = new List<ClienteDto>();
        _serviceMock.Setup(s => s.GetAll()).ReturnsAsync(dtos);

        var result = await _sut.GetAll();

        result.Should().BeSameAs(dtos);
        _serviceMock.Verify(s => s.GetAll(), Times.Once);
    }

    [Fact]
    public async Task GetById_DeveDelegarParaService()
    {
        var id = Guid.NewGuid();
        var dto = new ClienteDto(id, null, "Nome", "email@teste.com", null, null, null, true, 0, null);
        _serviceMock.Setup(s => s.GetById(id)).ReturnsAsync(dto);

        var result = await _sut.GetById(id);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.GetById(id), Times.Once);
    }

    [Fact]
    public async Task ConvertLead_DeveDelegarParaService()
    {
        var command = new ConvertLeadToClienteCommand(Guid.NewGuid());
        var response = new ConversaoClienteResponseDto(
            new ClienteDto(Guid.NewGuid(), command.LeadId, "Nome", "email@teste.com", null, null, null, true, 0, null),
            "Arch@1234"
        );
        _serviceMock.Setup(s => s.ConvertLead(command)).ReturnsAsync(response);

        var result = await _sut.ConvertLead(command);

        result.Should().BeSameAs(response);
        _serviceMock.Verify(s => s.ConvertLead(command), Times.Once);
    }

    [Fact]
    public async Task Update_DeveDelegarParaService()
    {
        var command = new AtualizarClienteCommand(Guid.NewGuid(), "Nome", "email@teste.com", null, null, null, null);
        var dto = new ClienteDto(command.Id, null, "Nome", "email@teste.com", null, null, null, true, 0, null);
        _serviceMock.Setup(s => s.Update(command)).ReturnsAsync(dto);

        var result = await _sut.Update(command);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.Update(command), Times.Once);
    }

    [Fact]
    public async Task UpdatePortalAccess_DeveDelegarParaService()
    {
        var command = new AtualizarPortalAccessCommand(Guid.NewGuid(), true);
        var dto = new ClienteDto(command.Id, null, "Nome", "email@teste.com", null, null, null, true, 0, null);
        _serviceMock.Setup(s => s.UpdatePortalAccess(command)).ReturnsAsync(dto);

        var result = await _sut.UpdatePortalAccess(command);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.UpdatePortalAccess(command), Times.Once);
    }
}
