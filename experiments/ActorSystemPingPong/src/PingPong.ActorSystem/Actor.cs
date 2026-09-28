namespace PingPong.Actors;

/// <summary>
/// Base class for actors. An actor only ever reacts to messages delivered through its
/// mailbox; it never exposes methods that others could call directly, which is what keeps
/// actor-based systems free of shared-state race conditions.
/// </summary>
public abstract class Actor
{
    /// <summary>
    /// Handles a single message. Actors process one message at a time, so implementations do
    /// not need any locking around their own state.
    /// </summary>
    public abstract Task ReceiveAsync(object message, IActorContext context);
}
