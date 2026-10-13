using PingPong.Actors;

namespace PingPong.Game;

/// <summary>
/// Serves the first <see cref="Ping"/> and keeps volleying whenever it gets a <see cref="Pong"/> back,
/// until <paramref name="maxRallies"/> is reached, then reports completion via <paramref name="onFinished"/>.
/// </summary>
public sealed class PingActor(IActorRef pong, int maxRallies, Action<int> onFinished) : Actor
{
    public override Task ReceiveAsync(object message, IActorContext context)
    {
        switch (message)
        {
            case Start:
                context.System.Log($"[{context.Self}] serving Ping #1");
                pong.Tell(new Ping(1), context.Self);
                break;

            case Pong(var rally):
                context.System.Log($"[{context.Self}] received Pong #{rally}");
                if (rally >= maxRallies)
                    onFinished(rally);
                else
                    pong.Tell(new Ping(rally + 1), context.Self);
                break;
        }

        return Task.CompletedTask;
    }
}
