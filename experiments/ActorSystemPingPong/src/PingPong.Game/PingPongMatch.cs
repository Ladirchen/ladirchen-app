using PingPong.Actors;

namespace PingPong.Game;

/// <summary>
/// Wires up a ping actor and a pong actor and runs a single match between them. This is the
/// entry point shared by every host (console app, browser/Bootsharp app, unit tests, ...) so the
/// actual actor-system logic only has to be written - and gets exercised - once.
/// </summary>
public static class PingPongMatch
{
    /// <summary>
    /// Runs a ping-pong match for <paramref name="rallies"/> round trips, streaming each log line
    /// to <paramref name="log"/> as it happens, and completes with the final rally count.
    /// </summary>
    public static Task<int> RunAsync(int rallies, Action<string> log)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(rallies, 1);

        var system = new ActorSystem(log);
        var completed = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pong = system.Spawn("pong", new PongActor());
        var ping = system.Spawn("ping", new PingActor(pong, rallies, finalRally =>
        {
            log($"Match finished after {finalRally} rallies.");
            completed.TrySetResult(finalRally);
        }));

        ping.Tell(new Start());
        return completed.Task;
    }
}
