using System;
using System.Globalization;

namespace LadirchenApp.Domain.Shared;

public sealed record IanaTimeZone
{
  public IanaTimeZone(string value)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(value);

    if (value != "UTC" && !TimeZoneInfo.TryConvertIanaIdToWindowsId(value, out _))
    {
      throw new ArgumentException("An IANA timezone is required.", nameof(value));
    }

    Zone = TimeZoneInfo.FindSystemTimeZoneById(value);
    Value = value;
  }

  public string Value
  {
    get;
  }
  private TimeZoneInfo Zone
  {
    get;
  }
  public DateTimeOffset LocalTime(DateTimeOffset instant)
  {

    return TimeZoneInfo.ConvertTime(instant, Zone);
  }

  public DateOnly LocalDate(DateTimeOffset instant)
  {

    return DateOnly.FromDateTime(LocalTime(instant).DateTime);
  }
}