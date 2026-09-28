using PingPong.Game;

namespace PingPong.ActorSystem.Tests;

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
    public async Task RunAsync_logs_every_ping_and_pong_plus_the_serve_and_completion_lines()
    {
        var log = new List<string>();
        const int rallies = 4;

        await PingPongMatch.RunAsync(rallies, log.Add).WaitAsync(TimeSpan.FromSeconds(10));

        // 1 serve line + 2 lines per rally (ping receives pong, pong receives ping) + 1 completion line.
        await Assert.That(log).Count().IsEqualTo(2 * rallies + 2);
        await Assert.That(log[0]).Contains("serving Ping #1");
        await Assert.That(log[^1]).Contains($"Match finished after {rallies} rallies.");
    }

    [Test]
    public async Task RunAsync_rejects_non_positive_rally_counts()
    {
        await Assert.That(() => PingPongMatch.RunAsync(0, _ => { }))
            .Throws<ArgumentOutOfRangeException>();
    }
}
