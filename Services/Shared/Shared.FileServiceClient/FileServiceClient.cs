using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.FileServiceClient.Models;

namespace Shared.FileServiceClient
{
    public class FileServiceClient : IFileServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FileServiceClient> _logger;
        private readonly FileStorageSettings _settings;

        public FileServiceClient(
            HttpClient httpClient,
            IOptions<FileStorageSettings> settings,
            ILogger<FileServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task<FileUploadResult?> UploadFileAsync(Stream fileStream, string fileName, string contentType, string category = "document")
        {
            try
            {
                using var content = new MultipartFormDataContent();

                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                content.Add(fileContent, "File", fileName);
                content.Add(new StringContent(category), "Category");

                var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/files/upload", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<FileUploadResult>(responseContent, options);
                }

                _logger.LogError("Failed to upload file. Status: {StatusCode}, Content: {Content}",
                    response.StatusCode, await response.Content.ReadAsStringAsync());
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
                return null;
            }
        }

        public async Task<FileUploadResult?> UploadFileAsync(IFormFile file, string category = "document")
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Attempted to upload null or empty file");
                return null;
            }

            using var stream = file.OpenReadStream();
            return await UploadFileAsync(stream, file.FileName, file.ContentType, category);
        }

        public async Task<FileUploadResult?> UploadAvatarAsync(Stream fileStream, string fileName, string contentType)
        {
            return await UploadFileAsync(fileStream, fileName, contentType, "avatar");
        }

        public async Task<FileUploadResult?> UploadAvatarAsync(IFormFile avatarFile)
        {
            return await UploadFileAsync(avatarFile, "avatar");
        }

        public async Task<bool> DeleteFileAsync(string fileId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_settings.BaseUrl}/api/files/{fileId}");
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully deleted file with ID: {FileId}", fileId);
                    return true;
                }

                _logger.LogWarning("Failed to delete file with ID: {FileId}. Status: {StatusCode}", 
                    fileId, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file with ID: {FileId}", fileId);
                return false;
            }
        }

        public string GetFileUrl(string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                _logger.LogWarning("Attempted to get URL for null or empty file ID");
                return string.Empty;
            }

            return $"{_settings.BaseUrl}/api/files/{fileId}";
        }

        public async Task<(byte[] data, string contentType, string fileName)?> GetFileAsync(string fileId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/api/files/{fileId}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsByteArrayAsync();
                    var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
                    var fileName = ExtractFileNameFromResponse(response) ?? "unknown";

                    return (data, contentType, fileName);
                }

                _logger.LogWarning("Failed to get file with ID: {FileId}. Status: {StatusCode}", 
                    fileId, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file with ID: {FileId}", fileId);
                return null;
            }
        }

        public async Task<IEnumerable<FileUploadResult>> GetFilesByCategoryAsync(string category)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/api/files/category/{category}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<IEnumerable<FileUploadResult>>(responseContent, options) 
                           ?? Enumerable.Empty<FileUploadResult>();
                }

                _logger.LogWarning("Failed to get files for category: {Category}. Status: {StatusCode}", 
                    category, response.StatusCode);
                return Enumerable.Empty<FileUploadResult>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting files for category: {Category}", category);
                return Enumerable.Empty<FileUploadResult>();
            }
        }

        private static string? ExtractFileNameFromResponse(HttpResponseMessage response)
        {
            var contentDisposition = response.Content.Headers.ContentDisposition;
            return contentDisposition?.FileName?.Trim('"');
        }
    }
} 