using Amazon.S3;
using Amazon.S3.Model;
using ArchiFlow.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Services;

public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _region;

    public S3StorageService(IConfiguration configuration, IAmazonS3? s3Client = null)
    {
        _bucketName = configuration["AWS:BucketName"] 
                   ?? configuration["AWS_S3_BUCKET"]
                   ?? Environment.GetEnvironmentVariable("AWS_S3_BUCKET") 
                   ?? throw new InvalidOperationException("Nome do bucket S3 não configurado. Defina a variável 'AWS_S3_BUCKET'.");

        _region = configuration["AWS:Region"]
               ?? configuration["AWS_REGION"]
               ?? Environment.GetEnvironmentVariable("AWS_REGION")
               ?? "us-east-1";

        _s3Client = s3Client ?? throw new InvalidOperationException("Cliente AmazonS3 não injetado.");
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        var safeFileName = Path.GetFileName(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";
        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = uniqueFileName,
            InputStream = fileStream,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(putRequest);

        return $"https://{_bucketName}.s3.{_region}.amazonaws.com/{uniqueFileName}";
    }

    public async Task DeleteAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return;

        try
        {
            var uri = new Uri(fileUrl);
            var key = uri.AbsolutePath.TrimStart('/');

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);
        }
        catch (UriFormatException)
        {
            // URL inválida ou não formatada no padrão HTTP
        }
    }
}
