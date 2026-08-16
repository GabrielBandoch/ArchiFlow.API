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

    public S3StorageService(IConfiguration configuration, IAmazonS3? s3Client = null)
    {
        _bucketName = configuration["AWS:BucketName"] ?? "archiflow-uploads";
        _s3Client = s3Client ?? (IAmazonS3)Activator.CreateInstance(typeof(AmazonS3Client))!;
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = uniqueFileName,
            InputStream = fileStream,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(putRequest);

        return $"https://{_bucketName}.s3.amazonaws.com/{uniqueFileName}";
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
