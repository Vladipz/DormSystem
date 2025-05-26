namespace Auth.API.Models
{
    public class LinkCodeResponse
    {
        public string Code { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
    }
}