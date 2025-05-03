using Microsoft.Extensions.DependencyInjection;

namespace Shared.UserServiceClient
{

    public static class UserServiceClientExtensions
    {
        public static IServiceCollection AddUserServiceClient(this IServiceCollection services, string baseUrl)
        {
            services.AddHttpClient<IAuthServiceClient, HttpAuthServiceClient>(client =>
             {
                 client.BaseAddress = new Uri(baseUrl);
             });

            return services;
        }
    }

}