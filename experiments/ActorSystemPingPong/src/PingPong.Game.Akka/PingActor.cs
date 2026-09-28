using Akka.Actor;

namespace PingPong.AkkaGame;

/// <summary>
/// Serves the opening <see cref="Start"/>, volleys <see cref="Ping"/> messages to the pong actor, and counts
/// rallies until <paramref name="maxRallies"/> is reached, at which point <paramref name="onFinished"/> fires.
/// </summary>
public sealed class PingActor : ReceiveActor
{
    public PingActor(IActorRef pong, int maxRallies, Action<string> log, Action<int> onFinished)
    {
        Receive<Start>(_ =>
        {
            log($"[{Self.Path.Name}] starting match -> sending Ping #1");
            pong.Tell(new Ping(1), Self);
        });

        Receive<Pong>(reply =>
        {
            log($"[{Self.Path.Name}] received Pong #{reply.Rally}");

            if (reply.Rally >= maxRallies)
            {
                onFinished(reply.Rally);
                pong.Tell(PoisonPill.Instance, Self);
                Context.Stop(Self);
                return;
            }

            var next = reply.Rally + 1;
            log($"[{Self.Path.Name}] sending Ping #{next}");
            Sender.Tell(new Ping(next), Self);
        });
    }
}
