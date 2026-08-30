using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

using Mumrich.SpaDevMiddleware.Extensions;

using Serilog;

namespace LadirchenApp.Webhost;

public static class Program
{
  private static async Task Main(string[] args)
  {
    var configuration = new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile("appsettings.json", optional: false)
      .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
      .Build();

    Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateBootstrapLogger();

    try
    {
      var builder = WebApplication.CreateBuilder(args);
      var appSettings = builder.Configuration.Get<AppSettings>();

      ArgumentNullException.ThrowIfNull(appSettings);

      builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));
      builder.SetupSpaMiddleware(appSettings);

      var app = builder.Build();

      app.UseSerilogRequestLogging();

      app.MapSinglePageApps(appSettings);

      await app.RunAsync();
    }
    catch (Exception ex)
    {
      Log.Fatal(ex, "Host terminated unexpectedly");
    }
    finally
    {
      await Log.CloseAndFlushAsync();
    }
  }
}
