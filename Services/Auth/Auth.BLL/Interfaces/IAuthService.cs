using ErrorOr;

namespace Auth.BLL.Interfaces
{
    public interface IAuthService
    {
        // Повертає JWT access token для автентифікованого користувача
        Task<ErrorOr<string>> CreateTokenAsync(string userId);

        // Успіх: JWT token string
        // Помилка: InvalidUser, TokenGenerationFailed

        // Створює refresh token для можливості оновлення access token
        Task<ErrorOr<string>> CreateRefreshTokenAsync(string userId);

        // Успіх: Refresh token string
        // Помилка: InvalidUser, TokenGenerationFailed

        // Реєструє нового користувача в системі
        Task<ErrorOr<Guid>> RegisterUserAsync(string email, string password);

        // Успіх: UserId string
        // Помилка: DuplicateEmail, InvalidEmail, WeakPassword

        // Оновлює пару access+refresh токенів використовуючи існуючий refresh token
        Task<ErrorOr<(string accessToken, string refreshToken)>> RefreshTokenAsync(string refreshToken);

        // Успіх: Tuple з новим access token та refresh token
        // Помилка: InvalidRefreshToken, TokenExpired

        // Генерує код авторизації для PKCE flow
        Task<ErrorOr<string>> GenerateAuthCodeAsync(string email, string password, string codeChallenge);

        // Успіх: Auth code string
        // Помилка: InvalidClient, InvalidCodeChallenge

        // Перевіряє код авторизації та code verifier
        Task<bool> VerifyAuthCodeAsync(string authCode, string codeVerifier);

        // Успіх: bool - true якщо код валідний
        // Помилка: InvalidAuthCode, InvalidCodeVerifier, AuthCodeExpired
    }
}