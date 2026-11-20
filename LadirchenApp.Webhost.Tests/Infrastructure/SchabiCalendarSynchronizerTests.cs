using System;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Infrastructure;

namespace LadirchenApp.Webhost.Tests.Infrastructure;

public sealed class SchabiCalendarSynchronizerTests
{
  [Test]
  [Arguments(2026, 11, 16, 7, 2026, 11, 23)]
  [Arguments(2026, 11, 19, 14, 2026, 12, 3)]
  [Arguments(2026, 11, 22, 30, 2026, 12, 22)]
  public async Task SynchronizationUsesTheConfiguredRange(
    int year,
    int month,
    int day,
    int syncHorizonDays,
    int expectedYear,
    int expectedMonth,
    int expectedDay
  )
  {
    var periodEnd = SchabiCalendarSynchronizer.GetSynchronizationPeriodEnd(
      new DateOnly(year, month, day),
      syncHorizonDays
    );

    await Assert.That(periodEnd).IsEqualTo(new DateOnly(expectedYear, expectedMonth, expectedDay));
  }

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

  [Test]
  public async Task AppointmentWithoutDescriptionReceivesFallbackDescription()
  {
    var appointment = new SchabiCalendarTask(
      "appointment-1",
      "EN: Test Unit 2",
      string.Empty,
      new DateOnly(2026, 12, 3)
    );

    var description = SchabiCalendarSynchronizer.GetFocusDescription(appointment);

    await Assert.That(description).IsEqualTo("Schabi-Termin am 03.12.2026");
  }

  [Test]
  public async Task AppointmentKeepsItsSchabiDescription()
  {
    var appointment = new SchabiCalendarTask(
      "appointment-1",
      "EN: Test Unit 2",
      "Unit 2 vorbereiten",
      new DateOnly(2026, 12, 3)
    );

    var description = SchabiCalendarSynchronizer.GetFocusDescription(appointment);

    await Assert.That(description).IsEqualTo("Unit 2 vorbereiten");
  }
}
