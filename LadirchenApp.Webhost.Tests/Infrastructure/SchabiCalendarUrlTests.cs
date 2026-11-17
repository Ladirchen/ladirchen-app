using System;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Infrastructure;

namespace LadirchenApp.Webhost.Tests.Infrastructure;

public sealed class SchabiCalendarUrlTests
{
  [Test]
  public async Task AcceptsOnlySecureSchabiHosts()
  {
    var url = SchabiCalendarUrl.Parse("https://web.schabi.ch/calendar/private-token.ics");

    await Assert.That(url.Host).IsEqualTo("web.schabi.ch");
    await Assert.That(() => SchabiCalendarUrl.Parse("http://web.schabi.ch/calendar.ics"))
      .Throws<ArgumentException>();
    await Assert.That(() => SchabiCalendarUrl.Parse("https://schabi.ch.example.test/calendar.ics"))
      .Throws<ArgumentException>();
  }
}
