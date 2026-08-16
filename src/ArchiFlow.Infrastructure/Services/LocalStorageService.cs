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
        var uploadsFolder = Path.GetFullPath(Path.Combine(_rootPath, "uploads"));
        
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var safeFileName = Path.GetFileName(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";
        var filePath = Path.GetFullPath(Path.Combine(uploadsFolder, uniqueFileName));

        if (!filePath.StartsWith(uploadsFolder, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Caminho de arquivo inválido.");
        }

        using (var destinationStream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(destinationStream);
        }

        return $"/uploads/{uniqueFileName}";
    }

    public Task DeleteAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return Task.CompletedTask;

        var safeFileName = Path.GetFileName(fileUrl);
        var uploadsFolder = Path.GetFullPath(Path.Combine(_rootPath, "uploads"));
        var filePath = Path.GetFullPath(Path.Combine(uploadsFolder, safeFileName));

        if (!filePath.StartsWith(uploadsFolder, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}
