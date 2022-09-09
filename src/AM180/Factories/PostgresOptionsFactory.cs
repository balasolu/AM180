using AM180.Factories.Interfaces;
using AM180.Models.Abstractions;
using AM180.Models.AppConfigurationOptions;

namespace AM180.Factories;

public class PostgresOptionsFactory : IOptionsFactory
{
    public const string NAME = "postgres";
    const string POSTGRES_OPTIONS = "APPLICATION:POSTGRESOPTIONS";

    public string Name =>
        NAME;

    public Options BuildOptions(IConfiguration configuration)
    {
        var options = new PostgresOptions();
        configuration.GetSection(POSTGRES_OPTIONS).Bind(options);
        if (options.ConnectionString != null)
            options.ConnectionString = options.ConnectionString.Trim();
        return options;
    }
}
