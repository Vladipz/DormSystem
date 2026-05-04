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
    CancellationToken cancellationToken) =>
{
    var activeScenario = ResolveRandomScenario(options.Value.Probabilities);

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

static MotivationScenario ResolveRandomScenario(MotivationFakeProbabilities probabilities)
{
    var roll = Random.Shared.NextDouble();
    var threshold = probabilities.Slow;
    if (roll < threshold)
    {
        return MotivationScenario.Slow;
    }

    threshold += probabilities.Error;
    if (roll < threshold)
    {
        return MotivationScenario.Error;
    }

    threshold += probabilities.Unavailable;
    if (roll < threshold)
    {
        return MotivationScenario.Unavailable;
    }

    threshold += probabilities.Abort;
    if (roll < threshold)
    {
        return MotivationScenario.Abort;
    }

    return MotivationScenario.Ok;
}
