using Carter;
using Carter.OpenApi;

using Events.API.Database;
using Events.API.Features.Events;
using Events.API.Services;

using FluentValidation;

using MassTransit;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using Scalar.AspNetCore;

using Shared.TokenService.Services;
using Shared.UserServiceClient;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication()
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateActor = false,
            ValidateIssuerSigningKey = false,
            ValidateLifetime = false,
            ValidateTokenReplay = false,
            SignatureValidator = (token, _) => new JsonWebToken(token),
        };
    });

builder.Services.AddAuthorization();

// Add native .NET 10 OpenAPI document generation
builder.Services.AddOpenApi();

// Setup Database
builder.Services.AddDbContext<EventsDbContext>(options =>
{
    // Add SQLite support with default connection string
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Configure MassTransit
builder.Services.AddMassTransit(config =>
{
    // Configure RabbitMQ as the message broker
    config.SetKebabCaseEndpointNameFormatter();

    config.AddConsumers(typeof(Program).Assembly);

    config.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqSettings = builder.Configuration.GetSection("RabbitMq");
        var host = rabbitMqSettings["Host"] ?? "localhost";
        var username = rabbitMqSettings["Username"] ?? "guest";
        var password = rabbitMqSettings["Password"] ?? "guest";

        cfg.Host(host, h =>
        {
            h.Username(username);
            h.Password(password);
        });

        cfg.ConfigureEndpoints(context);
    });
});

// Configure Auth Service integration
string authServiceUrl = builder.Configuration.GetValue<string>("AuthService:ApiUrl") ?? throw new
InvalidOperationException("AuthService:ApiUrl is not configured.");

builder.Services.Configure<AuthServiceSettings>(builder.Configuration.GetSection("AuthService"));

builder.Services.AddUserServiceClient(authServiceUrl);

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
        
        // Apply any pending migrations
        await context.Database.MigrateAsync();
        
        // Log migration completion
        var logger = services.GetRequiredService<ILogger<Program>>();
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

await app.RunAsync();