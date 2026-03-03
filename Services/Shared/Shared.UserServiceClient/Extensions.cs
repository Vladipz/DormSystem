using Microsoft.Extensions.DependencyInjection;

namespace Shared.UserServiceClient
{

    public static class UserServiceClientExtensions
    {
        /// <summary>
        /// Adds UserServiceClient with Aspire service discovery support.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="serviceUrl">Optional service URL. If null, uses Aspire service discovery with "https+http://auth-service".</param>
        public static IServiceCollection AddUserServiceClient(this IServiceCollection services, string? serviceUrl = null)
        {
            services.AddHttpClient<IAuthServiceClient, HttpAuthServiceClient>(client =>
             {
                 // Aspire service discovery name, or fallback to provided URL
                 var baseUrl = serviceUrl ?? "https+http://auth-service";
                 client.BaseAddress = new Uri(baseUrl);
             })
             .AddServiceDiscovery(); // Enable Aspire service discovery

            return services;
        }
    }

}