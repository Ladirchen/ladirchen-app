using System;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Infrastructure;

namespace LadirchenApp.Webhost.Tests.Infrastructure;

public sealed class SchabiCalendarSynchronizerTests
{
  [Test]
  public async Task NewIntegrationIsDueImmediately()
  {
    var due = SchabiCalendarSynchronizer.IsScheduledSyncDue(
      new DateOnly(2026, 11, 16),
      new TimeOnly(5, 0),
      DayOfWeek.Thursday,
      new TimeOnly(6, 0),
      null
    );

    await Assert.That(due).IsTrue();
  }

  [Test]
  [Arguments(DayOfWeek.Wednesday, 7, false)]
  [Arguments(DayOfWeek.Thursday, 5, false)]
  [Arguments(DayOfWeek.Thursday, 6, true)]
  public async Task ExistingIntegrationUsesConfiguredWeekdayAndTime(
    DayOfWeek day,
    int hour,
    bool expected
  )
  {
    var today = new DateOnly(2026, 11, 19);
    var due = SchabiCalendarSynchronizer.IsScheduledSyncDue(
      today,
      new TimeOnly(hour, 0),
      day,
      new TimeOnly(6, 0),
      today.AddDays(-7)
    );

    await Assert.That(due).IsEqualTo(expected);
  }

  [Test]
  public async Task IntegrationRunsOnlyOnceOnScheduledDay()
  {
    var today = new DateOnly(2026, 11, 19);
    var due = SchabiCalendarSynchronizer.IsScheduledSyncDue(
      today,
      new TimeOnly(12, 0),
      DayOfWeek.Thursday,
      new TimeOnly(6, 0),
      today
    );

    await Assert.That(due).IsFalse();
  }
}
