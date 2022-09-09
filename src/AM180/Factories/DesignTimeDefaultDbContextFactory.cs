using System;
using System.Collections;
using System.Reflection;
using AM180.Contexts;
using AM180.Models.AppConfigurationOptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AM180.Factories;

	public class DesignTimeDefaultDbContextFactory : IDesignTimeDbContextFactory<DefaultDbContext>
	{
		public DesignTimeDefaultDbContextFactory()
		{
		}

    public DefaultDbContext CreateDbContext(string[] args)
    {
        foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables())
        {
            if (environmentVariable.Key.ToString()!.Contains("APPLICATION") && environmentVariable.Value!.ToString()!.StartsWith('/'))
                Environment.SetEnvironmentVariable(environmentVariable.Key.ToString()!, File.ReadAllText(environmentVariable.Value.ToString()!));
        }

        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        var postgresOptions = new PostgresOptionsFactory().BuildOptions(configuration) as PostgresOptions;
        if (postgresOptions == null || postgresOptions.ConnectionString == null)
            throw new Exception("PostgresOptions is null");
        var optionsBuilder = new DbContextOptionsBuilder<DefaultDbContext>()
            .UseNpgsql(postgresOptions.ConnectionString, x =>
            {
                x.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
            });
        return new DefaultDbContext(optionsBuilder.Options);
    }
}

