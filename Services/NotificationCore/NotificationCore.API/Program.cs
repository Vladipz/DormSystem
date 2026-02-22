using System.Text.Json.Serialization;

using Carter;

using MassTransit;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using NotificationCore.API.Data;
using NotificationCore.API.Events.Events;

using RoomService.Client;

using Scalar.AspNetCore;

using Shared.TokenService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (OpenTelemetry, health checks, service discovery)
builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("notifications-db"));
});

// Configure JSON options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Configure MassTransit
builder.Services.AddMassTransit(config =>
{
    config.SetKebabCaseEndpointNameFormatter();

    config.AddConsumersFromNamespaceContaining<EventCreatedConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(builder.Configuration.GetConnectionString("rabbitmq")!));

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173", // Vite dev server
                "http://localhost:4173", // Vite preview
                "http://localhost:3000") // Alternative local development
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Configure Authentication
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


// Configure MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Configure Room Service Client
// Use Aspire service discovery (null) or fall back to configured URL
var roomServiceUrl = builder.Configuration["RoomServiceSettings:BaseUrl"];
builder.Services.AddRoomServiceClient(builder.Configuration, roomServiceUrl);

builder.Services.AddCarter();
builder.Services.AddScoped<ITokenService, TokenService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Add Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Add CORS middleware
app.UseCors();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await SeedData.InitializeAsync(dbContext);
}

app.MapGroup("/api")

   .MapCarter();

// Map Aspire health check endpoints
app.MapDefaultEndpoints();

await app.RunAsync();