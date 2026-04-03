var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("compose");

// env variables
var jwtSecret = builder.AddParameter("jwt-secret", secret: true);
var jwtIssuer = builder.AddParameter("jwt-issuer");
var jwtAudience = builder.AddParameter("jwt-audience");

// ===== INFRASTRUCTURE =====

// PostgreSQL with multiple databases
// Aspire generates a secure random password automatically and injects it via connection strings
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("shared_postgres_data")
    .WithPgAdmin();

// Create databases for each service
var authDb = postgres.AddDatabase("auth-db");
var eventsDb = postgres.AddDatabase("events-db");
var roomsDb = postgres.AddDatabase("rooms-db");
var inspectionsDb = postgres.AddDatabase("inspections-db");
var notificationsDb = postgres.AddDatabase("notifications-db");
var telegramDb = postgres.AddDatabase("telegram-db");

// RabbitMQ message bus
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithDataVolume("rabbitmq_data");

// ===== MICROSERVICES =====
var fileStorageService = builder.AddProject<Projects.FileStorage_API>("file-storage-service");

var authService = builder.AddProject<Projects.Auth_API>("auth-service")
    .WithReference(authDb)
    .WithReference(fileStorageService)
    .WaitFor(authDb)
    .WithEnvironment("Jwt__Secret", jwtSecret)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience);

var eventService = builder.AddProject<Projects.Events_API>("event-service")
    .WithReference(eventsDb)
    .WithReference(rabbitmq)
    .WithReference(authService)
    .WaitFor(eventsDb)
    .WaitFor(rabbitmq)
    .WithEnvironment("Jwt__Secret", jwtSecret)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience);

var roomService = builder.AddProject<Projects.Rooms_API>("room-service")
    .WithReference(roomsDb)
    .WithReference(rabbitmq)
    .WithReference(authService)
    .WithReference(fileStorageService)
    .WaitFor(roomsDb)
    .WaitFor(rabbitmq);

var inspectionService = builder.AddProject<Projects.Inspections_API>("inspection-service")
    .WithReference(inspectionsDb)
    .WithReference(rabbitmq)
    .WithReference(roomService)
    .WaitFor(inspectionsDb)
    .WaitFor(rabbitmq)
    .WithEnvironment("Jwt__Secret", jwtSecret)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience);

var notificationService = builder.AddProject<Projects.NotificationCore_API>("notification-service")
    .WithReference(notificationsDb)
    .WithReference(rabbitmq)
    .WithReference(roomService)
    .WaitFor(notificationsDb)
    .WaitFor(rabbitmq)
    .WithEnvironment("Jwt__Secret", jwtSecret)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience);

var botToken = builder.AddParameter("botToken", secret: true);

var telegramService = builder.AddProject<Projects.TelegramAgent_API>("telegram-service")
    .WithReference(telegramDb)
    .WithReference(rabbitmq)
    .WithReference(authService)
    .WithEnvironment("TelegramBot__Token", botToken)
    .WaitFor(telegramDb)
    .WaitFor(rabbitmq);

var bookingService = builder.AddProject<Projects.Booking_API>("booking-service");

var apiGateway = builder.AddProject<Projects.ApiGateway>("api-gateway")
    .WithReference(authService)
    .WithReference(eventService)
    .WithReference(inspectionService)
    .WithReference(roomService)
    .WithReference(notificationService)
    .WithReference(telegramService)
    .WithReference(fileStorageService)
    .WithReference(bookingService)
    .WithEnvironment("Jwt__Secret", jwtSecret)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience);

// var frontend = builder.AddViteApp("frontend", "../../../frontend/dorm-app")
//     .WithYarn()
//     .WithReference(apiGateway)
//     .WaitFor(apiGateway);

builder.Build().Run();
