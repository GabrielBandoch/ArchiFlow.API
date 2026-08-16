using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Clientes.Commands;
using ArchiFlow.Application.Clientes.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Controllers;

public class ClientesControllerTests
{
    private readonly Mock<IClienteFacade> _facadeMock;
    private readonly ClientesController _sut;

    public ClientesControllerTests()
    {
        _facadeMock = new Mock<IClienteFacade>();
        _sut = new ClientesController(_facadeMock.Object);
    }

    [Fact]
    public async Task GetAll_DeveRetornarOkComClientes()
    {
        var clientes = new List<ClienteDto>();
        _facadeMock.Setup(f => f.GetAll()).ReturnsAsync(clientes);

        var result = await _sut.GetAll();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(clientes);
    }

    [Fact]
    public async Task GetById_QuandoExiste_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var dto = new ClienteDto(id, null, "Nome", "email@teste.com", null, null, null, true, 0, null);
        _facadeMock.Setup(f => f.GetById(id)).ReturnsAsync(dto);

        var result = await _sut.GetById(id);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetById_QuandoNaoExiste_DeveRetornarNotFound()
    {
        _facadeMock.Setup(f => f.GetById(It.IsAny<Guid>())).ReturnsAsync((ClienteDto?)null);

        var result = await _sut.GetById(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ConvertLead_DeveRetornarOkComResposta()
    {
        var command = new ConvertLeadToClienteCommand(Guid.NewGuid());
        var response = new ConversaoClienteResponseDto(
            new ClienteDto(Guid.NewGuid(), command.LeadId, "Nome", "email@teste.com", null, null, null, true, 0, null),
            "Arch@1234"
        );
        _facadeMock.Setup(f => f.ConvertLead(command)).ReturnsAsync(response);

        var result = await _sut.ConvertLead(command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task Update_DeveRetornarOkComClienteAtualizado()
    {
        var id = Guid.NewGuid();
        var command = new AtualizarClienteCommand(id, "Novo Nome", "novo@email.com", null, null, null, null);
        var dto = new ClienteDto(id, null, "Novo Nome", "novo@email.com", null, null, null, true, 0, null);
        _facadeMock.Setup(f => f.Update(command)).ReturnsAsync(dto);

        var result = await _sut.Update(command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task UpdatePortalAccess_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var command = new AtualizarPortalAccessCommand(id, false);
        var dto = new ClienteDto(id, null, "Nome", "email@teste.com", null, null, null, false, 0, null);
        _facadeMock.Setup(f => f.UpdatePortalAccess(command)).ReturnsAsync(dto);

        var result = await _sut.UpdatePortalAccess(command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }
}
