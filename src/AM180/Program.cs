using AM180.Data;
using AM180.Extensions;
using AM180.Factories;
using AM180.Models.AppConfigurationOptions;
using Serilog.Events;
using Serilog;
using System.Collections;
using System.Reflection;
using System.Diagnostics;

var executingAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Logger(config =>
    {
        config
            .MinimumLevel.Information()
            .WriteTo.Console();
    });
Log.Logger = loggerConfig.CreateLogger();

Log.Information("updating environment variables...");

foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables())
{
    if (environmentVariable.Key.ToString()!.Contains("APPLICATION") && environmentVariable.Value!.ToString()!.StartsWith('/'))
        Environment.SetEnvironmentVariable(environmentVariable.Key.ToString()!, await File.ReadAllTextAsync(environmentVariable.Value.ToString()!));
}

Log.Information("environment variables updated!");
Log.Information("building configuration...");
// build local config and azure options
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

var azureOptions = new AzureOptionsFactory().BuildOptions(configuration) as AzureOptions;
if (azureOptions == null) throw new Exception("AzureOptions is null");

configuration = new ConfigurationBuilder()
    .AddAzureAppConfiguration(azureOptions.AppConfig).Build();

var options = new WebApplicationOptions()
{
    ApplicationName = executingAssemblyName,
    Args = args,
    WebRootPath = "wwwroot"
};

var builder = WebApplication.CreateBuilder(options);
builder.Host
    .UseSerilog();

builder.WebHost
    .ConfigureAppConfiguration(x =>
    {
        x.AddConfiguration(configuration);
    })
    .ConfigureServices(x =>
    {
        x.AddControllers();
        x.AddAppConfigurationOptions();
        x.AddRazorPages();
        x.AddServerSideBlazor();
        x.AddSingleton<WeatherForecastService>();
    })
    .UseStaticWebAssets();


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

try
{
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
    }

    app.UseStaticFiles();

    app.UseRouting();

    app.MapBlazorHub();
    app.MapFallbackToPage("/_Host");

    await app.RunAsync("http://*:5000");
}
catch (Exception e)
{
    if (e.GetType().Name.Equals("StopTheHostException", StringComparison.Ordinal))
        throw;

    Log.Fatal(e, "host terminated unexpectedly...");
    if (Debugger.IsAttached && Environment.UserInteractive)
    {
        Console.WriteLine(string.Concat(Environment.NewLine, "press any key to exit..."));
        Console.Read();
    }
}
finally
{
    Log.Information("closing and flushing");
    Log.CloseAndFlush();
}
