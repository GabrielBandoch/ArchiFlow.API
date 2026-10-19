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
        var tenantId = SanitizePathPart(_tenantContext?.TenantId ?? "default");
        var safeCategory = string.IsNullOrWhiteSpace(category) ? "geral" : SanitizePathPart(category);
        
        var relativeFolder = Path.Combine("uploads", "tenants", tenantId, safeCategory);
        if (!string.IsNullOrWhiteSpace(subPath))
        {
            relativeFolder = Path.Combine(relativeFolder, SanitizePathPart(subPath));
        }

        var fullFolderPath = Path.GetFullPath(Path.Combine(_rootPath, relativeFolder));
        if (!fullFolderPath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Caminho de pasta inválido.");
        }

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

        var relativeSegments = Path.Combine(relativeFolder, uniqueFileName).Replace('\\', '/');
        return $"/{relativeSegments}";
    }

    public Task DeleteAsync(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return Task.CompletedTask;

        var (isValid, filePath) = ResolverEValidarCaminho(fileUrl);
        if (!isValid || filePath == null) return Task.CompletedTask;

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    public Task<Stream?> DownloadAsync(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return Task.FromResult<Stream?>(null);

        var (isValid, filePath) = ResolverEValidarCaminho(fileUrl);
        if (!isValid || filePath == null || !File.Exists(filePath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public string ObterChaveArquivo(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return string.Empty;
        return fileUrl.TrimStart('/');
    }

    private (bool isValid, string? fullPath) ResolverEValidarCaminho(string fileUrl)
    {
        var normalizedUrl = fileUrl.Trim().TrimStart('/');
        if (normalizedUrl.Contains(".."))
        {
            throw new InvalidOperationException("Tentativa de travessia de diretório detectada.");
        }

        var currentTenantId = _tenantContext?.TenantId;
        var normalizedSlash = normalizedUrl.Replace('\\', '/');

        if (normalizedSlash.StartsWith("uploads/tenants/", StringComparison.OrdinalIgnoreCase) ||
            normalizedSlash.StartsWith("tenants/", StringComparison.OrdinalIgnoreCase))
        {
            var cleanPrefix = normalizedSlash.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase)
                ? normalizedSlash["uploads/".Length..]
                : normalizedSlash;

            var segments = cleanPrefix.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 2)
            {
                var fileTenantId = segments[1];
                if (!string.IsNullOrWhiteSpace(currentTenantId) &&
                    !string.Equals(currentTenantId, fileTenantId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new UnauthorizedAccessException("Acesso negado: o recurso pertence a outro tenant.");
                }
            }
        }

        var relativePath = normalizedUrl.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, relativePath));

        if (!fullPath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Caminho fora da raiz permitida.");
        }

        if (File.Exists(fullPath))
        {
            return (true, fullPath);
        }

        var safeFileName = Path.GetFileName(fileUrl);
        var legacyFlatPath = Path.GetFullPath(Path.Combine(_rootPath, "uploads", safeFileName));
        if (legacyFlatPath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase) && File.Exists(legacyFlatPath))
        {
            return (true, legacyFlatPath);
        }

        return (true, fullPath);
    }

    private static string SanitizePathPart(string part)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", part.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).Trim();
    }
}
