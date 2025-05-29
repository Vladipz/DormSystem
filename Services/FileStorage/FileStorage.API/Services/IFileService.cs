using FileStorage.API.Models;

namespace FileStorage.API.Services
{
    public interface IFileService
    {
        Task<FileMetadata> SaveFileAsync(IFormFile file, string category);
        Task<(byte[] data, string contentType, string fileName)> GetFileAsync(string id);
        Task<bool> DeleteFileAsync(string id);
        Task<IEnumerable<FileMetadata>> GetFilesByCategory(string category);
    }
}