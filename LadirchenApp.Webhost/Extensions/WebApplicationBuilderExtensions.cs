using System;
using System.IO;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Commands;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Application.Queries;
using LadirchenApp.Webhost.Infrastructure;
using LadirchenApp.Webhost.Infrastructure.Persistence;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Mumrich.SpaDevMiddleware.Domain.Contracts;
using Mumrich.SpaDevMiddleware.Extensions;

using Serilog;

namespace LadirchenApp.Webhost.Extensions;

public static class WebApplicationBuilderExtensions
{
  private static readonly TimeSpan _webShutdownTimeout = TimeSpan.FromSeconds(25);
  private static readonly TimeSpan _daemonShutdownTimeout = TimeSpan.FromSeconds(55);

  public static void SetupLadierchenWebhost(
    this WebApplicationBuilder webApplicationBuilder,
    ISpaMiddlewareSettings appSettings
  )
  {
    ConfigureSerilog(webApplicationBuilder);
    ConfigureWebEndpoint(webApplicationBuilder);
    ConfigureForwardedHeaders(webApplicationBuilder);
    ConfigureSpaPaths(webApplicationBuilder, appSettings);
    ConfigureLocalSpaToolPath(webApplicationBuilder);
    webApplicationBuilder.SetupSpaMiddleware(appSettings);
    webApplicationBuilder.SetupNswag();
    webApplicationBuilder.Services.ConfigureHttpJsonOptions(options =>
    {
      options.SerializerOptions.AllowOutOfOrderMetadataProperties = true;
    });
    ConfigureSharedServices(
      webApplicationBuilder,
      requireRedisBackplane: RedisConnectionString.IsRequiredForWeb(
        webApplicationBuilder.Configuration
      ),
      shutdownTimeout: _webShutdownTimeout
    );
    ConfigureWebServices(webApplicationBuilder);
  }

  public static void SetupLadierchenDaemon(this WebApplicationBuilder webApplicationBuilder)
  {
    ConfigureSerilog(webApplicationBuilder);
    webApplicationBuilder.WebHost.UseUrls(
      HostingEndpointResolver.ResolveDaemonUrl()
    );
    ConfigureSharedServices(
      webApplicationBuilder,
      requireRedisBackplane: true,
      shutdownTimeout: _daemonShutdownTimeout
    );
    webApplicationBuilder.Services.AddHostedService<FamilyAggregateOutboxDispatcher>();
    webApplicationBuilder.Services.AddHostedService<WeeklyInterestSettlementService>();
  }

  public static void SetupLadierchenMigration(this WebApplicationBuilder webApplicationBuilder)
  {
    ConfigureSerilog(webApplicationBuilder);
    ConfigurePersistence(webApplicationBuilder);
    ConfigureGracefulShutdown(webApplicationBuilder, _webShutdownTimeout);
  }

  private static void ConfigureSharedServices(
    WebApplicationBuilder webApplicationBuilder,
    bool requireRedisBackplane,
    TimeSpan shutdownTimeout
  )
  {
    ConfigurePersistence(webApplicationBuilder);
    webApplicationBuilder.Services.AddLadirchenDataProtection(
      webApplicationBuilder.Configuration
    );
    ConfigureSignalR(webApplicationBuilder, requireRedisBackplane);
    ConfigureHealthChecks(webApplicationBuilder);
    ConfigureGracefulShutdown(webApplicationBuilder, shutdownTimeout);
    ConfigureApplicationServices(webApplicationBuilder);
  }

  private static void ConfigureApplicationServices(WebApplicationBuilder webApplicationBuilder)
  {
    webApplicationBuilder.Services.AddSingleton(TimeProvider.System);
    webApplicationBuilder.Services.AddScoped<ITypedAggregateUnitOfWork, DatabaseTypedAggregateUnitOfWork>();
    webApplicationBuilder.Services.AddScoped<IFamilyRepository, DatabaseFamilyRepository>();
    webApplicationBuilder.Services.AddScoped<IFamilyEconomyRepository, DatabaseFamilyEconomyRepository>();
    webApplicationBuilder.Services.AddScoped<IFamilyProgressionRepository, DatabaseFamilyProgressionRepository>();
    webApplicationBuilder.Services.AddScoped<IHouseCustomizationRepository, DatabaseHouseCustomizationRepository>();
    webApplicationBuilder.Services.AddScoped<
      IFamilyCommandExecutionCoordinator,
      PostgreSqlFamilyCommandExecutionCoordinator
    >();
    webApplicationBuilder.Services.AddScoped<AuthenticationService>();
    webApplicationBuilder.Services.AddScoped<CurrentAuthenticationSessionReader>();
    webApplicationBuilder.Services.AddScoped<FamilyMembershipOutboxWriter>();
    webApplicationBuilder.Services.AddScoped<MemberAccountService>();
    webApplicationBuilder.Services.AddScoped<GetFamilyAggregateQueryHandler>();
    webApplicationBuilder.Services.AddScoped<GetWeeklyInterestSettlementPlanQueryHandler>();
    webApplicationBuilder.Services.AddScoped<ExecuteFamilyCommandHandler>();
    webApplicationBuilder.Services.AddScoped<
      ICommandHandler<SettleAvailableWeeklyInterestCommand, WeeklyInterestSettlementResult>,
      SettleAvailableWeeklyInterestCommandHandler
    >();
    webApplicationBuilder.Services.AddScoped<SimulateSavingsInterestQueryHandler>();
  }

  private static void ConfigureForwardedHeaders(WebApplicationBuilder webApplicationBuilder)
  {
    if (ForwardedHeadersConfiguration.IsEnabled(webApplicationBuilder.Configuration))
    {
      webApplicationBuilder.Services.Configure<ForwardedHeadersOptions>(
        ForwardedHeadersConfiguration.Configure
      );
    }
  }

  private static void ConfigureGracefulShutdown(
    WebApplicationBuilder webApplicationBuilder,
    TimeSpan shutdownTimeout
  )
  {
    webApplicationBuilder.Services.Configure<HostOptions>(options =>
    {
      options.ServicesStopConcurrently = true;
      options.ShutdownTimeout = shutdownTimeout;
    });
  }

  private static void ConfigureHealthChecks(WebApplicationBuilder webApplicationBuilder)
  {
    webApplicationBuilder.Services
      .AddHealthChecks()
      .AddDbContextCheck<LadirchenDbContext>("postgresql", tags: ["ready"]);
  }

  private static void ConfigurePersistence(WebApplicationBuilder webApplicationBuilder)
  {
    var connectionString = PostgreSqlConnectionString.Resolve(webApplicationBuilder.Configuration);

    webApplicationBuilder.Services.AddDbContext<LadirchenDbContext>(options =>
      options.UseNpgsql(connectionString)
    );
  }

  private static void ConfigureSerilog(WebApplicationBuilder webApplicationBuilder)
  {
    webApplicationBuilder.Host.UseSerilog((context, config) =>
      config.ReadFrom.Configuration(context.Configuration)
    );
  }

  private static void ConfigureSignalR(
    WebApplicationBuilder webApplicationBuilder,
    bool requireRedisBackplane
  )
  {
    var signalR = webApplicationBuilder.Services.AddSignalR();
    var redis = RedisConnectionString.Resolve(
      webApplicationBuilder.Configuration,
      requireRedisBackplane
    );

    if (redis is not null)
    {
      signalR.AddStackExchangeRedis(options =>
      {
        options.Configuration = redis;
      });
    }
  }

  private static void ConfigureWebEndpoint(WebApplicationBuilder webApplicationBuilder)
  {
    var webUrl = HostingEndpointResolver.ResolveWebUrl(webApplicationBuilder.Configuration);

    if (webUrl is not null)
    {
      webApplicationBuilder.WebHost.UseUrls(webUrl);
    }
  }

  private static void ConfigureWebServices(WebApplicationBuilder webApplicationBuilder)
  {
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
