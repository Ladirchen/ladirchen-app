using System.Text.RegularExpressions;
using TUnit.Playwright;

namespace PingPong.Browser.E2ETests;

/// <summary>End-to-end coverage for the Akka.NET variant's Bootsharp browser host.</summary>
public class AkkaPingPongPageTests : PageTest
{
    [Test]
    public async Task Boots_and_shows_ready_status()
    {
        await Page.GotoAsync(TestServer.AkkaBaseUrl);

        await Expect(Page).ToHaveTitleAsync(new Regex("PingPong Akka.NET Actor System"));
        await Expect(Page.Locator("#status")).ToHaveTextAsync("ready", new() { Timeout = 30_000 });
    }

    [Test]
    public async Task Running_a_match_logs_every_rally_and_reports_completion()
    {
        await Page.GotoAsync(TestServer.AkkaBaseUrl);
        await Expect(Page.Locator("#status")).ToHaveTextAsync("ready", new() { Timeout = 30_000 });

        for (var run = 0; run < 3; run++)
        {
            await Page.Locator("#run").ClickAsync();
            await Expect(Page.Locator("#status")).ToHaveTextAsync("done (5 rallies)", new() { Timeout = 30_000 });
        }

        var log = await Page.Locator("#log").InnerTextAsync();
        await Assert.That(log).Contains("starting match -> sending Ping #1");
        await Assert.That(log).Contains("received Ping #5 -> replying Pong #5");
        await Assert.That(log).Contains("received Pong #5");
        await Assert.That(log.Contains("actor system shutdown skipped", StringComparison.Ordinal)).IsFalse();
        await Assert.That(log.Split("starting match -> sending Ping #1", StringSplitOptions.None).Length - 1).IsEqualTo(3);
    }
}
