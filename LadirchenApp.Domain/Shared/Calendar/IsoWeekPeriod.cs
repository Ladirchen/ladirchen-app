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
  public string Key
  {
    get
    {
      return $"{Year:D4}-W{Week:D2}";
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

  public static bool TryParse(string? value, out IsoWeekPeriod? period)
  {
    period = null;

    if (
      string.IsNullOrWhiteSpace(value)
      || value.Length != 8
      || value[4] != '-'
      || value[5] != 'W'
      || !int.TryParse(value.AsSpan(0, 4), NumberStyles.None, CultureInfo.InvariantCulture, out var year)
      || !int.TryParse(value.AsSpan(6, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var week)
      || year < 1
      || year > 9999
      || week < 1
      || week > ISOWeek.GetWeeksInYear(year)
    )
    {
      return false;
    }

    period = new IsoWeekPeriod(year, week);

    return true;
  }
}
