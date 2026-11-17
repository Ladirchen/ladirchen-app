using System;

namespace LadirchenApp.Webhost.Infrastructure;

internal static class SchabiCalendarUrl
{
  public static Uri Parse(string value)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(value);

    if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri)
      || uri.Scheme != Uri.UriSchemeHttps
      || !(uri.Host.Equals("schabi.ch", StringComparison.OrdinalIgnoreCase)
        || uri.Host.EndsWith(".schabi.ch", StringComparison.OrdinalIgnoreCase)))
    {
      throw new ArgumentException("A secure Schabi calendar URL is required.", nameof(value));
    }

    return uri;
  }
}
