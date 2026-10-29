using System;

using Microsoft.Extensions.Configuration;

namespace LadirchenApp.Webhost.Infrastructure;

public static class HostingEndpointResolver
{
  public const int DefaultDaemonHealthPort = 8081;

  public static string? ResolveWebUrl(IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);
    var configuredPort = configuration["PORT"];

    return string.IsNullOrWhiteSpace(configuredPort)
      ? null
      : CreateUrl(ParsePort(configuredPort, "PORT"));
  }

  public static string ResolveDaemonUrl()
  {
    return CreateUrl(DefaultDaemonHealthPort);
  }

  private static string CreateUrl(int port)
  {
    return $"http://0.0.0.0:{port}";
  }

  private static int ParsePort(string configuredPort, string configurationKey)
  {
    if (
      !int.TryParse(configuredPort, out var port)
      || port is < 1 or > 65535
    )
    {
      throw new InvalidOperationException(
        $"The {configurationKey} configuration value must be a TCP port between 1 and 65535."
      );
    }

    return port;
  }
}
