namespace Auth.BLL.Models
{
    public class LinkCodeDto
    {
        public string Code { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
    }
}