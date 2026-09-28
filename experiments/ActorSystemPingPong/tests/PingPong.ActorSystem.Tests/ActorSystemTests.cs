using PingPong.Actors;

namespace PingPong.ActorSystem.Tests;

public class ActorSystemTests
{
    [Test]
    public async Task Tell_delivers_message_to_actor_mailbox()
    {
        var system = new PingPong.Actors.ActorSystem();
        var probe = new ProbeActor();
        var actorRef = system.Spawn("probe", probe);

        actorRef.Tell("hello");
        await probe.WaitForAsync(1, TimeSpan.FromSeconds(5));

        await Assert.That(probe.Received).Count().IsEqualTo(1);
        await Assert.That(probe.Received[0]).IsEqualTo("hello");
    }

    [Test]
    public async Task Messages_are_processed_in_send_order()
    {
        var system = new PingPong.Actors.ActorSystem();
        var probe = new ProbeActor();
        var actorRef = system.Spawn("probe", probe);

        for (var i = 0; i < 10; i++)
            actorRef.Tell(i);

        await probe.WaitForAsync(10, TimeSpan.FromSeconds(5));

        await Assert.That(probe.Received).IsEquivalentTo(Enumerable.Range(0, 10).Cast<object>());
    }

    [Test]
    public async Task Context_exposes_self_and_sender()
    {
        var system = new PingPong.Actors.ActorSystem();
        var probe = new ProbeActor();
        var probeRef = system.Spawn("probe", probe);

        IActorRef? observedSelf = null;
        IActorRef? observedSender = null;
        var recorder = new RecordingActor((message, context) =>
        {
            observedSelf = context.Self;
            observedSender = context.Sender;
        });
        var recorderRef = system.Spawn("recorder", recorder);

        recorderRef.Tell("ping", probeRef);
        await recorder.WaitForAsync(1, TimeSpan.FromSeconds(5));

        await Assert.That(observedSelf).IsEqualTo(recorderRef);
        await Assert.That(observedSender).IsEqualTo(probeRef);
    }

    [Test]
    public async Task An_actor_that_throws_logs_and_keeps_processing_further_messages()
    {
        var logLines = new List<string>();
        var system = new PingPong.Actors.ActorSystem(logLines.Add);
        var probe = new ProbeActor();
        var probeRef = system.Spawn("probe", probe);

        var faulty = new RecordingActor((message, context) =>
        {
            if (Equals(message, "boom"))
                throw new InvalidOperationException("boom");

            probeRef.Tell(message);
        });
        var faultyRef = system.Spawn("faulty", faulty);

        faultyRef.Tell("boom");
        faultyRef.Tell("still alive");
        await probe.WaitForAsync(1, TimeSpan.FromSeconds(5));

        await Assert.That(probe.Received).Count().IsEqualTo(1);
        await Assert.That(probe.Received[0]).IsEqualTo("still alive");
        await Assert.That(logLines.Any(l => l.Contains("unhandled exception"))).IsTrue();
    }

    /// <summary>A minimal actor that delegates each message to a supplied callback, for tests that only need one handler.</summary>
    private sealed class RecordingActor(Action<object, IActorContext> onMessage) : Actor
    {
        private readonly object _gate = new();
        private TaskCompletionSource? _waiter;
        private int _count;
        private int _waitForCount;

        public override Task ReceiveAsync(object message, IActorContext context)
        {
            onMessage(message, context);

            lock (_gate)
            {
                _count++;
                if (_waiter is not null && _count >= _waitForCount)
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
                if (_count >= count)
                    return Task.CompletedTask;

                _waitForCount = count;
                _waiter = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                return _waiter.Task.WaitAsync(timeout);
            }
        }
    }
}
