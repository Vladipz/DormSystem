namespace Events.API.Services;

public interface IMotivationFakeClient
{
    Task<string> GetPhraseAsync(CancellationToken cancellationToken = default);
}
