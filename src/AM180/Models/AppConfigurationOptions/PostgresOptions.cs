using AM180.Models.Abstractions;

namespace AM180.Models.AppConfigurationOptions;

public sealed class PostgresOptions : Options
{
    public string? ConnectionString { get; set; }
}
