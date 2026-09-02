using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Serilog;

namespace LadirchenApp.Webhost.Helpers;

public static class LoggerHelper
{
  public static async Task RunWithSerilogAsync(Func<ILogger, Task> aFunc)
  {
    IConfiguration configuration = BuildBootstrapConfiguration();
    ConfigurationManager bootstrapConfiguration = new();

    foreach (var setting in configuration.AsEnumerable())
    {
      bootstrapConfiguration[setting.Key] = setting.Value;
    }

    var sourceContext = Assembly.GetCallingAssembly().GetName().Name ?? "Webhost";

    Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(bootstrapConfiguration).CreateBootstrapLogger();

    var bootstrapLogger = Log.Logger.ForContext(Serilog.Core.Constants.SourceContextPropertyName, sourceContext);
    bootstrapLogger.Information("*** Starting up Ladierchen Host...");

    try
    {
      await aFunc.Invoke(bootstrapLogger);
      bootstrapLogger.Information("*** Ladierchen Host shut down gracefully.");
    }
    catch (Exception ex)
    {
      bootstrapLogger.Fatal(ex, "*** Unhandled exception in Ladierchen Host!");
    }
    finally
    {
      bootstrapLogger.Information("*** Ladierchen Host Shut down complete!");
      await Log.CloseAndFlushAsync();
    }
  }

  private static IConfiguration BuildBootstrapConfiguration()
  {
    var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

    return new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
      .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false)
      .AddEnvironmentVariables()
      .Build();
  }
}
