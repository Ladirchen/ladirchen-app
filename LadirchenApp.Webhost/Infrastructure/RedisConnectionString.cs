using System;
using System.Globalization;

using Microsoft.Extensions.Configuration;

using StackExchange.Redis;

namespace LadirchenApp.Webhost.Infrastructure;

public static class RedisConnectionString
{
  public const string SignalRChannelPrefix = "LadirchenApp:SignalR";

  public static bool IsRequiredForWeb(IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    return string.Equals(
      configuration["VERCEL_ENV"],
      "production",
      StringComparison.OrdinalIgnoreCase
    );
  }

  public static ConfigurationOptions? Resolve(IConfiguration configuration, bool required)
  {
    ArgumentNullException.ThrowIfNull(configuration);
    var configured = configuration["REDIS_URL"]
      ?? configuration.GetConnectionString("Redis");

    if (string.IsNullOrWhiteSpace(configured))
    {
      if (required)
      {
        throw new InvalidOperationException(
          "The Redis connection string is required for the shared SignalR backplane."
        );
      }

      return null;
    }

    return Normalize(configured);
  }

  public static ConfigurationOptions Normalize(string configured)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(configured);

    if (
      !Uri.TryCreate(configured, UriKind.Absolute, out var uri)
      || uri.Scheme is not ("redis" or "rediss")
    )
    {
      return ConfigureDefaults(ConfigurationOptions.Parse(configured));
    }

    if (string.IsNullOrWhiteSpace(uri.Host))
    {
      throw new ArgumentException("The Redis URL is missing a host.", nameof(configured));
    }

    if (!string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
    {
      throw new ArgumentException(
        "Redis URL query parameters and fragments are not supported.",
        nameof(configured)
      );
    }

    var options = new ConfigurationOptions
    {
      Ssl = uri.Scheme == "rediss",
    };
    options.EndPoints.Add(uri.Host, uri.Port > 0 ? uri.Port : 6379);
    ConfigureCredentials(options, uri.UserInfo);
    ConfigureDatabase(options, uri.AbsolutePath);

    return ConfigureDefaults(options);
  }

  private static void ConfigureCredentials(ConfigurationOptions options, string userInfo)
  {
    if (string.IsNullOrEmpty(userInfo))
    {
      return;
    }

    var credentials = userInfo.Split(':', 2, StringSplitOptions.None);
    var user = Uri.UnescapeDataString(credentials[0]);

    if (!string.IsNullOrEmpty(user))
    {
      options.User = user;
    }

    if (credentials.Length == 2)
    {
      options.Password = Uri.UnescapeDataString(credentials[1]);
    }
  }

  private static void ConfigureDatabase(ConfigurationOptions options, string absolutePath)
  {
    var databaseText = absolutePath.Trim('/');

    if (string.IsNullOrEmpty(databaseText))
    {
      return;
    }

    if (
      !int.TryParse(databaseText, NumberStyles.None, CultureInfo.InvariantCulture, out var database)
      || database < 0
    )
    {
      throw new ArgumentException("The Redis URL database must be a non-negative integer.", nameof(absolutePath));
    }

    options.DefaultDatabase = database;
  }

  private static ConfigurationOptions ConfigureDefaults(ConfigurationOptions options)
  {
    options.AbortOnConnectFail = false;
    options.ClientName ??= "LadirchenApp";
    options.ChannelPrefix = RedisChannel.Literal(SignalRChannelPrefix);

    return options;
  }
}
