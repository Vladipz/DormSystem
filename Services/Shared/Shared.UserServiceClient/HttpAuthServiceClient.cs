using System.Net.Http.Json;
using System.Text.Json;

using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Shared.UserServiceClient
{
    public class HttpAuthServiceClient : IAuthServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpAuthServiceClient> _logger;

        public HttpAuthServiceClient(
            HttpClient httpClient,
            IOptions<AuthServiceSettings> settings,
            ILogger<HttpAuthServiceClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task<ErrorOr<UserDto>> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/user/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get user information from Auth service: {StatusCode} {Response}", response.StatusCode, await response.Content.ReadAsStringAsync());
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

            var normalizedIds = userIds
                .Distinct()
                .ToList();

            if (normalizedIds.Count == 0)
            {
                return new Dictionary<Guid, UserDto>();
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "/api/user/batch",
                    new GetUsersByIdsRequest { UserIds = normalizedIds });

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Failed to get users information from Auth service: {StatusCode} {Response}",
                        response.StatusCode,
                        await response.Content.ReadAsStringAsync());
                    return Error.Failure("Auth.GetUsersFailed", $"Failed to get users information. Status: {response.StatusCode}");
                }

                var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
                if (users is null)
                {
                    return Error.Failure("Auth.InvalidResponse", "Invalid response from Auth service");
                }

                return users.ToDictionary(user => user.Id, user => user);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error when getting users info in batch");
                return Error.Failure("Auth.ConnectionError", ex.Message);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error when getting users info in batch");
                return Error.Failure("Auth.InvalidResponse", ex.Message);
            }
            catch (Exception ex) when (
                ex is not OperationCanceledException &&
                ex is not ObjectDisposedException)
            {
                _logger.LogError(ex, "Unexpected error when getting users info in batch");
                return Error.Unexpected(description: ex.Message);
            }
        }
    }
}
