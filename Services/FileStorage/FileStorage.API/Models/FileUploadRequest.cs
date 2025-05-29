namespace FileStorage.API.Models
{
    public class FileUploadRequest
    {
        public IFormFile File { get; set; }

        public string Category { get; set; } = "document";
    }
}