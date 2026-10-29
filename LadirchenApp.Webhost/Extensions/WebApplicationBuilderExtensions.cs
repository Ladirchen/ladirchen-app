using System;
using System.IO;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Commands;
using LadirchenApp.Application.Queries;
using LadirchenApp.Webhost.Infrastructure;
using LadirchenApp.Webhost.Infrastructure.Persistence;
using LadirchenApp.Webhost.Realtime;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Mumrich.SpaDevMiddleware.Domain.Contracts;
using Mumrich.SpaDevMiddleware.Extensions;

using Serilog;

namespace LadirchenApp.Webhost.Extensions;

public static class WebApplicationBuilderExtensions
{
  public static void SetupLadierchenWebhost(
    this WebApplicationBuilder webApplicationBuilder,
    ISpaMiddlewareSettings appSettings
  )
  {
    webApplicationBuilder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));
    ConfigureSpaPaths(webApplicationBuilder, appSettings);
    ConfigureLocalSpaToolPath(webApplicationBuilder);
    webApplicationBuilder.SetupSpaMiddleware(appSettings);
    webApplicationBuilder.SetupNswag();
    webApplicationBuilder.Services.ConfigureHttpJsonOptions(options =>
    {
      options.SerializerOptions.AllowOutOfOrderMetadataProperties = true;
    });
    ConfigurePersistence(webApplicationBuilder);
    webApplicationBuilder.Services.AddSignalR();
    webApplicationBuilder.Services
      .AddAuthentication(FamilySessionIdentity.AuthenticationScheme)
      .AddCookie(
        FamilySessionIdentity.AuthenticationScheme,
        options =>
        {
          options.Cookie.HttpOnly = true;
          options.Cookie.Name = "ladirchen.family-session";
          options.Cookie.SameSite = SameSiteMode.Strict;
          options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
          options.Events = new CookieAuthenticationEvents
          {
            OnRedirectToAccessDenied = context =>
            {
              context.Response.StatusCode = StatusCodes.Status403Forbidden;

              return Task.CompletedTask;
            },
            OnRedirectToLogin = context =>
            {
              context.Response.StatusCode = StatusCodes.Status401Unauthorized;

              return Task.CompletedTask;
            },
          };
          options.ExpireTimeSpan = TimeSpan.FromHours(12);
          options.SlidingExpiration = true;
        }
      );
    webApplicationBuilder.Services.AddAuthorization();
    webApplicationBuilder.Services.AddRateLimiter(options =>
    {
      options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
      options.AddPolicy(
        "authentication",
        context => RateLimitPartition.GetFixedWindowLimiter(
          context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
          _ => new FixedWindowRateLimiterOptions
          {
            AutoReplenishment = true,
            PermitLimit = 10,
            QueueLimit = 0,
            Window = TimeSpan.FromMinutes(1),
          }
        )
      );
    });
    webApplicationBuilder.Services.AddSingleton(TimeProvider.System);
    webApplicationBuilder.Services.AddScoped<IFamilyAggregateSnapshotRepository, DatabaseFamilyAggregateSnapshotRepository>();
    webApplicationBuilder.Services.AddScoped<
      IFamilyCommandExecutionCoordinator,
      PostgreSqlFamilyCommandExecutionCoordinator
    >();
    webApplicationBuilder.Services.AddSingleton<IFamilyAggregateChangePublisher, SignalRFamilyAggregateChangePublisher>();
    webApplicationBuilder.Services.AddScoped<AuthenticationService>();
    webApplicationBuilder.Services.AddScoped<GetFamilyAggregateQueryHandler>();
    webApplicationBuilder.Services.AddScoped<GetWeeklyInterestSettlementPlanQueryHandler>();
    webApplicationBuilder.Services.AddScoped<ExecuteFamilyCommandHandler>();
    webApplicationBuilder.Services.AddScoped<
      ICommandHandler<SettleAvailableWeeklyInterestCommand, WeeklyInterestSettlementResult>,
      SettleAvailableWeeklyInterestCommandHandler
    >();
    webApplicationBuilder.Services.AddScoped<SimulateSavingsInterestQueryHandler>();
    webApplicationBuilder.Services.AddHostedService<FamilyAggregateOutboxDispatcher>();
    webApplicationBuilder.Services.AddHostedService<WeeklyInterestSettlementService>();
  }

  private static void ConfigureSpaPaths(
    WebApplicationBuilder webApplicationBuilder,
    ISpaMiddlewareSettings appSettings
  )
  {
    appSettings.BasePublicPath = webApplicationBuilder.Environment.ContentRootPath;

    foreach (var spaSettings in appSettings.SinglePageApps.Values)
    {
      if (!Path.IsPathFullyQualified(spaSettings.SpaRootPath))
      {
        spaSettings.SpaRootPath = Path.GetFullPath(
          Path.Combine(webApplicationBuilder.Environment.ContentRootPath, spaSettings.SpaRootPath)
        );
      }
    }
  }

  private static void ConfigurePersistence(WebApplicationBuilder webApplicationBuilder)
  {
    var connectionString = PostgreSqlConnectionString.Resolve(webApplicationBuilder.Configuration);

    webApplicationBuilder.Services.AddDbContext<LadirchenDbContext>(options =>
      options.UseNpgsql(connectionString)
    );
  }

  private static void ConfigureLocalSpaToolPath(WebApplicationBuilder webApplicationBuilder)
  {
    var executableDirectory = Path.Combine(
      webApplicationBuilder.Environment.ContentRootPath,
      "Apps",
      "ladirchen-app",
      "node_modules",
      ".bin"
    );
    var currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
    var pathEntries = currentPath.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

    if (!Array.Exists(pathEntries, entry => string.Equals(entry, executableDirectory, StringComparison.Ordinal)))
    {
      Environment.SetEnvironmentVariable("PATH", $"{executableDirectory}{Path.PathSeparator}{currentPath}");
    }
  }

  /// <summary>
  ///   Sets up the NSwag/OpenAPI documentation for the application, including configuring API group names if provided.
  /// </summary>
  /// <param name="webApplicationBuilder">The web application builder.</param>
  /// <param name="apiGroupNames">The array of API group names to include in the OpenAPI documentation.</param>
  public static void SetupNswag(this WebApplicationBuilder webApplicationBuilder, string[]? apiGroupNames = null)
  {
    webApplicationBuilder.Services.AddEndpointsApiExplorer();
    webApplicationBuilder.Services.AddOpenApiDocument(options =>
    {

      if (apiGroupNames != null && apiGroupNames.Length > 0)
      {
        options.ApiGroupNames = apiGroupNames;
      }

    });
  }
}
