using System.Text.Json.Serialization;

using Carter;

using FluentValidation;

using Mapster;

using MapsterMapper;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Rooms.API.Data;
using Rooms.API.Features.Blocks;
using Rooms.API.Features.Buildings;
using Rooms.API.Features.Floors;
using Rooms.API.Features.Maintenance;
using Rooms.API.Features.Places;
using Rooms.API.Features.Rooms;
using Rooms.API.Services;

using Shared.TokenService.Services;
using Shared.UserServiceClient;

var builder = WebApplication.CreateBuilder(args);

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
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthorization();

// Register Mapster
var config = TypeAdapterConfig.GlobalSettings;
config.Scan(typeof(Program).Assembly);
builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, Mapper>();

// Register HttpClient for Auth Service
string authServiceUrl = builder.Configuration["AuthService:ApiUrl"] ?? throw new InvalidOperationException("AuthServiceUrl:ApiUrl is not configured.");

builder.Services.Configure<AuthServiceSettings>(builder.Configuration.GetSection("AuthService"));

builder.Services.AddUserServiceClient(authServiceUrl);

// Register MaintenanceTicketEnricher
builder.Services.AddScoped<MaintenanceTicketEnricher>();

// Реєструємо DbContext з SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Room Validators
builder.Services.AddTransient<IValidator<CreateRoom.Command>, CreateRoom.Validator>();
builder.Services.AddTransient<IValidator<GetRooms.Query>, GetRooms.Validator>();
builder.Services.AddTransient<IValidator<GetRoomById.Query>, GetRoomById.Validator>();
builder.Services.AddTransient<IValidator<UpdateRoom.Command>, UpdateRoom.Validator>();
builder.Services.AddTransient<IValidator<DeleteRoom.Command>, DeleteRoom.Validator>();

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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Rooms API", Version = "v1" });

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

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rooms API v1");
        c.RoutePrefix = "swagger";
        c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
    });
}

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
await SeedData.InitializeAsync(dbContext);

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapGroup("/api")
   .WithOpenApi()
   .MapCarter();

app.Run();