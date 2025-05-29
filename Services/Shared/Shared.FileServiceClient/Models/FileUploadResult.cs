namespace Shared.FileServiceClient.Models
{
    public class FileUploadResult
    {
        public string Id { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
    }
} 