namespace PingPong.Actors;

/// <summary>
/// A minimal actor system: spawns actors and gives them mailbox-backed references, plus a
/// single place to plug in logging so hosts (console, browser, tests, ...) can capture output
/// however suits them.
/// </summary>
public sealed class ActorSystem
{
    private readonly Action<string> _logSink;

    public ActorSystem(Action<string>? logSink = null)
    {
        _logSink = logSink ?? (_ => { });
    }

    /// <summary>Creates and starts a new actor, returning a reference other actors can send messages to.</summary>
    public IActorRef Spawn(string name, Actor actor) => new LocalActorRef(name, actor, this);

    /// <summary>Writes a line to whatever log sink the host configured (console, DOM, in-memory buffer, ...).</summary>
    public void Log(string message) => _logSink(message);
}
