using Bootsharp;
using PingPong.Game;

/// <summary>
/// Bootsharp entry point: exposes the same <see cref="PingPongMatch"/> used by the console app to
/// JavaScript, so a browser page can run the actor-based ping-pong match and observe its log output.
/// </summary>
public static partial class Program
{
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
        await PingPongMatch.RunAsync(rallies, line => OnLog?.Invoke(line));
}
