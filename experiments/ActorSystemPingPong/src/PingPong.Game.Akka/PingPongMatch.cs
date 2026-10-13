using Akka.Actor;
using Akka.Configuration;

namespace PingPong.AkkaGame;

/// <summary>
/// Wires up an Akka.NET <see cref="ActorSystem"/> with a ping and a pong actor and runs a match of the
/// requested length, mirroring the shape of <c>PingPong.Game.PingPongMatch</c> (the hand-rolled variant)
/// so both can be hosted identically by console and browser front-ends.
/// </summary>
public static class PingPongMatch
{
    /// <summary>Runs a match of <paramref name="rallies"/> ping/pong exchanges, streaming log lines via <paramref name="log"/>.</summary>
    /// <returns>The rally count the match finished on (equal to <paramref name="rallies"/> on success).</returns>
    public static async Task<int> RunAsync(int rallies, Action<string> log)
    {
        var system = CreateActorSystem();
        try
        {
            return await RunAsync(system, rallies, log).ConfigureAwait(false);
        }
        finally
        {
            await system.Terminate().ConfigureAwait(false);
        }
    }

    /// <summary>Runs a match in an existing actor system, leaving that system alive after the match.</summary>
    public static async Task<int> RunAsync(ActorSystem system, int rallies, Action<string> log)
    {
        if (rallies <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rallies), rallies, "Rally count must be positive.");
        }

        ArgumentNullException.ThrowIfNull(system);
        ArgumentNullException.ThrowIfNull(log);

        var finished = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pong = system.ActorOf(Props.Create(() => new PongActor(log)));
        var ping = system.ActorOf(
            Props.Create(() => new PingActor(pong, rallies, log, count => finished.TrySetResult(count))));

        ping.Tell(new Start());

        return await finished.Task.ConfigureAwait(false);
    }

    /// <summary>Creates an Akka.NET actor system with the configuration required by the demo.</summary>
    public static ActorSystem CreateActorSystem()
    {
        // Passing the default config explicitly (instead of ActorSystem.Create(name) alone) skips Akka's
        // fallback to ConfigurationFactory.Load(), which reads app.config via System.Configuration -
        // an API that throws under browser-wasm, where there is no configuration subsystem.
        // "stdout-loglevel = off" additionally disables Akka's built-in StandardOutLogger, which tries to
        // set Console.ForegroundColor for colorized startup/shutdown messages - unsupported under
        // browser-wasm's console. Actor-level logging still flows entirely through our own `log` sink.
        // "run-by-clr-shutdown-hook = off" stops CoordinatedShutdown from registering a POSIX/CLR process
        // termination signal handler, which browser-wasm cannot support either.
        var config = ConfigurationFactory
            .ParseString("""
                akka {
                  stdout-loglevel = off
                  coordinated-shutdown.run-by-clr-shutdown-hook = off
                }
                """)
            .WithFallback(ConfigurationFactory.Default());
        return ActorSystem.Create("ping-pong-akka", config);
    }
}
