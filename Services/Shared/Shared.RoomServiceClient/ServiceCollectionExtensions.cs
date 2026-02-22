using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Shared.RoomServiceClient;

namespace RoomService.Client;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds RoomServiceClient with Aspire service discovery support.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="serviceUrl">Optional service URL. If null, uses Aspire service discovery with "https+http://room-service".</param>
    public static IServiceCollection AddRoomServiceClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string? serviceUrl = null)
    {
        services.Configure<RoomServiceSettings>(
            configuration.GetSection("RoomServiceSettings"));

        services.AddHttpClient<IRoomService, HttpRoomService>((serviceProvider, client) =>
        {
            // Aspire service discovery name, or fallback to provided URL
            var baseUrl = serviceUrl ?? "https+http://room-service";
            client.BaseAddress = new Uri(baseUrl);
        })
        .AddServiceDiscovery(); // Enable Aspire service discovery

        return services;
    }
} 