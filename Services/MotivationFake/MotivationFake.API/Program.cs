using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.Configure<MotivationFakeOptions>(builder.Configuration.GetSection("MotivationFake"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var phrases = new[]
{
    "Keep going, your next event can be the best one.",
    "Show up today, memories follow.",
    "Small dorm events can become big friendships.",
    "A good event starts with one person saying yes.",
};

app.MapGet("/api/motivation/phrase", async (
    HttpContext httpContext,
    IOptions<MotivationFakeOptions> options,
    string? scenario,
    CancellationToken cancellationToken) =>
{
    var activeScenarioResult = TryResolveScenario(scenario, options.Value.Probabilities, out var activeScenario);
    if (activeScenarioResult is not null)
    {
        return activeScenarioResult;
    }

    return activeScenario switch
    {
        MotivationScenario.Ok => Results.Ok(new MotivationPhraseResponse(GetRandomPhrase(phrases))),
        MotivationScenario.Slow => await SlowResponseAsync(phrases, options.Value.SlowDelayMs, cancellationToken),
        MotivationScenario.Error => Results.Problem(detail: "Upstream dependency failed", statusCode: StatusCodes.Status500InternalServerError),
        MotivationScenario.Unavailable => Results.Problem(detail: "Service temporarily unavailable", statusCode: StatusCodes.Status503ServiceUnavailable),
        MotivationScenario.Abort => Abort(httpContext),
        _ => Results.Problem(detail: "Unsupported scenario", statusCode: StatusCodes.Status500InternalServerError),
    };
})
.WithName("GetMotivationPhrase")
.WithTags("Motivation");

app.MapDefaultEndpoints();

app.Run();

static string GetRandomPhrase(IReadOnlyList<string> phrases)
    => phrases[Random.Shared.Next(phrases.Count)];

static async Task<IResult> SlowResponseAsync(IReadOnlyList<string> phrases, int delayMs, CancellationToken cancellationToken)
{
    await Task.Delay(delayMs, cancellationToken);
    return Results.Ok(new MotivationPhraseResponse(GetRandomPhrase(phrases)));
}

static IResult Abort(HttpContext httpContext)
{
    httpContext.Abort();
    return Results.Empty;
}

static IResult? TryResolveScenario(
    string? requestedScenario,
    MotivationFakeProbabilities probabilities,
    out MotivationScenario scenario)
{
    if (!string.IsNullOrWhiteSpace(requestedScenario))
    {
        if (!Enum.TryParse<MotivationScenario>(requestedScenario, ignoreCase: true, out scenario))
        {
            return Results.Problem(
                detail: "Scenario must be one of: ok, slow, error, unavailable, abort.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return null;
    }

    var roll = Random.Shared.NextDouble();
    var threshold = probabilities.Slow;
    if (roll < threshold)
    {
        scenario = MotivationScenario.Slow;
        return null;
    }

    threshold += probabilities.Error;
    if (roll < threshold)
    {
        scenario = MotivationScenario.Error;
        return null;
    }

    threshold += probabilities.Unavailable;
    if (roll < threshold)
    {
        scenario = MotivationScenario.Unavailable;
        return null;
    }

    threshold += probabilities.Abort;
    if (roll < threshold)
    {
        scenario = MotivationScenario.Abort;
        return null;
    }

    scenario = MotivationScenario.Ok;
    return null;
}

public sealed record MotivationPhraseResponse(string Phrase);

public sealed class MotivationFakeOptions
{
    public int SlowDelayMs { get; init; } = 5000;

    public MotivationFakeProbabilities Probabilities { get; init; } = new();
}

public sealed class MotivationFakeProbabilities
{
    public double Slow { get; init; } = 0.20;

    public double Error { get; init; } = 0.10;

    public double Unavailable { get; init; } = 0.05;

    public double Abort { get; init; } = 0.05;
}

public enum MotivationScenario
{
    Ok,
    Slow,
    Error,
    Unavailable,
    Abort,
}
