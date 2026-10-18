using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Shared;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Services;

public class LocalStorageService : IStorageService
{
    private readonly string _rootPath;
    private readonly ITenantContext? _tenantContext;

    public LocalStorageService(string rootPath) : this(null, rootPath) { }

    public LocalStorageService(ITenantContext? tenantContext = null, string? rootPath = null)
    {
        _tenantContext = tenantContext;
        _rootPath = rootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    }

    public Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        return UploadAsync(fileStream, fileName, contentType, "geral", null);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string category, string? subPath = null)
    {
        var tenantId = _tenantContext?.TenantId ?? "default";
        var safeCategory = string.IsNullOrWhiteSpace(category) ? "geral" : SanitizePathPart(category);
        
        var relativeFolder = Path.Combine("uploads", "tenants", tenantId, safeCategory);
        if (!string.IsNullOrWhiteSpace(subPath))
        {
            relativeFolder = Path.Combine(relativeFolder, SanitizePathPart(subPath));
        }

        var fullFolderPath = Path.GetFullPath(Path.Combine(_rootPath, relativeFolder));
        if (!Directory.Exists(fullFolderPath))
        {
            Directory.CreateDirectory(fullFolderPath);
        }

        var safeFileName = Path.GetFileName(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";
        var filePath = Path.GetFullPath(Path.Combine(fullFolderPath, uniqueFileName));

        if (!filePath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Caminho de arquivo inválido.");
        }

        using (var destinationStream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(destinationStream);
        }

        var urlPath = "/" + Path.Combine(relativeFolder, uniqueFileName).Replace('\\', '/');
        return urlPath;
    }

    public Task DeleteAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return Task.CompletedTask;

        var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var filePath = Path.GetFullPath(Path.Combine(_rootPath, relativePath));

        if (filePath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase) && File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        else
        {
            var safeFileName = Path.GetFileName(fileUrl);
            var flatPath = Path.GetFullPath(Path.Combine(_rootPath, "uploads", safeFileName));
            if (flatPath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase) && File.Exists(flatPath))
            {
                File.Delete(flatPath);
            }
        }

        return Task.CompletedTask;
    }

    public Task<Stream?> DownloadAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return Task.FromResult<Stream?>(null);

        var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var filePath = Path.GetFullPath(Path.Combine(_rootPath, relativePath));

        if (filePath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase) && File.Exists(filePath))
        {
            Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult<Stream?>(stream);
        }

        var safeFileName = Path.GetFileName(fileUrl);
        var flatPath = Path.GetFullPath(Path.Combine(_rootPath, "uploads", safeFileName));
        if (flatPath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase) && File.Exists(flatPath))
        {
            Stream stream = new FileStream(flatPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult<Stream?>(stream);
        }

        return Task.FromResult<Stream?>(null);
    }

    public string ObterChaveArquivo(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return string.Empty;
        return fileUrl.TrimStart('/');
    }

    private static string SanitizePathPart(string part)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", part.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).Trim();
    }
}
