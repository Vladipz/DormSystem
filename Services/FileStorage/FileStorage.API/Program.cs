using FileStorage.API.Configuration;
using FileStorage.API.Middleware;
using FileStorage.API.Services;

using Microsoft.AspNetCore.OpenApi;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Налаштування конфігурації
builder.Services.Configure<FileStorageSettings>(
    builder.Configuration.GetSection("FileStorage"));

// Додавання сервісів
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddScoped<IFileService, FileService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

app.Run();