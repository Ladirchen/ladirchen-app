using System;
using System.Globalization;

namespace LadirchenApp.Domain.Shared;

public sealed record PromotionDeadline
{
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
