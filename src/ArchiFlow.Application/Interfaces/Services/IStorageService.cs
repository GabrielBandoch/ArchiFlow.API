using System.IO;
using System.Threading.Tasks;

namespace ArchiFlow.Application.Interfaces.Services;

public interface IStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType);
    Task DeleteAsync(string fileUrl);
}
