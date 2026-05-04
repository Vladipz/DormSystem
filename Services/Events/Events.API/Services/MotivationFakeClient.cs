using System.Text.Json;

using Polly.Timeout;

namespace Events.API.Services;

public sealed class MotivationFakeClient : IMotivationFakeClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MotivationFakeClient> _logger;

    public MotivationFakeClient(HttpClient httpClient, ILogger<MotivationFakeClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GetPhraseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/motivation/phrase", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("MotivationFake returned {StatusCode}", response.StatusCode);
                return string.Empty;
            }

            var payload = await response.Content.ReadFromJsonAsync<MotivationPhraseResponse>(cancellationToken);
            return payload?.Phrase ?? string.Empty;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "HTTP error while calling MotivationFake service");
            return string.Empty;
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogWarning(ex, "MotivationFake call timed out");
            return string.Empty;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid JSON from MotivationFake service");
            return string.Empty;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "MotivationFake call was canceled unexpectedly");
            return string.Empty;
        }
    }
}
