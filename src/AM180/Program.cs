﻿using AM180.Contexts;
using AM180.Data;
using AM180.Extensions;
using AM180.Factories;
using AM180.Handlers;
using AM180.Models.Abstractions;
using AM180.Models.AppConfigurationOptions;
using AM180.Providers;
using AM180.Services.Interfaces;
using AM180.Services;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Serilog.Events;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

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
if (azureOptions == null)
    throw new Exception("AzureOptions is null");

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
        x.AddAppConfigurationOptions();
        x.AddDataProtection()
            .PersistKeysToDbContext<DefaultDbContext>();
        x.AddIdentity<User, Role>(x =>
        {
            x.User.RequireUniqueEmail = true;
            x.Password.RequireDigit = true;
            x.Password.RequireNonAlphanumeric = true;
            x.Password.RequireUppercase = true;
        })
        .AddEntityFrameworkStores<DefaultDbContext>()
        .AddDefaultTokenProviders();
        // redis connection string is 'redis' for docker compose
        var redisConnectionString = "redis";
        x.AddStackExchangeRedisCache(x =>
        {
            x.Configuration = redisConnectionString;
            x.InstanceName = $"{executingAssemblyName}_";
        });
        x.AddSignalR().AddStackExchangeRedis(redisConnectionString);
        x.AddControllers();
        x.AddRazorPages();
        x.AddServerSideBlazor();
        x.AddSingleton<WeatherForecastService>();
        x.AddScoped<IAuthService, AuthService>();
        x.AddScoped<ILocalStorageService, LocalStorageService>();
        x.AddScoped<AuthenticationStateProvider, DefaultRevalidatingServerAuthenticationStateProvider<User>>();
        x.AddScoped<IHostEnvironmentAuthenticationStateProvider>(x =>
        {
            // this is safe because
            //     the `RevalidatingServerAuthenticationStateProvider` extends the `ServerAuthenticationStateProvider`
            var provider = (ServerAuthenticationStateProvider)x.GetRequiredService<AuthenticationStateProvider>();
            return provider;
        });
        x.AddScoped<CircuitHandler, DefaultCircuitHandler>();
    })
    .UseStaticWebAssets();

try
{
    var app = builder.Build();

    await app.MigrateDefaultDbContextAsync();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
    }

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
    app.UseCookiePolicy(new CookiePolicyOptions
    {
        MinimumSameSitePolicy = SameSiteMode.Lax
    });

    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
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
