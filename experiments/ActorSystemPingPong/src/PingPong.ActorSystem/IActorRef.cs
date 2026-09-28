namespace PingPong.Actors;

/// <summary>
/// A lightweight handle to an actor's mailbox. Actors (and outside callers) only ever
/// interact with each other through this reference - never through direct object access.
/// </summary>
public interface IActorRef
{
    string Name { get; }

    /// <summary>
    /// Enqueues <paramref name="message"/> on the target actor's mailbox and returns immediately.
    /// The message is processed asynchronously, one at a time, by the actor's own message loop.
    /// </summary>
    void Tell(object message, IActorRef? sender = null);
}
