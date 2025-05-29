using System.Text;

using Auth.BLL.Interfaces;
using Auth.BLL.Models;
using Auth.BLL.Services;
using Auth.DAL.Data;
using Auth.DAL.Entities;
using Auth.DAL.Extentions;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Shared.FileServiceClient.Extensions;
using Shared.TokenService.Services;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Your API Name",
        Version = "v1",
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
    });

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

// Якщо розробка, то налаштовуємо Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API Name v1"));
}

// Для перехоплення та перенаправлення на HTTPS
app.UseHttpsRedirection();

// Реєстрація маршрутів контролерів
app.MapControllers();

// Запуск програми
await app.RunAsync();