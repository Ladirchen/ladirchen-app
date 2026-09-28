using PingPong.Actors;

namespace PingPong.ActorSystem.Tests;

/// <summary>
/// An actor that records every message it receives and lets tests await until a given number of
/// messages have arrived. A lock is needed because the actor's own message pump can run on a
/// different thread pool thread than the test that is awaiting it.
/// </summary>
internal sealed class ProbeActor : Actor
{
    private readonly object _gate = new();
    private readonly List<object> _received = [];
    private TaskCompletionSource? _waiter;
    private int _waitForCount;

    public IReadOnlyList<object> Received
    {
        get { lock (_gate) return [.. _received]; }
    }

    public override Task ReceiveAsync(object message, IActorContext context)
    {
        lock (_gate)
        {
            _received.Add(message);
            if (_waiter is not null && _received.Count >= _waitForCount)
            {
                _waiter.TrySetResult();
                _waiter = null;
            }
        }

        return Task.CompletedTask;
    }

    public Task WaitForAsync(int count, TimeSpan timeout)
    {
        lock (_gate)
        {
            if (_received.Count >= count)
                return Task.CompletedTask;

            _waitForCount = count;
            _waiter = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            return _waiter.Task.WaitAsync(timeout);
        }
    }
}
