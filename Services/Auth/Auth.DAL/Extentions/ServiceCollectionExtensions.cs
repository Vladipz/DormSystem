using Auth.DAL.Data;
using Auth.DAL.Interfaces;
using Auth.DAL.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.DAL.Extentions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDalServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Check for Aspire connection string first, then fall back to DefaultConnection
            var connectionString = configuration.GetConnectionString("auth-db")
                ?? configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string 'auth-db' or 'DefaultConnection' not found.");
            }

            services.AddDbContext<AuthDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Register repositories
            services.AddScoped<ILinkCodeRepository, LinkCodeRepository>();

            // Apply pending migrations at startup
            var serviceProvider = services.BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
                dbContext.Database.Migrate();
            }

            return services;
        }
    }
}