namespace Auth.BLL.Models
{
    /// <summary>
    /// Represents a token response containing access and refresh tokens.
    /// </summary>
    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;
    }
}