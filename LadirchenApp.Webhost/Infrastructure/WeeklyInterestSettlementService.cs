using System;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Commands;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LadirchenApp.Webhost.Infrastructure;

public sealed class WeeklyInterestSettlementService : BackgroundService
{
  private static readonly TimeSpan SettlementInterval = TimeSpan.FromHours(1);
  private readonly ILogger<WeeklyInterestSettlementService> _logger;
  private readonly IServiceScopeFactory _scopeFactory;

  public WeeklyInterestSettlementService(
    IServiceScopeFactory scopeFactory,
    ILogger<WeeklyInterestSettlementService> logger
  )
  {
    _scopeFactory = scopeFactory;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    await SettleAvailablePeriodsAsync(stoppingToken);
    using var timer = new PeriodicTimer(SettlementInterval);

    while (await timer.WaitForNextTickAsync(stoppingToken))
    {
      await SettleAvailablePeriodsAsync(stoppingToken);
    }
  }

  private async Task SettleAvailablePeriodsAsync(CancellationToken cancellationToken)
  {
    try
    {
      await using var scope = _scopeFactory.CreateAsyncScope();
      var handler = scope.ServiceProvider.GetRequiredService<
        ICommandHandler<SettleAvailableWeeklyInterestCommand, WeeklyInterestSettlementResult>
      >();
      var result = await handler.HandleAsync(new SettleAvailableWeeklyInterestCommand(), cancellationToken);
      _logger.LogInformation(
        "Weekly savings-interest settlement processed {AttemptedPeriods} periods: {SettledPeriods} settled, {DeferredPeriods} deferred and {FailedFamilies} invalid families skipped.",
        result.AttemptedPeriods,
        result.SettledPeriods,
        result.DeferredPeriods,
        result.FailedFamilies
      );
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    catch (Exception exception)
    {
      _logger.LogError(exception, "The scheduled savings-interest settlement failed.");
    }
  }
}
