using Akka.Actor;

namespace PingPong.AkkaGame;

/// <summary>Always replies to a <see cref="Ping"/> with a <see cref="Pong"/> carrying the same rally count.</summary>
public sealed class PongActor : ReceiveActor
{
    public PongActor(Action<string> log)
    {
        Receive<Ping>(ping =>
        {
            log($"[{Self.Path.Name}] received Ping #{ping.Rally} -> replying Pong #{ping.Rally}");
            Sender.Tell(new Pong(ping.Rally));
        });
    }
}
