using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Commands;
using LadirchenApp.Application.Queries;
using LadirchenApp.Domain.Shared;

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
      var repository = scope.ServiceProvider.GetRequiredService<IFamilyAggregateSnapshotRepository>();
      var familyIds = await repository.ListFamilyIdsAsync(cancellationToken);

      foreach (var familyId in familyIds)
      {
        await SettleFamilyAsync(familyId, cancellationToken);
      }
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    catch (Exception exception)
    {
      _logger.LogError(exception, "The scheduled savings-interest settlement failed.");
    }
  }

  private async Task SettleFamilyAsync(FamilyId familyId, CancellationToken cancellationToken)
  {
    await using var scope = _scopeFactory.CreateAsyncScope();
    var planner = scope.ServiceProvider.GetRequiredService<GetWeeklyInterestSettlementPlanQueryHandler>();
    var handler = scope.ServiceProvider.GetRequiredService<ExecuteFamilyCommandHandler>();
    var plan = await planner.HandleAsync(familyId, cancellationToken);

    if (plan is null)
    {
      return;
    }

    foreach (var period in plan.Periods)
    {
      var command = new FamilyCommand(
        CreateSettlementCommandId(familyId, period.Key),
        familyId,
        plan.AdministratorId,
        "savings.settle-weekly-interest",
        JsonSerializer.SerializeToElement(new { year = period.Year, week = period.Week })
      );

      try
      {
        await handler.HandleAsync(command, cancellationToken);
      }
      catch (FamilyCommandException exception)
      {
        _logger.LogInformation(
          exception,
          "Interest period {PeriodKey} for family {FamilyId} is not ready for settlement.",
          period.Key,
          familyId.Value
        );
      }
      catch (FamilyAggregateConflictException exception)
      {
        _logger.LogInformation(
          exception,
          "Interest period {PeriodKey} for family {FamilyId} will be retried after a concurrent update.",
          period.Key,
          familyId.Value
        );
      }
    }
  }

  private static Guid CreateSettlementCommandId(FamilyId familyId, string periodKey)
  {
    var source = Encoding.UTF8.GetBytes($"weekly-interest:{familyId.Value:D}:{periodKey}");
    var hash = SHA256.HashData(source);

    return new Guid(hash.AsSpan(0, 16));
  }
}
