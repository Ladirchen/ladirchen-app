using System;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Extensions;
using LadirchenApp.Webhost.Helpers;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace LadirchenApp.Webhost;

public static class Program
{
  private static async Task Main(string[] args)
  {
    await LoggerHelper.RunWithSerilogAsync(async logger =>
    {
      var builder = WebApplication.CreateBuilder(args);
      var appSettings = builder.Configuration.Get<AppSettings>();

      ArgumentNullException.ThrowIfNull(appSettings);

      builder.SetupLadierchenWebhost(appSettings);

      logger.Information("*** Setup for Ladirchen webhost complete, building and configuring application..§.");

      var app = builder.Build();

      app.ConfigLadierchenWebhostApp(appSettings, logger);

      logger.Information("*** Configuration for Ladirchen webhost complete, launching application...");

      await app.RunAsync();
    });
  }
}
