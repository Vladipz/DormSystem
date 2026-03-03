using FluentValidation;

using MassTransit;

using Microsoft.EntityFrameworkCore;

using Scalar.AspNetCore;

using Telegram.Bot;

using TelegramAgent.API.Data;
using TelegramAgent.API.Features;
using TelegramAgent.API.Features.Bot;
using TelegramAgent.API.Features.Bot.Commands;
using TelegramAgent.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (OpenTelemetry, health checks, service discovery)
builder.AddServiceDefaults();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add HTTP client factory
builder.Services.AddHttpClient();

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddDbContext<TelegramDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("telegram-db"));
});

// Add Telegram Bot
// NOTE: Do NOT use IHttpClientFactory here — Aspire's AddServiceDefaults() injects
// service discovery and resilience handlers into all factory clients, which intercepts
// the TLS handshake to api.telegram.org and causes UntrustedRoot SSL errors.
// A direct HttpClient with SocketsHttpHandler bypasses Aspire's pipeline entirely.
var botToken = builder.Configuration["TelegramBot:Token"] ?? throw new InvalidOperationException("Telegram bot token is required");
builder.Services.AddSingleton<ITelegramBotClient>(_ =>
{
    var httpClient = new HttpClient(new SocketsHttpHandler(), disposeHandler: true);
    return new TelegramBotClient(botToken, httpClient);
});

// Add Telegram commands
builder.Services.AddScoped<ITelegramCommand, StartCommand>();
builder.Services.AddScoped<ITelegramCommand, HelpCommand>();
builder.Services.AddScoped<ITelegramCommand, AuthCommand>();
builder.Services.AddScoped<ITelegramCommand, UnlinkCommand>();
builder.Services.AddScoped<ITelegramCommand, StatusCommand>();

// Add command registry and bot handler
builder.Services.AddScoped<ITelegramCommandRegistry, TelegramCommandRegistry>();
builder.Services.AddScoped<ITelegramBotCommandHandler, TelegramBotCommandHandler>();

// Add Telegram bot background service
builder.Services.AddHostedService<TelegramBotService>();

// Add MassTransit
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddConsumer<NotificationDelivery.NotificationCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(builder.Configuration.GetConnectionString("rabbitmq")!));

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<TelegramDbContext>();

        // Apply any pending migrations
        await context.Database.MigrateAsync();

        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database migrations applied successfully for TelegramAgent");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the TelegramAgent database");
        throw;
    }
}

// Map Aspire health check endpoints
app.MapDefaultEndpoints();

app.Run();