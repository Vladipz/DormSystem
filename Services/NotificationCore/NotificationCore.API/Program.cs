using System.Text;
using System.Text.Json.Serialization;

using Carter;

using MassTransit;

using MediatR;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using NotificationCore.API.Data;
using NotificationCore.API.Events.Events;

using RoomService.Client;

using Scalar.AspNetCore;

using Shared.TokenService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JSON options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Configure MassTransit
builder.Services.AddMassTransit(config =>
{
    config.SetKebabCaseEndpointNameFormatter();

    // Регіструємо всіх консюмерів
    config.AddConsumersFromNamespaceContaining<EventCreatedConsumer>();

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

        // Автоматично конфігурує всі endpoint-и для зареєстрованих консюмерів
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
builder.Services.AddRoomServiceClient(builder.Configuration);

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

await app.RunAsync();