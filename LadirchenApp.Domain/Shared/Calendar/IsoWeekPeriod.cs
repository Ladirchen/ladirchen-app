using System;
using System.Globalization;

namespace LadirchenApp.Domain.Shared;

public sealed record IsoWeekPeriod
{
  public IsoWeekPeriod(int year, int week)
  {
    ArgumentOutOfRangeException.ThrowIfLessThan(year, 1);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(year, 9999);
    ArgumentOutOfRangeException.ThrowIfLessThan(week, 1);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(week, ISOWeek.GetWeeksInYear(year));
    Year = year;
    Week = week;
  }
  public int Year
  {
    get;
  }
  public int Week
  {
    get;
  }
  public DateOnly Start
  {
    get
    {

      return DateOnly.FromDateTime(ISOWeek.ToDateTime(Year, Week, DayOfWeek.Monday));
    }
  }

  public bool Contains(DateOnly date)
  {

    return ISOWeek.GetYear(date.ToDateTime(TimeOnly.MinValue)) == Year
    && ISOWeek.GetWeekOfYear(date.ToDateTime(TimeOnly.MinValue)) == Week;
  }

  public static IsoWeekPeriod For(DateOnly date)
  {

    return new(
      ISOWeek.GetYear(date.ToDateTime(TimeOnly.MinValue)), ISOWeek.GetWeekOfYear(date.ToDateTime(TimeOnly.MinValue)));
  }
}