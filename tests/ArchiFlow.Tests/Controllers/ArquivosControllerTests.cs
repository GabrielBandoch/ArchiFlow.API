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
    public async Task Upload_Should_Return_BadRequest_When_File_Is_Null()
    {
        // Act
        var result = await _controller.Upload(null!, Guid.NewGuid(), true);

        // Assert
        var badRequest = result as BadRequestObjectResult;
        badRequest.Should().NotBeNull();
        badRequest!.Value.Should().Be("Nenhum arquivo enviado.");
    }

    [Fact]
    public async Task Upload_Should_Return_Ok_When_File_Is_Valid()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var content = "dummy pdf content";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.FileName).Returns("planta.pdf");
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");
        fileMock.Setup(f => f.Length).Returns(content.Length);

        var projetoId = Guid.NewGuid();
        var dto = new ArquivoDto(Guid.NewGuid(), projetoId, "planta.pdf", "https://s3.com/planta.pdf", "application/pdf", true, DateTime.UtcNow);

        _mockFacade.Setup(f => f.Upload(It.IsAny<UploadArquivoCommand>()))
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.Upload(fileMock.Object, projetoId, true);

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
