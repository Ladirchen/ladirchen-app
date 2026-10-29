using System;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;

using Npgsql;

namespace LadirchenApp.Webhost.Infrastructure.Persistence;

public static class PostgreSqlConnectionString
{
  public static string Resolve(IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);
    var configured = configuration["DATABASE_URL"]
      ?? configuration["POSTGRES_URL"]
      ?? configuration.GetConnectionString("Ladirchen")
      ?? throw new InvalidOperationException("The Ladirchen PostgreSQL connection string is required.");

    return Normalize(configured);
  }

  public static string Normalize(string configured)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(configured);

    if (
      !Uri.TryCreate(configured, UriKind.Absolute, out var uri)
      || uri.Scheme is not ("postgres" or "postgresql")
    )
    {
      return ConfigureDefaults(new NpgsqlConnectionStringBuilder(configured)).ConnectionString;
    }

    var credentials = uri.UserInfo.Split(':', 2, StringSplitOptions.None);

    if (credentials.Length != 2 || string.IsNullOrWhiteSpace(uri.Host))
    {
      throw new ArgumentException("The PostgreSQL URL is missing credentials or a host.", nameof(configured));
    }

    var builder = new NpgsqlConnectionStringBuilder
    {
      Host = uri.Host,
      Port = uri.IsDefaultPort ? 5432 : uri.Port,
      Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
      Username = Uri.UnescapeDataString(credentials[0]),
      Password = Uri.UnescapeDataString(credentials[1]),
    };

    foreach (var (key, value) in ParseQuery(uri.Query))
    {
      ApplyQueryParameter(builder, key, value);
    }

    if (string.IsNullOrWhiteSpace(builder.Database))
    {
      throw new ArgumentException("The PostgreSQL URL is missing a database name.", nameof(configured));
    }

    return ConfigureDefaults(builder).ConnectionString;
  }

  private static NpgsqlConnectionStringBuilder ConfigureDefaults(NpgsqlConnectionStringBuilder builder)
  {
    builder.Pooling = true;

    if (builder.MaxPoolSize == 100)
    {
      builder.MaxPoolSize = 20;
    }

    if (string.IsNullOrWhiteSpace(builder.ApplicationName))
    {
      builder.ApplicationName = "LadirchenApp";
    }

    return builder;
  }

  private static IReadOnlyDictionary<string, string> ParseQuery(string query)
  {
    var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
    {
      var parts = pair.Split('=', 2, StringSplitOptions.None);
      values[Uri.UnescapeDataString(parts[0])] = parts.Length == 2
        ? Uri.UnescapeDataString(parts[1])
        : string.Empty;
    }

    return values;
  }

  private static void ApplyQueryParameter(
    NpgsqlConnectionStringBuilder builder,
    string key,
    string value
  )
  {
    var connectionStringKey = key.ToLowerInvariant() switch
    {
      "application_name" => "Application Name",
      "channel_binding" => "Channel Binding",
      "connect_timeout" => "Timeout",
      "max_pool_size" => "Maximum Pool Size",
      "min_pool_size" => "Minimum Pool Size",
      "options" => "Options",
      "pooling" => "Pooling",
      "sslmode" => "SSL Mode",
      _ => throw new ArgumentException($"The PostgreSQL URL parameter '{key}' is not supported."),
    };
    builder[connectionStringKey] = value;
  }
}
