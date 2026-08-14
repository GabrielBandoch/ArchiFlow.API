using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Application.Leads.Commands;
using ArchiFlow.Application.Leads.DTOs;
using ArchiFlow.Application.Leads.Facades;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Facades;

public class OrigemLeadFacadeTests
{
    private readonly Mock<IOrigemLeadService> _serviceMock;
    private readonly OrigemLeadFacade _sut;

    public OrigemLeadFacadeTests()
    {
        _serviceMock = new Mock<IOrigemLeadService>();
        _sut         = new OrigemLeadFacade(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_DeveDelegarParaService()
    {
        var dtos = new List<OrigemLeadDto>();
        _serviceMock.Setup(s => s.GetAll()).ReturnsAsync(dtos);

        var result = await _sut.GetAll();

        result.Should().BeSameAs(dtos);
        _serviceMock.Verify(s => s.GetAll(), Times.Once);
    }

    [Fact]
    public async Task GetAllActive_DeveDelegarParaService()
    {
        var dtos = new List<OrigemLeadDto>();
        _serviceMock.Setup(s => s.GetAllActive()).ReturnsAsync(dtos);

        var result = await _sut.GetAllActive();

        result.Should().BeSameAs(dtos);
        _serviceMock.Verify(s => s.GetAllActive(), Times.Once);
    }

    [Fact]
    public async Task GetById_DeveDelegarParaService()
    {
        var id = Guid.NewGuid();
        var dto = new OrigemLeadDto(id, "Instagram", true, DateTime.UtcNow);
        _serviceMock.Setup(s => s.GetById(id)).ReturnsAsync(dto);

        var result = await _sut.GetById(id);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.GetById(id), Times.Once);
    }

    [Fact]
    public async Task Create_DeveDelegarParaService()
    {
        var command = new CriarOrigemLeadCommand("Instagram");
        var dto = new OrigemLeadDto(Guid.NewGuid(), "Instagram", true, DateTime.UtcNow);
        _serviceMock.Setup(s => s.Create(command)).ReturnsAsync(dto);

        var result = await _sut.Create(command);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.Create(command), Times.Once);
    }

    [Fact]
    public async Task Update_DeveDelegarParaService()
    {
        var command = new AtualizarOrigemLeadCommand(Guid.NewGuid(), "Instagram");
        var dto = new OrigemLeadDto(command.Id, "Instagram", true, DateTime.UtcNow);
        _serviceMock.Setup(s => s.Update(command)).ReturnsAsync(dto);

        var result = await _sut.Update(command);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.Update(command), Times.Once);
    }

    [Fact]
    public async Task Desativar_DeveDelegarParaService()
    {
        var id = Guid.NewGuid();
        var dto = new OrigemLeadDto(id, "Instagram", false, DateTime.UtcNow);
        _serviceMock.Setup(s => s.Desativar(id)).ReturnsAsync(dto);

        var result = await _sut.Desativar(id);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.Desativar(id), Times.Once);
    }

    [Fact]
    public async Task Reativar_DeveDelegarParaService()
    {
        var id = Guid.NewGuid();
        var dto = new OrigemLeadDto(id, "Instagram", true, DateTime.UtcNow);
        _serviceMock.Setup(s => s.Reativar(id)).ReturnsAsync(dto);

        var result = await _sut.Reativar(id);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.Reativar(id), Times.Once);
    }
}
