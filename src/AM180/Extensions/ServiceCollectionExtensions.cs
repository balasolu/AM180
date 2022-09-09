using AM180.Contexts;
using AM180.Factories;
using AM180.Models.AppConfigurationOptions;
using AM180.Providers;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AM180.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppConfigurationOptions(this IServiceCollection services)
    {
        var executingAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        services.AddSingleton<OptionsProvider>();
        using var serviceProvider = services.BuildServiceProvider();
        var optionsProvider = serviceProvider.GetRequiredService<OptionsProvider>();
        if (optionsProvider != null)
        {
            var options = optionsProvider.GetOptions<PostgresOptions>(PostgresOptionsFactory.NAME);
            if (options.ConnectionString != null)
            {
                services.AddDbContextPool<DefaultDbContext>(x =>
                {

                    x.UseNpgsql(options.ConnectionString, x =>
                    {
                        x.MigrationsAssembly(executingAssemblyName);
                    });
                    x.EnableSensitiveDataLogging();
                    x.EnableDetailedErrors();
                });
                services.AddDbContextFactory<DefaultDbContext>(x =>
                {
                    x.UseNpgsql(options.ConnectionString, x =>
                    {
                        x.MigrationsAssembly(executingAssemblyName);
                    });
                    x.EnableSensitiveDataLogging();
                    x.EnableDetailedErrors();
                });
            }
        }
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
