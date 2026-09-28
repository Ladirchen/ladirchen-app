using PingPong.AkkaGame;

namespace PingPong.AkkaGame.Tests;

public class PingPongMatchTests
{
    [Test]
    [Arguments(1)]
    [Arguments(3)]
    [Arguments(5)]
    [Arguments(20)]
    public async Task RunAsync_completes_with_the_requested_number_of_rallies(int rallies)
    {
        var log = new List<string>();

        var finalRally = await PingPongMatch.RunAsync(rallies, log.Add).WaitAsync(TimeSpan.FromSeconds(10));

        await Assert.That(finalRally).IsEqualTo(rallies);
    }

    [Test]
    public async Task RunAsync_logs_every_ping_and_pong_exchange()
    {
        var log = new List<string>();
        const int rallies = 4;

        await PingPongMatch.RunAsync(rallies, log.Add).WaitAsync(TimeSpan.FromSeconds(10));

        // 1 "starting match" line + 2 lines per rally (pong receives ping, ping receives pong) for all
        // but the last rally, which only logs the "received Pong" line (no further Ping is sent).
        await Assert.That(log).Contains(line => line.Contains("starting match"));
        await Assert.That(log).Contains(line => line.Contains("received Ping #1"));
        await Assert.That(log).Contains(line => line.Contains($"received Pong #{rallies}"));
    }

    [Test]
    public async Task RunAsync_rejects_non_positive_rally_counts()
    {
        await Assert.That(() => PingPongMatch.RunAsync(0, _ => { }))
            .Throws<ArgumentOutOfRangeException>();
    }
}
