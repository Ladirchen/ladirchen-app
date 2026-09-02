using Microsoft.AspNetCore.Builder;
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
    webApplicationBuilder.SetupSpaMiddleware(appSettings);
    webApplicationBuilder.SetupNswag();
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
