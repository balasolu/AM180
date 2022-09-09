using AM180.Factories.Interfaces;
using AM180.Models.Abstractions;
using AM180.Models.AppConfigurationOptions;

namespace AM180.Factories;

public class AzureOptionsFactory : IOptionsFactory
{
    public const string NAME = "azure";
    const string AZURE_OPTIONS = "APPLICATION:AZUREOPTIONS";

    public string Name =>
        NAME;

    public Options BuildOptions(IConfiguration configuration)
    {
        var options = new AzureOptions();
        configuration.GetSection(AZURE_OPTIONS).Bind(options);
        if (options.AppConfig != null)
            options.AppConfig = options.AppConfig.Trim();
        return options;
    }
}
