using Carter;
using Carter.OpenApi;

using Events.API.Database;
using Events.API.Features.Events;
using Events.API.Services;

using FluentValidation;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Events API", Version = "v1" });

    // Define the OAuth2.0 or Bearer authentication scheme for Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
    });

    // Make sure all endpoints are considered secured by default
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            Array.Empty<string>()
        },
    });
});

// Setup Database
builder.Services.AddDbContext<EventsDbContext>(options =>
{
    // Add PostgreSQL support with default connection string
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Management API v1");
        c.RoutePrefix = "swagger";
        c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
    });
}

app.UsePathBase("/api");
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();

await app.RunAsync();