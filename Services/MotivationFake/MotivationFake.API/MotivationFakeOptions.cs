public sealed class MotivationFakeOptions
{
    public int SlowDelayMs { get; init; } = 5000;

    public MotivationFakeProbabilities Probabilities { get; init; } = new();
}
