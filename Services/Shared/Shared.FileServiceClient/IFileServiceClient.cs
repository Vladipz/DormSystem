using Microsoft.AspNetCore.Http;
using Shared.FileServiceClient.Models;

namespace Shared.FileServiceClient
{
    public interface IFileServiceClient
    {
        /// <summary>
        /// Uploads a file from a stream with specified category.
        /// </summary>
        /// <param name="fileStream">The file stream to upload.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <param name="category">The category of the file (default: "document").</param>
        /// <returns>File upload result or null if failed.</returns>
        Task<FileUploadResult?> UploadFileAsync(Stream fileStream, string fileName, string contentType, string category = "document");

        /// <summary>
        /// Uploads a file from IFormFile with specified category.
        /// </summary>
        /// <param name="file">The form file to upload.</param>
        /// <param name="category">The category of the file (default: "document").</param>
        /// <returns>File upload result or null if failed.</returns>
        Task<FileUploadResult?> UploadFileAsync(IFormFile file, string category = "document");

        /// <summary>
        /// Uploads an avatar file from a stream.
        /// </summary>
        /// <param name="fileStream">The file stream to upload.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <returns>File upload result or null if failed.</returns>
        Task<FileUploadResult?> UploadAvatarAsync(Stream fileStream, string fileName, string contentType);

        /// <summary>
        /// Uploads an avatar file from IFormFile.
        /// </summary>
        /// <param name="avatarFile">The avatar file to upload.</param>
        /// <returns>File upload result or null if failed.</returns>
        Task<FileUploadResult?> UploadAvatarAsync(IFormFile avatarFile);

        /// <summary>
        /// Deletes a file by its ID.
        /// </summary>
        /// <param name="fileId">The ID of the file to delete.</param>
        /// <returns>True if deleted successfully, false otherwise.</returns>
        Task<bool> DeleteFileAsync(string fileId);

        /// <summary>
        /// Gets the URL for accessing a file by its ID.
        /// </summary>
        /// <param name="fileId">The ID of the file.</param>
        /// <returns>The URL to access the file.</returns>
        string GetFileUrl(string fileId);

        /// <summary>
        /// Downloads a file by its ID.
        /// </summary>
        /// <param name="fileId">The ID of the file to download.</param>
        /// <returns>File data, content type, and file name, or null if not found.</returns>
        Task<(byte[] data, string contentType, string fileName)?> GetFileAsync(string fileId);

        /// <summary>
        /// Gets all files by category.
        /// </summary>
        /// <param name="category">The category to filter by.</param>
        /// <returns>List of files in the specified category.</returns>
        Task<IEnumerable<FileUploadResult>> GetFilesByCategoryAsync(string category);
    }
} 