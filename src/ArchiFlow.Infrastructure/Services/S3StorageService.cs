using Amazon.S3;
using Amazon.S3.Model;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Shared;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Services;

public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly ITenantContext? _tenantContext;

    public S3StorageService(IConfiguration configuration, IAmazonS3 s3Client, ITenantContext? tenantContext = null)
        : this(configuration, tenantContext, s3Client)
    {
    }

    public S3StorageService(IConfiguration configuration, ITenantContext? tenantContext = null, IAmazonS3? s3Client = null)
    {
        _tenantContext = tenantContext;
        _bucketName = configuration["AWS_S3_BUCKET"] 
            ?? Environment.GetEnvironmentVariable("AWS_S3_BUCKET")
            ?? configuration["AWS:BucketName"] 
            ?? "cotu-storage";
        _s3Client = s3Client ?? (IAmazonS3)Activator.CreateInstance(typeof(AmazonS3Client))!;
    }

    public Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        return UploadAsync(fileStream, fileName, contentType, "geral", null);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string category, string? subPath = null)
    {
        var tenantId = _tenantContext?.TenantId ?? "default";
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
        if (string.IsNullOrEmpty(fileUrl)) return;

        try
        {
            var key = ObterChaveArquivo(fileUrl);
            if (string.IsNullOrEmpty(key)) return;

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);
        }
        catch (Exception)
        {
        }
    }

    public async Task<Stream?> DownloadAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return null;

        try
        {
            var key = ObterChaveArquivo(fileUrl);
            if (string.IsNullOrEmpty(key)) return null;

            var getRequest = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(getRequest);
            return response.ResponseStream;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public string ObterChaveArquivo(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return string.Empty;

        if (Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
        {
            return uri.AbsolutePath.TrimStart('/');
        }

        return fileUrl.TrimStart('/');
    }

    private static string SanitizePathPart(string part)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", part.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).Trim();
    }
}
