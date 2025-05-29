using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.FileServiceClient.Models;

namespace Shared.FileServiceClient.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds FileServiceClient services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddFileServiceClient(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure settings
            services.Configure<FileStorageSettings>(
                configuration.GetSection(FileStorageSettings.SectionName));

            // Add HttpClient for FileServiceClient with SSL bypass for development
            services.AddHttpClient<IFileServiceClient, FileServiceClient>(client =>
            {
                // Configure any default client settings here if needed
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                
                // For development: bypass SSL certificate validation
                // WARNING: Only use this in development environments!
#if DEBUG
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif
                
                return handler;
            });

            return services;
        }

        /// <summary>
        /// Adds FileServiceClient services to the service collection with custom settings.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureSettings">Action to configure FileStorageSettings.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddFileServiceClient(this IServiceCollection services, Action<FileStorageSettings> configureSettings)
        {
            // Configure settings
            services.Configure(configureSettings);

            // Add HttpClient for FileServiceClient with SSL bypass for development
            services.AddHttpClient<IFileServiceClient, FileServiceClient>(client =>
            {
                // Configure any default client settings here if needed
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                
                // For development: bypass SSL certificate validation
                // WARNING: Only use this in development environments!
#if DEBUG
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif
                
                return handler;
            });

            return services;
        }
    }
} 