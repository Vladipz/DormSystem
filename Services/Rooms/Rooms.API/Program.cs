using System.Text.Json.Serialization;

using Carter;

using FluentValidation;

using Mapster;

using MapsterMapper;

using MassTransit;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using Rooms.API.Data;
using Rooms.API.Features.Blocks;
using Rooms.API.Features.Buildings;
using Rooms.API.Features.Floors;
using Rooms.API.Features.Maintenance;
using Rooms.API.Features.Places;
using Rooms.API.Features.Rooms;
using Rooms.API.Services;

using Scalar.AspNetCore;

using Shared.TokenService.Services;
using Shared.FileServiceClient.Extensions;
using Shared.UserServiceClient;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (OpenTelemetry, health checks, service discovery)
builder.AddServiceDefaults();

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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowCredentials()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthorization();

// Add Antiforgery services
builder.Services.AddAntiforgery();

// Register Mapster
var config = TypeAdapterConfig.GlobalSettings;
config.Scan(typeof(Program).Assembly);
builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, Mapper>();

// Configure MassTransit
builder.Services.AddMassTransit(config =>
{
    config.SetKebabCaseEndpointNameFormatter();

    config.AddConsumer<EventCreatedConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(builder.Configuration.GetConnectionString("rabbitmq")!));

        cfg.ReceiveEndpoint("rooms-service-events", e =>
        {
            e.ConfigureConsumer<EventCreatedConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});

// Register HttpClient for Auth Service
builder.Services.Configure<AuthServiceSettings>(builder.Configuration.GetSection("AuthService"));

builder.Services.AddUserServiceClient();

// Register FileServiceClient for managing room photos
builder.Services.AddFileServiceClient(builder.Configuration);

// Register MaintenanceTicketEnricher
builder.Services.AddScoped<MaintenanceTicketEnricher>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("rooms-db"));
});

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Room Validators
builder.Services.AddTransient<IValidator<CreateRoom.Command>, CreateRoom.Validator>();
builder.Services.AddTransient<IValidator<GetRooms.Query>, GetRooms.Validator>();
builder.Services.AddTransient<IValidator<GetRoomById.Query>, GetRoomById.Validator>();
builder.Services.AddTransient<IValidator<UpdateRoom.Command>, UpdateRoom.Validator>();
builder.Services.AddTransient<IValidator<DeleteRoom.Command>, DeleteRoom.Validator>();
builder.Services.AddTransient<IValidator<UploadRoomPhoto.Command>, UploadRoomPhoto.Validator>();
builder.Services.AddTransient<IValidator<DeleteRoomPhoto.Command>, DeleteRoomPhoto.Validator>();

// Block Validators
builder.Services.AddTransient<IValidator<CreateBlock.Command>, CreateBlock.Validator>();
builder.Services.AddTransient<IValidator<GetBlocks.Query>, GetBlocks.Validator>();
builder.Services.AddTransient<IValidator<GetBlockById.Query>, GetBlockById.Validator>();
builder.Services.AddTransient<IValidator<UpdateBlock.Command>, UpdateBlock.Validator>();
builder.Services.AddTransient<IValidator<DeleteBlock.Command>, DeleteBlock.Validator>();

// Floor Validators
builder.Services.AddTransient<IValidator<CreateFloor.Command>, CreateFloor.Validator>();
builder.Services.AddTransient<IValidator<GetFloors.Query>, GetFloors.Validator>();
builder.Services.AddTransient<IValidator<GetFloorById.Query>, GetFloorById.Validator>();
builder.Services.AddTransient<IValidator<UpdateFloor.Command>, UpdateFloor.Validator>();
builder.Services.AddTransient<IValidator<DeleteFloor.Command>, DeleteFloor.Validator>();

// Place Validators
builder.Services.AddTransient<IValidator<CreatePlace.Command>, CreatePlace.Validator>();
builder.Services.AddTransient<IValidator<GetPlaces.Query>, GetPlaces.Validator>();
builder.Services.AddTransient<IValidator<GetPlaceById.Query>, GetPlaceById.Validator>();
builder.Services.AddTransient<IValidator<UpdatePlace.Command>, UpdatePlace.Validator>();
builder.Services.AddTransient<IValidator<DeletePlace.Command>, DeletePlace.Validator>();
builder.Services.AddTransient<IValidator<VacatePlace.Command>, VacatePlace.Validator>();
builder.Services.AddTransient<IValidator<OccupyPlace.Command>, OccupyPlace.Validator>();
builder.Services.AddTransient<IValidator<GetUserAddress.Query>, GetUserAddress.Validator>();

// Building Validators
builder.Services.AddTransient<IValidator<CreateBuilding.Command>, CreateBuilding.Validator>();
builder.Services.AddTransient<IValidator<GetBuildings.Query>, GetBuildings.Validator>();
builder.Services.AddTransient<IValidator<GetBuildingById.Query>, GetBuildingById.Validator>();
builder.Services.AddTransient<IValidator<UpdateBuilding.Command>, UpdateBuilding.Validator>();
builder.Services.AddTransient<IValidator<DeleteBuilding.Command>, DeleteBuilding.Validator>();

// Maintenance Validators
builder.Services.AddTransient<IValidator<CreateMaintenanceTicket.Command>, CreateMaintenanceTicket.Validator>();
builder.Services.AddTransient<IValidator<UpdateMaintenanceTicket.Command>, UpdateMaintenanceTicket.Validator>();
builder.Services.AddTransient<IValidator<DeleteMaintenanceTicket.Command>, DeleteMaintenanceTicket.Validator>();
builder.Services.AddTransient<IValidator<GetMaintenanceTickets.Query>, GetMaintenanceTickets.Validator>();
builder.Services.AddTransient<IValidator<GetMaintenanceTicketById.Query>, GetMaintenanceTicketById.Validator>();
builder.Services.AddTransient<IValidator<ChangeMaintenanceTicketStatus.Command>, ChangeMaintenanceTicketStatus.Validator>();

// Register TokenService
builder.Services.AddScoped<ITokenService, TokenService>();

// Configure Carter
builder.Services.AddCarter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
await SeedData.InitializeAsync(dbContext);

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapGroup("/api")

   .MapCarter();

// Map Aspire health check endpoints
app.MapDefaultEndpoints();

app.Run();
