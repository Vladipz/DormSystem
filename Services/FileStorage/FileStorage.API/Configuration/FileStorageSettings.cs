namespace FileStorage.API.Configuration
{
    public class FileStorageSettings
    {
        public string StoragePath { get; set; } = "Storage";
        public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
        public Dictionary<string, CategorySettings> Categories { get; set; } = new();
    }

    public class CategorySettings
    {
        public string Path { get; set; }
        public long MaxSize { get; set; }
        public string[] AllowedExtensions { get; set; }
        public int? MaxWidth { get; set; }
        public int? MaxHeight { get; set; }
    }
}