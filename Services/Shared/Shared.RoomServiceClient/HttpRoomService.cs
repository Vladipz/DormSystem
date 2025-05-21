using System.Net.Http.Json;

using ErrorOr;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RoomService.Client;

using Shared.Data.Dtos;


namespace Shared.RoomServiceClient;

public sealed class HttpRoomService : IRoomService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpRoomService> _logger;
    private readonly RoomServiceSettings _settings;

    public HttpRoomService(
        HttpClient httpClient,
        IOptions<RoomServiceSettings> settings,
        ILogger<HttpRoomService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<ErrorOr<List<RoomDto>>> GetRoomsInfoByDormitoryIdAsync(Guid dormitoryId, CancellationToken ct = default)
    {
        try
        {
            var requestUri = new Uri($"{_settings.ApiUrl}/api/rooms?dormitoryId={dormitoryId}");
            var response = await _httpClient.GetAsync(requestUri, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get rooms from Room service: {StatusCode} {Response}",
                    response.StatusCode, await response.Content.ReadAsStringAsync(ct));

                return Error.Failure("Room.GetRoomsFailed", $"Failed to get rooms. Status: {response.StatusCode}");
            }

            var rooms = await response.Content.ReadFromJsonAsync<List<RoomDto>>(cancellationToken: ct);
            if (rooms is null)
            {
                return Error.Failure("Room.InvalidResponse", "Invalid response from Room service");
            }

            return rooms;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error when getting rooms for dormitory {DormitoryId}", dormitoryId);
            return Error.Failure("Room.ConnectionError", ex.Message);
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error when getting rooms for dormitory {DormitoryId}", dormitoryId);
            return Error.Failure("Room.InvalidResponse", ex.Message);
        }
        catch (Exception ex) when (
            ex is not OperationCanceledException &&
            ex is not ObjectDisposedException)
        {
            _logger.LogError(ex, "Unexpected error when getting rooms for dormitory {DormitoryId}", dormitoryId);
            return Error.Unexpected(description: ex.Message);
        }
    }

    public async Task<ErrorOr<List<RoomDto>>> GetRoomsForInspectionAsync(Guid dormitoryId, bool includeSpecial = true, CancellationToken ct = default)
    {
        try
        {
            var requestUri = new Uri($"{_settings.ApiUrl}/api/rooms/for-inspection?dormitoryId={dormitoryId}&includeSpecial={includeSpecial}");
            var response = await _httpClient.GetAsync(requestUri, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get rooms for inspection from Room service: {StatusCode} {Response}",
                    response.StatusCode, await response.Content.ReadAsStringAsync(ct));

                return Error.Failure("Room.GetRoomsForInspectionFailed", $"Failed to get rooms for inspection. Status: {response.StatusCode}");
            }

            var rooms = await response.Content.ReadFromJsonAsync<List<RoomDto>>(cancellationToken: ct);
            if (rooms is null)
            {
                return Error.Failure("Room.InvalidResponse", "Invalid response from Room service");
            }

            return rooms;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error when getting rooms for inspection in dormitory {DormitoryId}", dormitoryId);
            return Error.Failure("Room.ConnectionError", ex.Message);
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error when getting rooms for inspection in dormitory {DormitoryId}", dormitoryId);
            return Error.Failure("Room.InvalidResponse", ex.Message);
        }
        catch (Exception ex) when (
            ex is not OperationCanceledException &&
            ex is not ObjectDisposedException)
        {
            _logger.LogError(ex, "Unexpected error when getting rooms for inspection in dormitory {DormitoryId}", dormitoryId);
            return Error.Unexpected(description: ex.Message);
        }
    }
}
