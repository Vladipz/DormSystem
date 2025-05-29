// Services/FileService.cs
using FileStorage.API.Configuration;
using FileStorage.API.Models;

using Microsoft.Extensions.Options;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

using System.Text.Json;

namespace FileStorage.API.Services
{
    public class FileService : IFileService
    {
        private readonly FileStorageSettings _settings;
        private readonly IWebHostEnvironment _environment;
        private readonly string _metadataPath;

        public FileService(IOptions<FileStorageSettings> settings, IWebHostEnvironment environment)
        {
            _settings = settings.Value;
            _environment = environment;
            _metadataPath = Path.Combine(_environment.ContentRootPath, _settings.StoragePath, "metadata");

            InitializeStorage();
        }

        private void InitializeStorage()
        {
            var storagePath = Path.Combine(_environment.ContentRootPath, _settings.StoragePath);

            if (!Directory.Exists(storagePath))
                Directory.CreateDirectory(storagePath);

            if (!Directory.Exists(_metadataPath))
                Directory.CreateDirectory(_metadataPath);

            // Створення папок для категорій
            foreach (var category in _settings.Categories)
            {
                var categoryPath = Path.Combine(storagePath, category.Value.Path);
                if (!Directory.Exists(categoryPath))
                    Directory.CreateDirectory(categoryPath);
            }
        }

        public async Task<FileMetadata> SaveFileAsync(IFormFile file, string category)
        {
            ValidateFile(file, category);

            var fileId = Guid.NewGuid().ToString();
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{fileId}{extension}";

            var categorySettings = _settings.Categories.GetValueOrDefault(category);
            var relativePath = categorySettings != null
                ? Path.Combine(categorySettings.Path, fileName)
                : Path.Combine("documents", fileName);

            var fullPath = Path.Combine(_environment.ContentRootPath, _settings.StoragePath, relativePath);

            // Збереження файлу
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Обробка зображень (зміна розміру для аватарок)
            if (category == "avatar" && IsImage(file.ContentType))
            {
                await ProcessImageAsync(fullPath, categorySettings);
            }

            var metadata = new FileMetadata
            {
                Id = fileId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Size = file.Length,
                Category = category,
                Path = relativePath,
                Url = $"/api/files/{fileId}"
            };

            // Збереження метаданих
            await SaveMetadataAsync(metadata);

            return metadata;
        }

        public async Task<(byte[] data, string contentType, string fileName)> GetFileAsync(string id)
        {
            var metadata = await GetMetadataAsync(id);
            if (metadata == null)
                throw new FileNotFoundException($"File with id {id} not found");

            var fullPath = Path.Combine(_environment.ContentRootPath, _settings.StoragePath, metadata.Path);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"File {fullPath} not found");

            var data = await File.ReadAllBytesAsync(fullPath);
            return (data, metadata.ContentType, metadata.FileName);
        }

        public async Task<bool> DeleteFileAsync(string id)
        {
            var metadata = await GetMetadataAsync(id);
            if (metadata == null)
                return false;

            var fullPath = Path.Combine(_environment.ContentRootPath, _settings.StoragePath, metadata.Path);

            if (File.Exists(fullPath))
                File.Delete(fullPath);

            var metadataFile = Path.Combine(_metadataPath, $"{id}.json");
            if (File.Exists(metadataFile))
                File.Delete(metadataFile);

            return true;
        }

        public async Task<IEnumerable<FileMetadata>> GetFilesByCategory(string category)
        {
            var files = Directory.GetFiles(_metadataPath, "*.json");
            var result = new List<FileMetadata>();

            foreach (var file in files)
            {
                var json = await File.ReadAllTextAsync(file);
                var metadata = JsonSerializer.Deserialize<FileMetadata>(json);

                if (metadata != null && metadata.Category == category)
                    result.Add(metadata);
            }

            return result.OrderByDescending(f => f.UploadedAt);
        }

        private void ValidateFile(IFormFile file, string category)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            var categorySettings = _settings.Categories.GetValueOrDefault(category);
            var maxSize = categorySettings?.MaxSize ?? _settings.MaxFileSize;

            if (file.Length > maxSize)
                throw new ArgumentException($"File size exceeds maximum allowed size of {maxSize} bytes");

            if (categorySettings?.AllowedExtensions != null)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!categorySettings.AllowedExtensions.Contains(extension))
                    throw new ArgumentException($"File extension {extension} is not allowed for category {category}");
            }
        }

        private async Task ProcessImageAsync(string imagePath, CategorySettings settings)
        {
            if (settings?.MaxWidth == null && settings?.MaxHeight == null)
                return;

            using var image = await Image.LoadAsync(imagePath);

            var width = settings.MaxWidth ?? image.Width;
            var height = settings.MaxHeight ?? image.Height;

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Max
            }));

            await image.SaveAsync(imagePath);
        }

        private bool IsImage(string contentType)
        {
            return contentType.StartsWith("image/");
        }

        private async Task SaveMetadataAsync(FileMetadata metadata)
        {
            var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var metadataFile = Path.Combine(_metadataPath, $"{metadata.Id}.json");
            await File.WriteAllTextAsync(metadataFile, json);
        }

        private async Task<FileMetadata> GetMetadataAsync(string id)
        {
            var metadataFile = Path.Combine(_metadataPath, $"{id}.json");

            if (!File.Exists(metadataFile))
                return null;

            var json = await File.ReadAllTextAsync(metadataFile);
            return JsonSerializer.Deserialize<FileMetadata>(json);
        }
    }
}