namespace Shared.FileServiceClient.Models
{
    public class FileStorageSettings
    {
        public const string SectionName = "FileStorage";
        
        public string BaseUrl { get; set; } = "https://localhost:7081";
    }
} 