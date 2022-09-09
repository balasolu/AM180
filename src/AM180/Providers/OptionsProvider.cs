using AM180.Factories.Interfaces;
using AM180.Models.Abstractions;

namespace AM180.Providers;

sealed class OptionsProvider
{
    public OptionsProvider(IConfiguration configuration)
    {
        var optionsTypes = FindOptionsTypes();
        var value = string.Join(" \n ", optionsTypes.Select(e => e.Name));
        foreach (var type in optionsTypes)
        {
            var optionsFactory = Activator.CreateInstance(type) as IOptionsFactory;
            if (optionsFactory != null)
                OptionsStore.Add(optionsFactory.Name, optionsFactory.BuildOptions(configuration));
        }
    }

    public Dictionary<string, Options> OptionsStore { get; set; } = new();

    public T GetOptions<T>(string providerName)
        where T : Options =>
            (T)OptionsStore[providerName];

    public static IEnumerable<Type> FindOptionsTypes()
    {
        var type = typeof(IOptionsFactory);
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && !p.Equals(type));
    }
}
