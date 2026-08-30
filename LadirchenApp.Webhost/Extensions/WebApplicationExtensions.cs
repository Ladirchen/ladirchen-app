using LadirchenApp.Webhost.Helpers;
using LadirchenApp.Webhost.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

using Mumrich.SpaDevMiddleware.Domain.Contracts;
using Mumrich.SpaDevMiddleware.Extensions;

using Serilog;

namespace LadirchenApp.Webhost.Extensions;

public static class WebApplicationExtensions
{
  public static void ConfigLadierchenWebhostApp(
    this WebApplication app,
    ISpaMiddlewareSettings appSettings,
    ILogger appLogger,
    string aboutInfoPath = "about"
  )
  {
    app.UseSerilogRequestLogging();
    app.UseNswag();
    app.UseWhenDev(appLogger);

    app.MapAboutInfo(aboutInfoPath);
    app.MapSinglePageApps(appSettings);
  }

  public static WebApplication UseNswag(this WebApplication aWebApplication)
  {
    aWebApplication.UseOpenApi(settings =>
      settings.PostProcess = (document, _) =>
      {
        AboutInfo aboutInfo = AssemblyHelper.GetAboutInfoFromAssembly();

        document.Info.Title = aboutInfo.Name;
        document.Info.Version = aboutInfo.SemVer;
      }
    );
    aWebApplication.UseSwaggerUi();

    return aWebApplication;
  }

  public static WebApplication UseWhenDev(this WebApplication aWebApplication, ILogger aLogger)
  {
    aLogger.Information(
      "*** App built (Hosting Environment: {EnvironmentName})",
      aWebApplication.Environment.EnvironmentName
    );

    if (aWebApplication.Environment.IsDevelopment())
    {
      aWebApplication.UseDeveloperExceptionPage();
    }

    return aWebApplication;
  }

  public static WebApplication MapAboutInfo(this WebApplication aWebApplication, string aPath = "about")
  {
    aWebApplication.MapGet(aPath, () => AssemblyHelper.GetAboutInfoFromAssembly()).WithTags("Info");

    return aWebApplication;
  }
}
