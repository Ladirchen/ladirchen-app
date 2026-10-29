using System;
using System.Globalization;

namespace LadirchenApp.Domain.Shared;

public sealed record PromotionDeadline
{
  private const string Format = "HH:mm";

  public PromotionDeadline(TimeOnly time)
  {
    Time = time;
  }

  public PromotionDeadline(DateTime localDeadline)
  {
    LocalDeadline = DateTime.SpecifyKind(localDeadline, DateTimeKind.Unspecified);
    Time = TimeOnly.FromDateTime(localDeadline);
  }

  public TimeOnly Time { get; }
  public DateTime? LocalDeadline { get; }

  public static bool TryParse(string? value, out PromotionDeadline? deadline)
  {
    if (TimeOnly.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
    {
      deadline = new PromotionDeadline(time);

      return true;
    }

    deadline = null;

    return false;
  }

  public override string ToString()
  {
    return Time.ToString(Format, CultureInfo.InvariantCulture);
  }

  public bool IsOpen(DateTimeOffset instant, IanaTimeZone zone)
  {
    var local = zone.LocalTime(instant).DateTime;

    if (LocalDeadline is not null)
    {
      return local < LocalDeadline.Value;
    }

    return TimeOnly.FromDateTime(local) < Time;
  }
}
