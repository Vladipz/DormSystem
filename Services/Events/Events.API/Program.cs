using Carter;
using Carter.OpenApi;

using Events.API.Database;
using Events.API.Features.Events;
using Events.API.Services;

using FluentValidation;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Shared.TokenService.Services;
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

// Configure Auth Service integration
builder.Services.Configure<AuthServiceSettings>(builder.Configuration.GetSection("AuthService"));
builder.Services.AddHttpClient<IAuthServiceClient, HttpAuthServiceClient>();
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