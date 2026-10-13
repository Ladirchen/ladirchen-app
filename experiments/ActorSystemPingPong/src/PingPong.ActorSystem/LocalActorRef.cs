using System.Threading.Channels;

namespace PingPong.Actors;

/// <summary>
/// An actor's mailbox and single-threaded message loop, combined behind an <see cref="IActorRef"/>.
/// Deliberately avoids <c>Task.Run</c>/thread pool usage: the pump only ever awaits the channel and
/// the actor's own async handler, so it works unmodified on single-threaded runtimes such as
/// browser-wasm, driven cooperatively by the current <see cref="SynchronizationContext"/>.
/// </summary>
internal sealed class LocalActorRef : IActorRef, IActorContext
{
    private readonly record struct Envelope(object Message, IActorRef? Sender);

    private readonly Channel<Envelope> _mailbox = Channel.CreateUnbounded<Envelope>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
    });

    private readonly Actor _actor;

    public string Name { get; }

    public ActorSystem System { get; }

    IActorRef IActorContext.Self => this;

    /// <summary>The sender of the message currently being processed, if any.</summary>
    public IActorRef? Sender { get; private set; }

    public LocalActorRef(string name, Actor actor, ActorSystem system)
    {
        Name = name;
        _actor = actor;
        System = system;

        // Fire-and-forget: schedules the message pump on the current context instead of a
        // dedicated thread, so it keeps working when there is no thread pool (e.g. wasm).
        _ = PumpAsync();
    }

    public void Tell(object message, IActorRef? sender = null) =>
        _mailbox.Writer.TryWrite(new Envelope(message, sender));

    private async Task PumpAsync()
    {
        await foreach (var envelope in _mailbox.Reader.ReadAllAsync())
        {
            Sender = envelope.Sender;
            try
            {
                await _actor.ReceiveAsync(envelope.Message, this);
            }
            catch (Exception ex)
            {
                System.Log($"[{Name}] unhandled exception while processing {envelope.Message.GetType().Name}: {ex}");
            }
        }
    }

    public override string ToString() => Name;
}
