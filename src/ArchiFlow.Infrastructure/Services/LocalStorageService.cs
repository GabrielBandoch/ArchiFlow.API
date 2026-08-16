using ArchiFlow.Application.Interfaces.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Services;

public class LocalStorageService : IStorageService
{
    private readonly string _rootPath;

    public LocalStorageService(string? rootPath = null)
    {
        _rootPath = rootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        var uploadsFolder = Path.Combine(_rootPath, "uploads");
        
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var destinationStream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(destinationStream);
        }

        return $"/uploads/{uniqueFileName}";
    }

    public Task DeleteAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return Task.CompletedTask;

        var fileName = Path.GetFileName(fileUrl);
        var filePath = Path.Combine(_rootPath, "uploads", fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}
