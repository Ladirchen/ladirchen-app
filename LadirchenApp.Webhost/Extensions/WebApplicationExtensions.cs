using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Commands;
using LadirchenApp.Application.Queries;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Helpers;
using LadirchenApp.Webhost.Infrastructure;
using LadirchenApp.Webhost.Models;
using LadirchenApp.Webhost.Realtime;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

using Mumrich.SpaDevMiddleware.Domain.Contracts;
using Mumrich.SpaDevMiddleware.Extensions;

using Serilog;

namespace LadirchenApp.Webhost.Extensions;

public static partial class WebApplicationExtensions
{
  public static void ConfigLadierchenWebhostApp(
    this WebApplication app,
    ISpaMiddlewareSettings appSettings,
    ILogger appLogger,
    string aboutInfoPath = "about"
  )
  {
    if (ForwardedHeadersConfiguration.IsEnabled(app.Configuration))
    {
      app.UseForwardedHeaders();
    }

    app.UseSerilogRequestLogging();
    app.UseNswag();
    app.UseWhenDev(appLogger);
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapLadirchenHealthChecks();
    app.MapAboutInfo(aboutInfoPath);
    app.MapAuthenticationApi();
    app.MapFamilyReadModelApi();
    app.MapFamilyCommandApi();
    app.MapFamilyProjectionApi();
    app.MapHub<FamilyUpdatesHub>("/hubs/family-updates");
    app.MapSinglePageApps(appSettings);
  }

  public static void ConfigLadierchenDaemonApp(this WebApplication app)
  {
    app.UseSerilogRequestLogging();
    app.MapLadirchenHealthChecks();
  }

  public static void MapLadirchenHealthChecks(this WebApplication app)
  {
    app.MapHealthChecks(
        "/health/live",
        new HealthCheckOptions
        {
          Predicate = _ => false,
        }
      )
      .AllowAnonymous();
    app.MapHealthChecks(
        "/health/ready",
        new HealthCheckOptions
        {
          Predicate = registration => registration.Tags.Contains("ready"),
        }
      )
      .AllowAnonymous();
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
