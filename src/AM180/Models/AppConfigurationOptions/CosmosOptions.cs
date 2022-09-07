using AM180.Models.Abstractions;

namespace AM180.Models.AppConfigurationOptions;

public sealed class CosmosOptions : Options
{
    public string? Endpoint { get; set; }
    public string? Key { get; set; }
}
