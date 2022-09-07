using AM180.Factories;
using AM180.Models.AppConfigurationOptions;
using AM180.Providers;
using Microsoft.Azure.Cosmos;

namespace AM180.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppConfigurationOptions(this IServiceCollection services)
        {
            services.AddSingleton<OptionsProvider>();
            services.AddCosmosClient();
            return services;
        }

        static IServiceCollection AddCosmosClient(this IServiceCollection services)
        {
            return services.AddSingleton(x =>
            {
                var optionsProvider = x.GetRequiredService<OptionsProvider>();
                if (optionsProvider != null)
                {
                    var options = optionsProvider.GetOptions<CosmosOptions>(CosmosOptionsFactory.NAME);
                    return new CosmosClient(options.Endpoint, options.Key);
                }
                throw new Exception("OptionsProvider is null");
            });
        }
    }
}
