using ErrorOr;

namespace Auth.BLL.Interfaces
{
    /// <summary>
    /// Provides authentication and authorization services.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Creates an access token for a user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A JWT access token or an error.</returns>
        Task<ErrorOr<string>> CreateTokenAsync(Guid userId);

        /// <summary>
        /// Creates a refresh token for a user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A refresh token or an error.</returns>
        Task<ErrorOr<string>> CreateRefreshTokenAsync(Guid userId);

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>The unique identifier of the newly registered user or an error.</returns>
        Task<ErrorOr<Guid>> RegisterUserAsync(string email, string password);

        /// <summary>
        /// Refreshes an expired access token using a refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns>A tuple containing new access and refresh tokens or an error.</returns>
        Task<ErrorOr<(string accessToken, string refreshToken)>> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Generates an authorization code for OAuth 2.0 PKCE flow.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <param name="codeChallenge">The PKCE code challenge.</param>
        /// <returns>An authorization code or an error.</returns>
        Task<ErrorOr<string>> GenerateAuthCodeAsync(string email, string password, string codeChallenge);

        /// <summary>
        /// Verifies the validity of an authorization code using the code verifier.
        /// </summary>
        /// <param name="authCode">The authorization code to verify.</param>
        /// <param name="codeVerifier">The PKCE code verifier.</param>
        /// <returns>True if the code is valid, false otherwise, or an error.</returns>
        Task<ErrorOr<bool>> VerifyAuthCodeAsync(string authCode, string codeVerifier);

        /// <summary>
        /// Validates an authorization code and creates new access and refresh tokens.
        /// </summary>
        /// <param name="authCode">The authorization code to validate.</param>
        /// <param name="codeVerifier">The PKCE code verifier.</param>
        /// <returns>A tuple containing new access and refresh tokens or an error.</returns>
        Task<ErrorOr<(string accessToken, string refreshToken)>> ValidateAndCreateTokensAsync(string authCode, string codeVerifier);
    }
}