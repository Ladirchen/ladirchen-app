using System;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Infrastructure;

namespace LadirchenApp.Webhost.Tests.Infrastructure;

public sealed class SchabiCalendarParserTests
{
  [Test]
  public async Task ParsesAndOrdersPersonalSchabiTasks()
  {
    const string calendar = """
      BEGIN:VCALENDAR
      VERSION:2.0
      BEGIN:VEVENT
      UID:math-1
      DTSTART;VALUE=DATE:20261120
      SUMMARY:Mathematik Seite 24
      DESCRIPTION:Aufgaben 1 bis 4 lösen
      END:VEVENT
      BEGIN:VEVENT
      UID:german-1
      DTSTART;VALUE=DATE:20261119
      SUMMARY:Deutsch lesen
      END:VEVENT
      END:VCALENDAR
      """;

    var tasks = SchabiCalendarParser.Parse(calendar);

    await Assert.That(tasks).Count().IsEqualTo(2);
    await Assert.That(tasks[0].ExternalId).IsEqualTo("german-1");
    await Assert.That(tasks[0].DueDate).IsEqualTo(new DateOnly(2026, 11, 19));
    await Assert.That(tasks[1].Title).IsEqualTo("Mathematik Seite 24");
    await Assert.That(tasks[1].Description).IsEqualTo("Aufgaben 1 bis 4 lösen");
  }

  [Test]
  public async Task RejectsAnInvalidCalendar()
  {
    await Assert.That(() => SchabiCalendarParser.Parse("not a calendar"))
      .ThrowsException();
  }

  [Test]
  public async Task IgnoresSchabiDeletedSubscriptionNotices()
  {
    const string calendar = """
      BEGIN:VCALENDAR
      VERSION:2.0
      BEGIN:VEVENT
      UID:deleted-subscription
      DTSTART;VALUE=DATE:20261116
      SUMMARY:Kalender-Abo von Benutzer in Schabi gelöscht. Bitte lösche dieses Abo in deinem Kalender.
      END:VEVENT
      END:VCALENDAR
      """;

    var tasks = SchabiCalendarParser.Parse(calendar);

    await Assert.That(tasks).IsEmpty();
  }

  [Test]
  public async Task IncludesEventAndAttachmentLinksInDescription()
  {
    const string calendar = """
      BEGIN:VCALENDAR
      VERSION:2.0
      BEGIN:VEVENT
      UID:linked-task
      DTSTART;VALUE=DATE:20261120
      SUMMARY:Arbeitsblatt bearbeiten
      DESCRIPTION:Bitte die Aufgaben lösen.
      URL:https://web.schabi.ch/aufgabe/linked-task
      ATTACH:https://web.schabi.ch/dokumente/arbeitsblatt.pdf
      ATTACH:mailto:teacher@example.com
      END:VEVENT
      END:VCALENDAR
      """;

    var tasks = SchabiCalendarParser.Parse(calendar);

    await Assert.That(tasks).Count().IsEqualTo(1);
    await Assert.That(tasks[0].Description).IsEqualTo("""
      Bitte die Aufgaben lösen.

      https://web.schabi.ch/aufgabe/linked-task

      https://web.schabi.ch/dokumente/arbeitsblatt.pdf
      """);
  }
}
