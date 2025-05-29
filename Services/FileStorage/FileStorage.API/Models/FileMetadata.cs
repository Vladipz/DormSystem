namespace FileStorage.API.Models
{
    public class FileMetadata
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
        public string Category { get; set; } // avatar, gallery, document
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string Path { get; set; }
        public string Url { get; set; }
    }
}