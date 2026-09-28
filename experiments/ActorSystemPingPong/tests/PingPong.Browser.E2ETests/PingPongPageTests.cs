using System.Text.RegularExpressions;
using TUnit.Playwright;

namespace PingPong.Browser.E2ETests;

public class PingPongPageTests : PageTest
{
    [Test]
    public async Task Boots_and_shows_ready_status()
    {
        await Page.GotoAsync(TestServer.HandRolledBaseUrl);

        await Expect(Page).ToHaveTitleAsync(new Regex("PingPong Actor System"));
        await Expect(Page.Locator("#status")).ToHaveTextAsync("ready", new() { Timeout = 30_000 });
    }

    [Test]
    public async Task Running_a_match_logs_every_rally_and_reports_completion()
    {
        await Page.GotoAsync(TestServer.HandRolledBaseUrl);
        await Expect(Page.Locator("#status")).ToHaveTextAsync("ready", new() { Timeout = 30_000 });

        await Page.Locator("#run").ClickAsync();

        await Expect(Page.Locator("#status")).ToHaveTextAsync("done (5 rallies)", new() { Timeout = 30_000 });

        var log = await Page.Locator("#log").InnerTextAsync();
        await Assert.That(log).Contains("[ping] serving Ping #1");
        await Assert.That(log).Contains("[pong] received Ping #5 -> replying Pong #5");
        await Assert.That(log).Contains("Match finished after 5 rallies.");
    }
}
