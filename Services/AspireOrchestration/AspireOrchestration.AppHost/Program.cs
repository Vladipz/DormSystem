var builder = DistributedApplication.CreateBuilder(args);

var authService = builder.AddProject<Projects.Auth_API>("auth-service");

var eventService = builder.AddProject<Projects.Events_API>("event-service");

var roomService = builder.AddProject<Projects.Rooms_API>("room-service");

var inspectionService = builder.AddProject<Projects.Inspections_API>("inspection-service");

var notificationService = builder.AddProject<Projects.NotificationCore_API>("notification-service");

var telegramService = builder.AddProject<Projects.TelegramAgent_API>("telegram-service");

var fileStorageService = builder.AddProject<Projects.FileStorage_API>("file-storage-service");

var apiGateway = builder.AddProject<Projects.ApiGateway_YARP>("api-gateway")
    .WithReference(authService)
    .WithReference(eventService)
    .WithReference(inspectionService)
    .WithReference(roomService)
    .WithReference(notificationService)
    .WithReference(telegramService)
    .WithReference(fileStorageService);

builder.Build().Run();