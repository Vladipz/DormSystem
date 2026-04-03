using System.Net.Http.Json;
using System.Web;

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

    public HttpRoomService(
        HttpClient httpClient,
        IOptions<RoomServiceSettings> settings,
        ILogger<HttpRoomService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<ErrorOr<List<RoomDto>>> GetRoomsInfoByDormitoryIdAsync(Guid dormitoryId, CancellationToken ct = default)
    {
        try
        {
            var requestUri = BuildRelativeUri("/api/rooms", new Dictionary<string, string?>
            {
                ["dormitoryId"] = dormitoryId.ToString(),
            });
            var response = await _httpClient.GetAsync(requestUri, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to get rooms from Room service: {StatusCode} {Response}",
                    response.StatusCode,
                    await response.Content.ReadAsStringAsync(ct));

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
            var requestUri = BuildRelativeUri("/api/rooms/for-inspection", new Dictionary<string, string?>
            {
                ["dormitoryId"] = dormitoryId.ToString(),
                ["includeSpecial"] = includeSpecial.ToString().ToLowerInvariant(),
            });
            var response = await _httpClient.GetAsync(requestUri, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to get rooms for inspection from Room service: {StatusCode} {Response}",
                    response.StatusCode,
                    await response.Content.ReadAsStringAsync(ct));

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

    public async Task<ErrorOr<List<PlaceDto>>> GetOccupiedPlacesByRoomIdAsync(Guid roomId, CancellationToken ct = default)
    {
        try
        {
            var requestUri = BuildRelativeUri("/api/places", new Dictionary<string, string?>
            {
                ["roomId"] = roomId.ToString(),
                ["isOccupied"] = bool.TrueString.ToLowerInvariant(),
                ["pageSize"] = "100",
            });
            var response = await _httpClient.GetAsync(requestUri, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to get occupied places from Room service: {StatusCode} {Response}",
                    response.StatusCode,
                    await response.Content.ReadAsStringAsync(ct));

                return Error.Failure("Room.GetOccupiedPlacesFailed", $"Failed to get occupied places. Status: {response.StatusCode}");
            }

            var placesResponse = await response.Content.ReadFromJsonAsync<PagedResponse<PlaceDto>>(cancellationToken: ct);
            if (placesResponse?.Items is null)
            {
                return Error.Failure("Room.InvalidResponse", "Invalid response from Room service");
            }

            return placesResponse.Items;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error when getting occupied places for room {RoomId}", roomId);
            return Error.Failure("Room.ConnectionError", ex.Message);
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error when getting occupied places for room {RoomId}", roomId);
            return Error.Failure("Room.InvalidResponse", ex.Message);
        }
        catch (Exception ex) when (
            ex is not OperationCanceledException &&
            ex is not ObjectDisposedException)
        {
            _logger.LogError(ex, "Unexpected error when getting occupied places for room {RoomId}", roomId);
            return Error.Unexpected(description: ex.Message);
        }
    }

    private static Uri BuildRelativeUri(string path, IReadOnlyDictionary<string, string?> queryParameters)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);

        foreach (var (key, value) in queryParameters)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                query[key] = value;
            }
        }

        var queryString = query.ToString();
        return new Uri(string.IsNullOrEmpty(queryString) ? path : $"{path}?{queryString}", UriKind.Relative);
    }

    // Generic paged response wrapper
    private class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new List<T>();
    }
}
