using System;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Extensions;
using LadirchenApp.Webhost.Helpers;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace LadirchenApp.Webhost;

public static class Program
{
  private static async Task Main(string[] args)
  {
    var processRole = ApplicationProcessRoleParser.Parse(args);
    var hostArguments = ApplicationProcessRoleParser.RemoveRoleArguments(args);

    await LoggerHelper.RunWithSerilogAsync(async logger =>
    {
      var builder = WebApplication.CreateBuilder(hostArguments);
      AppSettings? appSettings = null;

      if (processRole == ApplicationProcessRole.Web)
      {
        appSettings = builder.Configuration.Get<AppSettings>();
        ArgumentNullException.ThrowIfNull(appSettings);
        builder.SetupLadierchenWebhost(appSettings);
      }
      else if (processRole == ApplicationProcessRole.Daemon)
      {
        builder.SetupLadierchenDaemon();
      }
      else
      {
        builder.SetupLadierchenMigration();
      }

      logger.Information(
        "*** Setup for Ladirchen {ProcessRole} process complete, building application...",
        processRole
      );

      var app = builder.Build();

      if (!app.Environment.IsEnvironment("NSWAG"))
      {
        await app.MigrateLadirchenDatabaseAsync();
      }

      if (processRole == ApplicationProcessRole.Migration)
      {
        logger.Information("*** Ladirchen database migration complete.");

        return;
      }

      if (processRole == ApplicationProcessRole.Web)
      {
        ArgumentNullException.ThrowIfNull(appSettings);
        app.ConfigLadierchenWebhostApp(appSettings, logger);
      }
      else
      {
        app.ConfigLadierchenDaemonApp();
      }

      logger.Information(
        "*** Configuration for Ladirchen {ProcessRole} process complete, launching application...",
        processRole
      );

      await app.RunAsync();
    });
  }
}
