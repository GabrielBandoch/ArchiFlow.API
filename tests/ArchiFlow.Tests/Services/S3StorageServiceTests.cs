using Amazon.S3;
using Amazon.S3.Model;
using ArchiFlow.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Services;

public class S3StorageServiceTests
{
    private readonly Mock<IAmazonS3> _mockS3;

    public S3StorageServiceTests()
    {
        _mockS3 = new Mock<IAmazonS3>();
    }

    [Fact]
    public async Task UploadAsync_Should_Put_Object_In_S3_And_Return_Url()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"AWS:BucketName", "archiflow-uploads"}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var service = new S3StorageService(configuration, _mockS3.Object);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("test s3"));

        // Act
        var url = await service.UploadAsync(stream, "planta.dwg", "application/acad");

        // Assert
        url.Should().StartWith("https://archiflow-uploads.s3.amazonaws.com/");
        url.Should().Contain("planta.dwg");
        _mockS3.Verify(s => s.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_Object_From_S3()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var service = new S3StorageService(configuration, _mockS3.Object);

        // Act
        await service.DeleteAsync("https://archiflow-uploads.s3.amazonaws.com/123_foto.jpg");

        // Assert
        _mockS3.Verify(s => s.DeleteObjectAsync(It.Is<DeleteObjectRequest>(r => r.Key == "123_foto.jpg"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Should_Handle_Invalid_Uri_Gracefully()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var service = new S3StorageService(configuration, _mockS3.Object);

        // Act
        var act = async () => await service.DeleteAsync("invalid-uri-not-a-url");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteAsync_Should_Not_Throw_When_Url_Is_Null_Or_Empty()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var service = new S3StorageService(configuration, _mockS3.Object);

        // Act
        var act = async () => await service.DeleteAsync(string.Empty);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
