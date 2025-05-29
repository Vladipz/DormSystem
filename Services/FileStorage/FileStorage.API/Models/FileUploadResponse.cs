namespace FileStorage.API.Models
{
    public class FileUploadResponse
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }
        public long Size { get; set; }
    }
}