using ArchiFlow.Application.Arquivos.Commands;
using ArchiFlow.Application.Arquivos.DTOs;
using ArchiFlow.Application.Arquivos.Facades;
using ArchiFlow.Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Facades;

public class ArquivoFacadeTests
{
    private readonly Mock<IArquivoService> _serviceMock;
    private readonly ArquivoFacade _sut;

    public ArquivoFacadeTests()
    {
        _serviceMock = new Mock<IArquivoService>();
        _sut = new ArquivoFacade(_serviceMock.Object);
    }

    [Fact]
    public async Task Upload_DeveDelegarParaService()
    {
        var projetoId = Guid.NewGuid();
        var fileMock = new Mock<IFormFile>();
        var command = new UploadArquivoCommand(projetoId, fileMock.Object, true);
        var dto = new ArquivoDto(Guid.NewGuid(), projetoId, "test.pdf", "http://storage/test.pdf", "application/pdf", true, DateTime.UtcNow);

        _serviceMock.Setup(s => s.Upload(command)).ReturnsAsync(dto);

        var result = await _sut.Upload(command);

        result.Should().BeSameAs(dto);
        _serviceMock.Verify(s => s.Upload(command), Times.Once);
    }

    [Fact]
    public async Task GetByProjetoId_DeveDelegarParaService()
    {
        var projetoId = Guid.NewGuid();
        var lista = new List<ArquivoDto>();
        _serviceMock.Setup(s => s.GetByProjetoId(projetoId, false)).ReturnsAsync(lista);

        var result = await _sut.GetByProjetoId(projetoId);

        result.Should().BeSameAs(lista);
        _serviceMock.Verify(s => s.GetByProjetoId(projetoId, false), Times.Once);
    }

    [Fact]
    public async Task GetByProjetoId_ComApenasVisiveisCliente_DeveDelegarParaService()
    {
        var projetoId = Guid.NewGuid();
        var lista = new List<ArquivoDto>();
        _serviceMock.Setup(s => s.GetByProjetoId(projetoId, true)).ReturnsAsync(lista);

        var result = await _sut.GetByProjetoId(projetoId, true);

        result.Should().BeSameAs(lista);
        _serviceMock.Verify(s => s.GetByProjetoId(projetoId, true), Times.Once);
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
