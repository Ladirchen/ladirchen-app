using Bootsharp;
using Akka.Actor;
using PingPong.AkkaGame;

/// <summary>
/// Bootsharp entry point for the Akka.NET variant: exposes the same <see cref="PingPongMatch"/> shape as
/// the hand-rolled browser host, so a browser page can run the Akka-actor-based ping-pong match and
/// observe its log output. This is an empirical test of whether Akka.NET's default (thread-pool based)
/// dispatcher works under the single-threaded browser-wasm runtime.
/// </summary>
public static partial class Program
{
    private static readonly Lazy<ActorSystem> System = new(PingPongMatch.CreateActorSystem);

    /// <summary>One log line per actor message processed; subscribe to this from JavaScript.</summary>
    [Export]
    public static event Action<string>? OnLog;

    public static void Main()
    {
        // Nothing to do on boot: the match is started on demand via RunMatchAsync.
    }

    /// <summary>Runs a match with the given number of rallies and returns the final rally count.</summary>
    [Export]
    public static async Task<int> RunMatchAsync(int rallies) =>
        await PingPongMatch.RunAsync(System.Value, rallies, line => OnLog?.Invoke(line));
}
