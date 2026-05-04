using Carter;

using Events.API.Database;
using Events.API.Features.Events;
using Events.API.Services;

using FluentValidation;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;

using Npgsql;

using Polly;

using Scalar.AspNetCore;

using Shared.TokenService.Services;
using Shared.UserServiceClient;
var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (OpenTelemetry, health checks, service discovery)
builder.AddServiceDefaults();

builder.AddJwtAuthentication();
builder.Services.AddAuthorization();

// Add native .NET 10 OpenAPI document generation
builder.Services.AddOpenApi();

var eventsDbConnectionString = builder.Configuration.GetConnectionString("events-db")
    ?? throw new InvalidOperationException("Connection string 'events-db' is not configured.");

var eventsDbConnectionStringBuilder = new NpgsqlConnectionStringBuilder(eventsDbConnectionString)
{
    MaxPoolSize = 30,
};

builder.Services.AddDbContext<EventsDbContext>(options =>
{
    options.UseNpgsql(eventsDbConnectionStringBuilder.ConnectionString);
});

// Configure MassTransit
builder.Services.AddMassTransit(config =>
{
    config.SetKebabCaseEndpointNameFormatter();

    config.AddConsumers(typeof(Program).Assembly);

    config.UsingRabbitMq((context, cfg) =>
    {
        var connectionString = builder.Configuration.GetConnectionString("rabbitmq");
        cfg.Host(new Uri(connectionString!));

        cfg.ConfigureEndpoints(context);
    });
});

// Configure Auth Service integration
builder.Services.Configure<AuthServiceSettings>(builder.Configuration.GetSection("AuthService"));

builder.Services.AddUserServiceClient();

builder.Services.AddHttpClient<IMotivationFakeClient, MotivationFakeClient>(client =>
        client.BaseAddress = new Uri("http://motivation-fake-service"))
    .AddResilienceHandler("custom", pipeline =>
    {
        pipeline.AddTimeout(TimeSpan.FromSeconds(3));

        // two retries with exponential backoff starting at 200ms, plus jitter to avoid thundering herd
        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 2,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            Delay = TimeSpan.FromMilliseconds(10),
        });

        pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            SamplingDuration = TimeSpan.FromSeconds(10),
            FailureRatio = 0.9,
            MinimumThroughput = 5,
            BreakDuration = TimeSpan.FromSeconds(5),
        });

        pipeline.AddTimeout(TimeSpan.FromMilliseconds(500));
    });

builder.Services.AddScoped<ParticipantEnricher>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddTransient<IValidator<CreateEvent.Command>, CreateEvent.Validator>();
builder.Services.AddTransient<IValidator<EditEvent.Command>, EditEvent.Validator>();
builder.Services.AddTransient<IValidator<AddParticipant.Command>, AddParticipant.Validator>();
builder.Services.AddTransient<IValidator<RemoveParticipant.Command>, RemoveParticipant.Validator>();
builder.Services.AddTransient<IValidator<GenerateEventInvitation.Command>, GenerateEventInvitation.Validator>();
builder.Services.AddTransient<IValidator<ValidateEventInvitation.Query>, ValidateEventInvitation.Validator>();
builder.Services.AddTransient<IValidator<JoinEvent.Command>, JoinEvent.Validator>();

builder.Services.AddCarter();

var app = builder.Build();

// Apply database migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<EventsDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        // Apply any pending migrations
        await context.Database.MigrateAsync();

        await RuntimeSeedData.SeedAsync(context, logger);

        // Log migration completion
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    // Map OpenAPI document endpoint (.NET 10 native)
    app.MapOpenApi();

    // Use Scalar UI for modern, interactive API documentation
    // Access at: /scalar/v1
    app.MapScalarApiReference();
}

app.UsePathBase("/api");
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();

// Map Aspire health check endpoints
app.MapDefaultEndpoints();

await app.RunAsync();
