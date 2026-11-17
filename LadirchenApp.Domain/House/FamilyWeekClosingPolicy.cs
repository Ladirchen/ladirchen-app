using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House;

public static class FamilyWeekClosingPolicy
{
  private const int FinalDayClosingHour = 23;

  public static bool CanClose(
    IsoWeekPeriod period,
    IsoWeekPeriod currentPeriod,
    DateTimeOffset localTime,
    int averageEnergy
  )
  {
    ArgumentNullException.ThrowIfNull(period);
    ArgumentNullException.ThrowIfNull(currentPeriod);

    if (period.Start < currentPeriod.Start)
    {
      return true;
    }

    return period == currentPeriod
      && localTime.DayOfWeek == DayOfWeek.Sunday
      && (
        averageEnergy >= HouseEnergyPolicy.MinimumSuccessfulWeekEnergy
        || localTime.Hour >= FinalDayClosingHour
      );
  }
}
