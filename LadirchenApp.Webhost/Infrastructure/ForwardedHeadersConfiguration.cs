using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;

namespace LadirchenApp.Webhost.Infrastructure;

public static class ForwardedHeadersConfiguration
{
  public static bool IsEnabled(IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    return string.Equals(configuration["VERCEL"], "1", StringComparison.Ordinal);
  }

  public static void Configure(ForwardedHeadersOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
  }
}
