using System;
using System.Globalization;

namespace LadirchenApp.Domain.Shared;

public sealed record PromotionDeadline(TimeOnly Time)
{
  public bool IsOpen(DateTimeOffset instant, IanaTimeZone zone)
  {

    return TimeOnly.FromDateTime(zone.LocalTime(instant).DateTime) < Time;
  }
}