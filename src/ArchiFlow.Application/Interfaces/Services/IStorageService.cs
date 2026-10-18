using System.IO;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Interfaces.Services;

public interface IStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType);
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string category, string? subPath = null);
    Task DeleteAsync(string fileUrl);
    Task<Stream?> DownloadAsync(string fileUrl);
    string ObterChaveArquivo(string fileUrl);
}
