using AM180.Models.Abstractions;

namespace AM180.Factories.Interfaces;

public interface IOptionsFactory
{
    string Name { get; }
    Options BuildOptions(IConfiguration configuration);
}
