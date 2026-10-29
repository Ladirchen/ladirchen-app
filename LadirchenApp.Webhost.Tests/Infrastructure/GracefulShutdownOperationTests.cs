using System;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Commands;
using LadirchenApp.Webhost.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace LadirchenApp.Webhost.Tests.Infrastructure;

public sealed class GracefulShutdownOperationTests
{
  [Test]
  public async Task StopSignalDrainsCurrentSettlementAndDoesNotScheduleAnotherCycle()
  {
    var handler = new BlockingWeeklyInterestSettlementHandler();
    using var services = new ServiceCollection()
      .AddSingleton<
        ICommandHandler<SettleAvailableWeeklyInterestCommand, WeeklyInterestSettlementResult>
      >(handler)
      .BuildServiceProvider();
    using var service = new WeeklyInterestSettlementService(
      services.GetRequiredService<IServiceScopeFactory>(),
      NullLogger<WeeklyInterestSettlementService>.Instance
    );
    await service.StartAsync(CancellationToken.None);
    await handler.Started.WaitAsync(TimeSpan.FromSeconds(2));

    var stopTask = service.StopAsync(CancellationToken.None);
    var stoppedBeforeRelease = stopTask.IsCompleted;
    var operationWasCancelled = handler.OperationCancellationToken.IsCancellationRequested;
    handler.Release();
    await stopTask.WaitAsync(TimeSpan.FromSeconds(2));

    await Assert.That(stoppedBeforeRelease).IsFalse();
    await Assert.That(operationWasCancelled).IsFalse();
    await Assert.That(handler.CallCount).IsEqualTo(1);
  }

  [Test]
  public async Task CancelledStopTokenPreventsStartingANewOperation()
  {
    using var stopping = new CancellationTokenSource();
    stopping.Cancel();
    var called = false;

    await Assert.That(async () => await GracefulShutdownOperation.RunAsync(
      _ =>
      {
        called = true;

        return Task.CompletedTask;
      },
      stopping.Token
    )).Throws<OperationCanceledException>();
    await Assert.That(called).IsFalse();
  }

  [Test]
  public async Task OperationTimeoutCancelsWorkAndLeavesShutdownMargin()
  {
    var execution = GracefulShutdownOperation.RunAsync(
      async cancellationToken =>
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken),
      TimeSpan.FromMilliseconds(25),
      CancellationToken.None
    );

    await Assert.That(async () => await execution.WaitAsync(TimeSpan.FromSeconds(2)))
      .Throws<OperationCanceledException>();
    await Assert.That(GracefulShutdownOperation.MaximumDuration)
      .IsEqualTo(TimeSpan.FromSeconds(50));
    await Assert.That(GracefulShutdownOperation.MaximumDuration)
      .IsLessThan(TimeSpan.FromSeconds(60));
  }
}
