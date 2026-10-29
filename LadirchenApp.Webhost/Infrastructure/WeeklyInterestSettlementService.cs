using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Commands;
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
    var repository = scope.ServiceProvider.GetRequiredService<IFamilyAggregateSnapshotRepository>();
    var handler = scope.ServiceProvider.GetRequiredService<ExecuteFamilyCommandHandler>();
    var profile = await repository.GetAsync(familyId, "family-profile", cancellationToken);
    var progression = await repository.GetAsync(familyId, "family-progression", cancellationToken);
    var savings = await repository.GetAsync(familyId, "savings", cancellationToken);

    if (profile is null || progression is null || savings is null)
    {
      return;
    }

    var profileState = JsonNode.Parse(profile.State.GetRawText()) as JsonObject;
    var progressionState = JsonNode.Parse(progression.State.GetRawText()) as JsonObject;
    var savingsState = JsonNode.Parse(savings.State.GetRawText()) as JsonObject;
    var administratorId = (profileState?["members"] as JsonArray)?
      .OfType<JsonObject>()
      .FirstOrDefault(member =>
        member["guardianAccess"]?.GetValue<string>() == "admin"
        && !(member["invitationPending"]?.GetValue<bool>() ?? false)
      )?["id"]?.GetValue<string>();
    var evaluations = progressionState?["weeklyEvaluations"] as JsonObject;
    var settled = (savingsState?["settledInterestPeriods"] as JsonArray)?
      .Select(item => item?.GetValue<string>())
      .Where(item => item is not null)
      .ToHashSet(StringComparer.Ordinal) ?? [];

    if (administratorId is null || evaluations is null)
    {
      return;
    }

    foreach (var periodKey in evaluations.Select(item => item.Key).Where(key => !settled.Contains(key)))
    {
      if (!IsoWeekPeriod.TryParse(periodKey, out var period) || period is null)
      {
        _logger.LogWarning("Ignoring invalid stored interest period {PeriodKey} for family {FamilyId}.", periodKey, familyId.Value);
        continue;
      }

      using var payload = JsonDocument.Parse($"{{\"year\":{period.Year},\"week\":{period.Week}}}");
      var command = new FamilyCommand(
        familyId,
        new FamilyMemberId(administratorId),
        "savings.settle-weekly-interest",
        payload.RootElement.Clone()
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
          periodKey,
          familyId.Value
        );
      }
      catch (FamilyAggregateConflictException exception)
      {
        _logger.LogInformation(
          exception,
          "Interest period {PeriodKey} for family {FamilyId} will be retried after a concurrent update.",
          periodKey,
          familyId.Value
        );
      }
    }
  }
}
