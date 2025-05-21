using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Shared.RoomServiceClient;

namespace RoomService.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoomServiceClient(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.Configure<RoomServiceSettings>(
            configuration.GetSection("RoomServiceSettings"));

        services.AddHttpClient<IRoomService, HttpRoomService>((serviceProvider, client) =>
        {
            // Base address can be set here if needed
            // Any other HTTP client configuration can be done here
        });

        return services;
    }
} 