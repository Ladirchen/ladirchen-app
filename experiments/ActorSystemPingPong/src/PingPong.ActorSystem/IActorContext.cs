namespace PingPong.Actors;

/// <summary>
/// Context handed to an actor while it processes a message. Gives access to the actor's own
/// reference and to whoever sent the current message, mirroring the classic actor-model API.
/// </summary>
public interface IActorContext
{
    IActorRef Self { get; }

    IActorRef? Sender { get; }

    ActorSystem System { get; }
}
