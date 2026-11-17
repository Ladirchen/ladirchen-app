using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LadirchenApp.Webhost.Infrastructure;

public sealed class SchabiCalendarSynchronizationService : BackgroundService
{
  private static readonly TimeSpan _pollingInterval = TimeSpan.FromHours(1);
  private readonly ILogger<SchabiCalendarSynchronizationService> _logger;
  private readonly IServiceScopeFactory _scopeFactory;

  public SchabiCalendarSynchronizationService(
    IServiceScopeFactory scopeFactory,
    ILogger<SchabiCalendarSynchronizationService> logger
  )
  {
    _scopeFactory = scopeFactory;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    try
    {
      await SynchronizeSafelyAsync(stoppingToken);
      using var timer = new PeriodicTimer(_pollingInterval);

      while (await timer.WaitForNextTickAsync(stoppingToken))
      {
        await SynchronizeSafelyAsync(stoppingToken);
      }
    }
    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
  }

  private async Task SynchronizeSafelyAsync(CancellationToken cancellationToken)
  {
    try
    {
      await using var scope = _scopeFactory.CreateAsyncScope();
      var synchronizer = scope.ServiceProvider.GetRequiredService<SchabiCalendarSynchronizer>();
      await GracefulShutdownOperation.RunAsync(
        synchronizer.SynchronizeDueIntegrationsAsync,
        cancellationToken
      );
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    catch (Exception exception)
    {
      _logger.LogError(exception, "The scheduled Schabi calendar synchronization failed.");
    }
  }
}
