using ArchiFlow.Infrastructure.Services;
using FluentAssertions;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Services;

public class LocalStorageServiceTests
{
    private readonly string _tempWebRoot;

    public LocalStorageServiceTests()
    {
        _tempWebRoot = Path.Combine(Path.GetTempPath(), "archiflow_test_uploads_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempWebRoot);
    }

    [Fact]
    public async Task UploadAsync_Should_Create_File_And_Return_Url()
    {
        var service = new LocalStorageService(_tempWebRoot);
        var content = "test file content";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var fileName = "planta.pdf";
        var contentType = "application/pdf";

        var url = await service.UploadAsync(stream, fileName, contentType);

        url.Should().StartWith("/uploads/");
        url.Should().Contain(fileName);
        
        var relativePath = url.TrimStart('/');
        var fullPath = Path.Combine(_tempWebRoot, relativePath);
        File.Exists(fullPath).Should().BeTrue();

        if (Directory.Exists(_tempWebRoot))
        {
            Directory.Delete(_tempWebRoot, true);
        }
    }

    [Fact]
    public void UploadAsync_DefaultConstructor_Should_Work()
    {
        var service = new LocalStorageService();
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_File_When_Exists()
    {
        var service = new LocalStorageService(_tempWebRoot);
        var uploadsDir = Path.Combine(_tempWebRoot, "uploads");
        Directory.CreateDirectory(uploadsDir);
        var testFilePath = Path.Combine(uploadsDir, "delete_me.txt");
        await File.WriteAllTextAsync(testFilePath, "delete content");

        var fileUrl = "/uploads/delete_me.txt";

        await service.DeleteAsync(fileUrl);

        File.Exists(testFilePath).Should().BeFalse();

        if (Directory.Exists(_tempWebRoot))
        {
            Directory.Delete(_tempWebRoot, true);
        }
    }

    [Fact]
    public async Task DeleteAsync_Should_Not_Throw_When_Url_Is_Empty()
    {
        var service = new LocalStorageService(_tempWebRoot);

        var act = async () => await service.DeleteAsync(string.Empty);

        await act.Should().NotThrowAsync();
    }
}
