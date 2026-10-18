using Amazon.S3;
using Amazon.S3.Model;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Services;

public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly ITenantContext? _tenantContext;
    private readonly ILogger<S3StorageService>? _logger;

    public S3StorageService(
        IConfiguration configuration, 
        IAmazonS3 s3Client, 
        ITenantContext? tenantContext = null,
        ILogger<S3StorageService>? logger = null)
    {
        _s3Client = s3Client;
        _tenantContext = tenantContext;
        _logger = logger;
        _bucketName = configuration["AWS_S3_BUCKET"] 
            ?? Environment.GetEnvironmentVariable("AWS_S3_BUCKET")
            ?? configuration["AWS:BucketName"] 
            ?? "cotu-storage";
    }

    public Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        return UploadAsync(fileStream, fileName, contentType, "geral", null);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string category, string? subPath = null)
    {
        var tenantId = SanitizePathPart(_tenantContext?.TenantId ?? "default");
        var safeCategory = string.IsNullOrWhiteSpace(category) ? "geral" : SanitizePathPart(category);
        var safeFileName = Path.GetFileName(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";

        var key = $"tenants/{tenantId}/{safeCategory}";
        if (!string.IsNullOrWhiteSpace(subPath))
        {
            key += $"/{SanitizePathPart(subPath)}";
        }
        key += $"/{uniqueFileName}";

        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(putRequest);

        return $"https://{_bucketName}.s3.amazonaws.com/{key}";
    }

    public async Task DeleteAsync(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return;

        var key = ObterChaveArquivo(fileUrl);
        if (string.IsNullOrEmpty(key)) return;

        ValidarIsolamentoTenant(key);

        try
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Falha ao excluir objeto S3 com chave {Key}", key);
        }
    }

    public async Task<Stream?> DownloadAsync(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return null;

        var key = ObterChaveArquivo(fileUrl);
        if (string.IsNullOrEmpty(key)) return null;

        ValidarIsolamentoTenant(key);

        try
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(getRequest);
            return response.ResponseStream;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Falha ao obter objeto S3 com chave {Key}", key);
            return null;
        }
    }

    public string ObterChaveArquivo(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return string.Empty;

        if (Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
        {
            return uri.AbsolutePath.TrimStart('/');
        }

        return fileUrl.TrimStart('/');
    }

    private void ValidarIsolamentoTenant(string key)
    {
        if (key.Contains(".."))
        {
            throw new InvalidOperationException("Chave de objeto S3 inválida.");
        }

        var normalized = key.Replace('\\', '/').TrimStart('/');
        if (normalized.StartsWith("tenants/", StringComparison.OrdinalIgnoreCase))
        {
            var currentTenantId = _tenantContext?.TenantId;
            var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
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
    }

    private static string SanitizePathPart(string part)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", part.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).Trim();
    }
}
