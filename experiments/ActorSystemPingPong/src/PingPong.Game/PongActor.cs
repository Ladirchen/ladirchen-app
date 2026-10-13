using PingPong.Actors;

namespace PingPong.Game;

/// <summary>Always replies to a <see cref="Ping"/> with a <see cref="Pong"/> carrying the same rally count.</summary>
public sealed class PongActor : Actor
{
    public override Task ReceiveAsync(object message, IActorContext context)
    {
        if (message is Ping(var rally))
        {
            context.System.Log($"[{context.Self}] received Ping #{rally} -> replying Pong #{rally}");
            context.Sender?.Tell(new Pong(rally), context.Self);
        }

        return Task.CompletedTask;
    }
}
