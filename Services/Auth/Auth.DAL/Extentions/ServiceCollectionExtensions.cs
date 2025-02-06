using Auth.DAL.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.DAL.Extentions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDalServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string 'DefaultConnection' not found.");
            }

            services.AddDbContext<AuthDbContext>(options =>
                options.UseNpgsql(connectionString));

            return services;
        }
    }
}