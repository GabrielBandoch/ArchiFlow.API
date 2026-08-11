using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Leads.Commands;
using ArchiFlow.Application.Leads.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Controllers;

public class OrigensLeadControllerTests
{
    private readonly Mock<IOrigemLeadFacade> _facadeMock;
    private readonly OrigensLeadController _sut;

    public OrigensLeadControllerTests()
    {
        _facadeMock = new Mock<IOrigemLeadFacade>();
        _sut        = new OrigensLeadController(_facadeMock.Object);
    }

    [Fact]
    public async Task GetAll_DeveRetornarOkComOrigens()
    {
        var origens = new List<OrigemLeadDto>();
        _facadeMock.Setup(f => f.GetAll()).ReturnsAsync(origens);

        var result = await _sut.GetAll();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(origens);
    }

    [Fact]
    public async Task GetAllActive_DeveRetornarOkComOrigensAtivas()
    {
        var origens = new List<OrigemLeadDto>();
        _facadeMock.Setup(f => f.GetAllActive()).ReturnsAsync(origens);

        var result = await _sut.GetAllActive();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(origens);
    }

    [Fact]
    public async Task GetById_QuandoOrigemExiste_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var dto = new OrigemLeadDto(id, "Instagram", true, DateTime.UtcNow);
        _facadeMock.Setup(f => f.GetById(id)).ReturnsAsync(dto);

        var result = await _sut.GetById(id);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetById_QuandoOrigemNaoExiste_DeveRetornarNotFound()
    {
        var id = Guid.NewGuid();
        _facadeMock.Setup(f => f.GetById(id)).ReturnsAsync((OrigemLeadDto?)null);

        var result = await _sut.GetById(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_DeveRetornarCreated()
    {
        var command = new CriarOrigemLeadCommand("Instagram");
        var dto = new OrigemLeadDto(Guid.NewGuid(), "Instagram", true, DateTime.UtcNow);
        _facadeMock.Setup(f => f.Create(command)).ReturnsAsync(dto);

        var result = await _sut.Create(command);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(OrigensLeadController.GetById));
        createdResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Update_ComIdConsistente_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var command = new AtualizarOrigemLeadCommand(id, "Instagram");
        var dto = new OrigemLeadDto(id, "Instagram", true, DateTime.UtcNow);
        _facadeMock.Setup(f => f.Update(command)).ReturnsAsync(dto);

        var result = await _sut.Update(id, command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Update_ComIdInconsistente_DeveRetornarBadRequest()
    {
        var command = new AtualizarOrigemLeadCommand(Guid.NewGuid(), "Instagram");

        var result = await _sut.Update(Guid.NewGuid(), command);

        var badResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badResult.Value.Should().Be("ID inconsistente.");
    }

    [Fact]
    public async Task Desativar_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var dto = new OrigemLeadDto(id, "Instagram", false, DateTime.UtcNow);
        _facadeMock.Setup(f => f.Desativar(id)).ReturnsAsync(dto);

        var result = await _sut.Desativar(id);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Reativar_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var dto = new OrigemLeadDto(id, "Instagram", true, DateTime.UtcNow);
        _facadeMock.Setup(f => f.Reativar(id)).ReturnsAsync(dto);

        var result = await _sut.Reativar(id);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(dto);
    }
}
