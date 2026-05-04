public sealed class MotivationFakeProbabilities
{
    public double Slow { get; init; } = 0.20;

    public double Error { get; init; } = 0.10;

    public double Unavailable { get; init; } = 0.05;

    public double Abort { get; init; } = 0.05;
}
