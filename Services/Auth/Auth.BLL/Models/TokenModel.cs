namespace Auth.BLL.Models
{
    /// <summary>
    /// Represents a token response containing access and refresh tokens.
    /// </summary>
    public class TokenModel
    {
        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;
    }
}