var builder = DistributedApplication.CreateBuilder(args);

var authService = builder.AddProject<Projects.Auth_API>("auth-service");

var eventService = builder.AddProject<Projects.Events_API>("event-service");

var roomService = builder.AddProject<Projects.Rooms_API>("room-service");

var apiGateway = builder.AddProject<Projects.ApiGateway_YARP>("api-gateway")
    .WithReference(authService)
    .WithReference(eventService)
    .WithReference(roomService);

builder.Build().Run();