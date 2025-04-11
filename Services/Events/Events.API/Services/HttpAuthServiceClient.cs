using System.Net.Http.Json;
using System.Text.Json;

using ErrorOr;

using Events.API.Contracts;

using Microsoft.Extensions.Options;

namespace Events.API.Services
{
    public class HttpAuthServiceClient : IAuthServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpAuthServiceClient> _logger;
        private readonly AuthServiceSettings _settings;

        public HttpAuthServiceClient(
            HttpClient httpClient,
            IOptions<AuthServiceSettings> settings,
            ILogger<HttpAuthServiceClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task<ErrorOr<UserDto>> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var requestUri = new Uri($"{_settings.ApiUrl}/api/user/{userId}");
                var response = await _httpClient.GetAsync(requestUri);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get user information from Auth service: {StatusCode}", response.StatusCode);
                    return Error.Failure("Auth.GetUserFailed", $"Failed to get user information. Status: {response.StatusCode}");
                }

                var user = await response.Content.ReadFromJsonAsync<UserDto>();
                if (user is null)
                {
                    return Error.Failure("Auth.InvalidResponse", "Invalid response from Auth service");
                }

                return user;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error when getting user info for {UserId}", userId);
                return Error.Failure("Auth.ConnectionError", ex.Message);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error when getting user info for {UserId}", userId);
                return Error.Failure("Auth.InvalidResponse", ex.Message);
            }
            catch (Exception ex) when (
                ex is not OperationCanceledException &&
                ex is not ObjectDisposedException)
            {
                _logger.LogError(ex, "Unexpected error when getting user info for {UserId}", userId);
                return Error.Unexpected(description: ex.Message);
            }
        }

        public async Task<ErrorOr<Dictionary<Guid, UserDto>>> GetUsersByIdsAsync(IEnumerable<Guid> userIds)
        {
            ArgumentNullException.ThrowIfNull(userIds);

            // This is a simple implementation that fetches users one by one
            // In a production environment, you would want to implement a batch endpoint
            var result = new Dictionary<Guid, UserDto>();

            foreach (var userId in userIds)
            {
                var userResult = await GetUserByIdAsync(userId);

                if (userResult.IsError)
                {
                    _logger.LogWarning("Failed to get user {UserId}: {Error}", userId, userResult.FirstError.Description);
                    continue;
                }

                result[userId] = userResult.Value;
            }

            return result;
        }
    }
}