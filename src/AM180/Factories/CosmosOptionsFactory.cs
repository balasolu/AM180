using AM180.Factories.Interfaces;
using AM180.Models.Abstractions;
using AM180.Models.AppConfigurationOptions;

namespace AM180.Factories;

public class CosmosOptionsFactory : IOptionsFactory
{
    public const string NAME = "cosmos";
    const string COSMOS_OPTIONS = "APPLICATION:COSMOSOPTIONS";

    public string Name =>
        NAME;

    public Options BuildOptions(IConfiguration configuration)
    {
        var options = new CosmosOptions();
        configuration.GetSection(COSMOS_OPTIONS).Bind(options);
        if (options.Endpoint != null)
            options.Endpoint = options.Endpoint.Trim();
        if (options.Key != null)
            options.Key = options.Key.Trim();
        return options;
    }
}
