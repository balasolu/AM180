using AM180.Models.Abstractions;

namespace AM180.Models.AppConfigurationOptions;

public sealed class AzureOptions : Options
{
    public string? AppConfig { get; set; }
}
