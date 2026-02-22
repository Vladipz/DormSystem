var builder = DistributedApplication.CreateBuilder(args);

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
var authService = builder.AddProject<Projects.Auth_API>("auth-service")
    .WithReference(authDb)
    .WaitFor(authDb);

var eventService = builder.AddProject<Projects.Events_API>("event-service")
    .WithReference(eventsDb)
    .WithReference(rabbitmq)
    .WithReference(authService)
    .WaitFor(eventsDb)
    .WaitFor(rabbitmq);

var roomService = builder.AddProject<Projects.Rooms_API>("room-service")
    .WithReference(roomsDb)
    .WithReference(rabbitmq)
    .WithReference(authService)
    .WaitFor(roomsDb)
    .WaitFor(rabbitmq);

var fileStorageService = builder.AddProject<Projects.FileStorage_API>("file-storage-service");

var inspectionService = builder.AddProject<Projects.Inspections_API>("inspection-service")
    .WithReference(inspectionsDb)
    .WithReference(rabbitmq)
    .WithReference(roomService)
    .WaitFor(inspectionsDb)
    .WaitFor(rabbitmq);

var notificationService = builder.AddProject<Projects.NotificationCore_API>("notification-service")
    .WithReference(notificationsDb)
    .WithReference(rabbitmq)
    .WithReference(roomService)
    .WaitFor(notificationsDb)
    .WaitFor(rabbitmq);

var telegramService = builder.AddProject<Projects.TelegramAgent_API>("telegram-service")
    .WithReference(telegramDb)
    .WithReference(rabbitmq)
    .WaitFor(telegramDb)
    .WaitFor(rabbitmq);

var bookingService = builder.AddProject<Projects.Booking_API>("booking-service");

var _ = builder.AddProject<Projects.ApiGateway>("api-gateway")
    .WithReference(authService)
    .WithReference(eventService)
    .WithReference(inspectionService)
    .WithReference(roomService)
    .WithReference(notificationService)
    .WithReference(telegramService)
    .WithReference(fileStorageService)
    .WithReference(bookingService);

builder.Build().Run();