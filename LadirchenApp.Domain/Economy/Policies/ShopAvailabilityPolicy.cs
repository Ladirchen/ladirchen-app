using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public static class ShopAvailabilityPolicy
{
  public const int ClosingHour = 18;

  public static bool RedemptionIsOpen(DateTimeOffset now, IanaTimeZone zone)
  {

    return zone.LocalTime(now).Hour < ClosingHour;
  }
}
