using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Shared.FileServiceClient.Models;

namespace Shared.FileServiceClient.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private const string URI_STRING = "https+http://file-storage-service";

        /// <summary>
        /// Adds FileServiceClient services to the service collection with Aspire service discovery support.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddFileServiceClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FileStorageSettings>(
                configuration.GetSection(FileStorageSettings.SectionName));

            services.AddHttpClient<IFileServiceClient, FileServiceClient>(client =>
            {
                client.BaseAddress = new Uri(URI_STRING);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();

#if DEBUG
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

                return handler;
            })
            .AddServiceDiscovery();

            return services;
        }
    }
}
