using Microsoft.Extensions.DependencyInjection;

using Shared.RoomServiceClient;

namespace RoomService.Client;

public static class ServiceCollectionExtensions
{
    private const string RoomServiceBaseUrl = "https+http://room-service";

    /// <summary>
    /// Adds RoomServiceClient with Aspire service discovery support.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static IServiceCollection AddRoomServiceClient(
        this IServiceCollection services)
    {
        services.Configure<RoomServiceSettings>(_ =>
        {
        });

        services.AddHttpClient<IRoomService, HttpRoomService>(client =>
        {
            client.BaseAddress = new Uri(RoomServiceBaseUrl);
        })
        .AddServiceDiscovery();

        return services;
    }
}
