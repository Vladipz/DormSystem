using Carter;
using Carter.OpenApi;

using Events.API.Database;
using Events.API.Features.Events;

using FluentValidation;

using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddTransient<IValidator<CreateEvent.Command>, CreateEvent.Validator>();
builder.Services.AddTransient<IValidator<EditEvent.Command>, EditEvent.Validator>();

builder.Services.AddCarter();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Management API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();
app.MapCarter();

await app.RunAsync();