using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Commands;

namespace LadirchenApp.Webhost.Tests.Infrastructure;

internal sealed class BlockingWeeklyInterestSettlementHandler
  : ICommandHandler<SettleAvailableWeeklyInterestCommand, WeeklyInterestSettlementResult>
{
  private readonly TaskCompletionSource _release = new(
    TaskCreationOptions.RunContinuationsAsynchronously
  );
  private readonly TaskCompletionSource _started = new(
    TaskCreationOptions.RunContinuationsAsynchronously
  );

  public int CallCount { get; private set; }

  public CancellationToken OperationCancellationToken { get; private set; }

  public Task Started
  {
    get
    {
      return _started.Task;
    }
  }

  public async Task<WeeklyInterestSettlementResult> HandleAsync(
    SettleAvailableWeeklyInterestCommand command,
    CancellationToken cancellationToken
  )
  {
    CallCount++;
    OperationCancellationToken = cancellationToken;
    _started.TrySetResult();
    await _release.Task.WaitAsync(cancellationToken);

    return new WeeklyInterestSettlementResult(0, 0, 0, 0);
  }

  public void Release()
  {
    _release.TrySetResult();
  }
}
