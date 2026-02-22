using System.Text;

using Auth.BLL.Interfaces;
using Auth.BLL.Models;
using Auth.BLL.Services;
using Auth.DAL.Data;
using Auth.DAL.Entities;
using Auth.DAL.Extentions;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;

using Scalar.AspNetCore;

using Shared.FileServiceClient.Extensions;
using Shared.TokenService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (OpenTelemetry, health checks, service discovery)
builder.AddServiceDefaults();

// Add CORS service
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add shared FileServiceClient
builder.Services.AddFileServiceClient(builder.Configuration);

// Add shared TokenService
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILinkCodeService, LinkCodeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<DbSeederService>();

builder.Services.AddDalServices(builder.Configuration);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<AdminSettings>(builder.Configuration.GetSection("AdminSettings"));

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings section is not configured");

builder.Services.AddIdentity<User, Role>()
               .AddEntityFrameworkStores<AuthDbContext>()
               .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var seeder = services.GetRequiredService<DbSeederService>();
    await seeder.SeedDatabaseAsync();
}

// Use CORS before routing
app.UseCors();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Configure OpenAPI and Scalar
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Для перехоплення та перенаправлення на HTTPS
app.UseHttpsRedirection();

// Реєстрація маршрутів контролерів
app.MapControllers();

// Map Aspire health check endpoints
app.MapDefaultEndpoints();

// Запуск програми
await app.RunAsync();