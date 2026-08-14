using ArchiFlow.Application.Arquivos.Commands;
using ArchiFlow.Application.Arquivos.Services;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Shared;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Services;

public class ArquivoServiceTests
{
    private readonly Mock<IArquivoRepository> _mockRepo;
    private readonly Mock<IStorageService> _mockStorage;
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly ArquivoService _service;

    public ArquivoServiceTests()
    {
        _mockRepo = new Mock<IArquivoRepository>();
        _mockStorage = new Mock<IStorageService>();
        _mockUow = new Mock<IUnitOfWork>();
        _service = new ArquivoService(_mockRepo.Object, _mockStorage.Object, _mockUow.Object);
    }

    [Fact]
    public async Task GetByProjetoId_Should_Return_Mapped_Dtos()
    {
        // Arrange
        var projetoId = Guid.NewGuid();
        var lista = new List<Arquivo>
        {
            new Arquivo
            {
                Id = Guid.NewGuid(),
                ProjetoId = projetoId,
                Nome = "Planta_Baixa.pdf",
                UrlStorage = "https://s3.amazonaws.com/Planta_Baixa.pdf",
                Tipo = "application/pdf",
                VisivelCliente = true,
                CriadoEm = DateTime.UtcNow
            }
        };

        _mockRepo.Setup(r => r.GetByProjetoId(projetoId)).ReturnsAsync(lista);

        // Act
        var result = await _service.GetByProjetoId(projetoId);

        // Assert
        result.Should().HaveCount(1);
        var dto = result.First();
        dto.Nome.Should().Be("Planta_Baixa.pdf");
        dto.VisivelCliente.Should().BeTrue();
    }

    [Fact]
    public async Task Upload_Should_Throw_When_Stream_Is_Empty()
    {
        // Arrange
        var command = new UploadArquivoCommand(
            Guid.NewGuid(),
            "empty.txt",
            "text/plain",
            0,
            Stream.Null,
            true
        );

        // Act
        var act = async () => await _service.Upload(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Nenhum arquivo enviado.");
    }

    [Fact]
    public async Task Upload_Should_Throw_When_Length_Exceeds_20MB()
    {
        // Arrange
        using var stream = new MemoryStream(new byte[10]);
        var command = new UploadArquivoCommand(
            Guid.NewGuid(),
            "huge.zip",
            "application/zip",
            21 * 1024 * 1024,
            stream,
            true
        );

        // Act
        var act = async () => await _service.Upload(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("O arquivo excede o limite de tamanho permitido de 20MB.");
    }

    [Fact]
    public async Task Upload_Should_Save_To_Storage_And_Repository_Successfully()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("hello"));
        var command = new UploadArquivoCommand(
            Guid.NewGuid(),
            "render.png",
            "image/png",
            5,
            stream,
            true
        );

        _mockStorage.Setup(s => s.UploadAsync(stream, "render.png", "image/png"))
            .ReturnsAsync("https://s3.amazonaws.com/render.png");

        // Act
        var result = await _service.Upload(command);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be("render.png");
        result.UrlStorage.Should().Be("https://s3.amazonaws.com/render.png");
        _mockRepo.Verify(r => r.Create(It.IsAny<Arquivo>()), Times.Once);
        _mockUow.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_Should_Throw_When_Arquivo_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetById(id)).ReturnsAsync((Arquivo?)null);

        // Act
        var act = async () => await _service.Delete(id);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Delete_Should_Remove_From_Storage_And_Repository()
    {
        // Arrange
        var id = Guid.NewGuid();
        var arquivo = new Arquivo
        {
            Id = id,
            UrlStorage = "https://s3.amazonaws.com/render.png"
        };
        _mockRepo.Setup(r => r.GetById(id)).ReturnsAsync(arquivo);

        // Act
        await _service.Delete(id);

        // Assert
        _mockStorage.Verify(s => s.DeleteAsync(arquivo.UrlStorage), Times.Once);
        _mockRepo.Verify(r => r.Delete(id), Times.Once);
        _mockUow.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }
}
