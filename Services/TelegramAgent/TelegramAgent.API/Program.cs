using FluentValidation;

using MassTransit;

using Microsoft.EntityFrameworkCore;

using Telegram.Bot;

using TelegramAgent.API.Data;
using TelegramAgent.API.Features;
using TelegramAgent.API.Features.Bot;
using TelegramAgent.API.Features.Bot.Commands;
using TelegramAgent.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HTTP client factory
builder.Services.AddHttpClient();

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Add Entity Framework
builder.Services.AddDbContext<TelegramDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=TelegramAgent.db"));

// Add Telegram Bot
var botToken = builder.Configuration["TelegramBot:Token"] ?? throw new InvalidOperationException("Telegram bot token is required");
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));

// Add Telegram commands
builder.Services.AddScoped<ITelegramCommand, StartCommand>();
builder.Services.AddScoped<ITelegramCommand, HelpCommand>();
builder.Services.AddScoped<ITelegramCommand, AuthCommand>();
builder.Services.AddScoped<ITelegramCommand, UnlinkCommand>();
builder.Services.AddScoped<ITelegramCommand, StatusCommand>();

// Add command registry and bot handler
builder.Services.AddScoped<ITelegramCommandRegistry, TelegramCommandRegistry>();
builder.Services.AddScoped<ITelegramBotCommandHandler, TelegramBotCommandHandler>();

// Add Telegram bot background service
builder.Services.AddHostedService<TelegramBotService>();

// Add MassTransit
builder.Services.AddMassTransit(x =>
{
    // Add consumers
    x.AddConsumer<NotificationDelivery.NotificationCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        var rabbitMqUsername = builder.Configuration["RabbitMQ:Username"] ?? "guest";
        var rabbitMqPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";

        cfg.Host(rabbitMqHost, "/", h =>
        {
            h.Username(rabbitMqUsername);
            h.Password(rabbitMqPassword);
        });

        // Configure endpoints automatically
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TelegramDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();
