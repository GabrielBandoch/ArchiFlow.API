using ArchiFlow.API.Controllers;
using ArchiFlow.Application.Arquivos.Commands;
using ArchiFlow.Application.Arquivos.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Controllers;

public class ArquivosControllerTests
{
    private readonly Mock<IArquivoFacade> _mockFacade;
    private readonly ArquivosController _controller;

    public ArquivosControllerTests()
    {
        _mockFacade = new Mock<IArquivoFacade>();
        _controller = new ArquivosController(_mockFacade.Object);
    }

    [Fact]
    public async Task GetByProjeto_Should_Return_Ok_With_List()
    {
        // Arrange
        var projetoId = Guid.NewGuid();
        var lista = new List<ArquivoDto>
        {
            new ArquivoDto(Guid.NewGuid(), projetoId, "documento.pdf", "https://s3.com/doc.pdf", "application/pdf", true, DateTime.UtcNow)
        };
        _mockFacade.Setup(f => f.GetByProjetoId(projetoId)).ReturnsAsync(lista);

        // Act
        var result = await _controller.GetByProjeto(projetoId);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(lista);
    }

    [Fact]
    public async Task Upload_Should_Return_Ok_When_Command_Is_Processed()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var projetoId = Guid.NewGuid();
        var command = new UploadArquivoCommand(projetoId, fileMock.Object, true);
        var dto = new ArquivoDto(Guid.NewGuid(), projetoId, "planta.pdf", "https://s3.com/planta.pdf", "application/pdf", true, DateTime.UtcNow);

        _mockFacade.Setup(f => f.Upload(command))
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.Upload(command);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task Delete_Should_Return_NoContent()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = await _controller.Delete(id);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockFacade.Verify(f => f.Delete(id), Times.Once);
    }
}
