using Microsoft.Extensions.DependencyInjection;

namespace Shared.UserServiceClient
{
    public static class UserServiceClientExtensions
    {
        // NOTE - may be add overload with explicit URL for non-Aspire environments, but for now we can just use Aspire service discovery in all environments (it will fall back to the configured URL if not running in Aspire)

        /// <summary>
        /// Adds UserServiceClient with Aspire service discovery support.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddUserServiceClient(this IServiceCollection services)
        {
            const string defaultServiceAddress = "https+http://auth-service";
            services.AddHttpClient<IAuthServiceClient, HttpAuthServiceClient>(client =>
             {
                 client.BaseAddress = new Uri(defaultServiceAddress);
             })
             .AddServiceDiscovery(); // Enable Aspire service discovery

            return services;
        }
    }
}