using Carter;
using Carter.OpenApi;

using Events.API.Database;
using Events.API.Features.Events;

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

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Description = "Carter Sample API",
        Version = "v1",
        Title = "A Carter API to manage Actors/Films/Crew etc",
    });
    options.DocInclusionPredicate((s, description) =>
    {
        for (int i = 0; i < description.ActionDescriptor.EndpointMetadata.Count; i++)
        {
            object? metaData = description.ActionDescriptor.EndpointMetadata[i];
            if (metaData is IIncludeOpenApi)
            {
                return true;
            }
        }

        return false;
    });
});

// Setup Database
builder.Services.AddDbContext<EventsDbContext>(options =>
{
    // Add PostgreSQL support with default connection string
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddTransient<IValidator<CreateEvent.Command>, CreateEvent.Validator>();
builder.Services.AddTransient<IValidator<EditEvent.Command>, EditEvent.Validator>();
builder.Services.AddTransient<IValidator<AddParticipant.Command>, AddParticipant.Validator>();
builder.Services.AddTransient<IValidator<RemoveParticipant.Command>, RemoveParticipant.Validator>();

builder.Services.AddCarter();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Management API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UsePathBase("/api");
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();

await app.RunAsync();