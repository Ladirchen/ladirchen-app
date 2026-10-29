using System;
using System.Threading;
using System.Threading.Tasks;

namespace LadirchenApp.Webhost.Infrastructure;

internal static class GracefulShutdownOperation
{
  // Reserve ten seconds of Vercel's 60-second termination window for host teardown.
  private static readonly TimeSpan _maximumDuration = TimeSpan.FromSeconds(50);

  internal static TimeSpan MaximumDuration
  {
    get
    {
      return _maximumDuration;
    }
  }

  internal static Task RunAsync(
    Func<CancellationToken, Task> operation,
    CancellationToken stoppingToken
  )
  {
    return RunAsync(operation, _maximumDuration, stoppingToken);
  }

  internal static async Task RunAsync(
    Func<CancellationToken, Task> operation,
    TimeSpan maximumDuration,
    CancellationToken stoppingToken
  )
  {
    ArgumentNullException.ThrowIfNull(operation);
    stoppingToken.ThrowIfCancellationRequested();
    using var operationCancellation = new CancellationTokenSource(maximumDuration);

    await operation(operationCancellation.Token);
  }
}
