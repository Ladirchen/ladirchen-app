using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Queries;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

public sealed class SettleAvailableWeeklyInterestCommandHandler
  : ICommandHandler<SettleAvailableWeeklyInterestCommand, WeeklyInterestSettlementResult>
{
  private readonly ExecuteFamilyCommandHandler _familyCommands;
  private readonly GetWeeklyInterestSettlementPlanQueryHandler _planner;
  private readonly IFamilyAggregateSnapshotRepository _repository;

  public SettleAvailableWeeklyInterestCommandHandler(
    IFamilyAggregateSnapshotRepository repository,
    GetWeeklyInterestSettlementPlanQueryHandler planner,
    ExecuteFamilyCommandHandler familyCommands
  )
  {
    _repository = repository;
    _planner = planner;
    _familyCommands = familyCommands;
  }

  public async Task<WeeklyInterestSettlementResult> HandleAsync(
    SettleAvailableWeeklyInterestCommand command,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(command);
    var attemptedPeriods = 0;
    var settledPeriods = 0;
    var deferredPeriods = 0;
    var failedFamilies = 0;
    var familyIds = await _repository.ListFamilyIdsAsync(cancellationToken);

    foreach (var familyId in familyIds)
    {
      WeeklyInterestSettlementPlan? plan;

      try
      {
        plan = await _planner.HandleAsync(familyId, cancellationToken);
      }
      catch (Exception exception) when (IsInvalidFamilyState(exception))
      {
        failedFamilies++;

        continue;
      }

      if (plan is null)
      {
        continue;
      }

      foreach (var period in plan.Periods)
      {
        attemptedPeriods++;
        var familyCommand = new FamilyCommand(
          CreateCommandId(familyId, period.Key),
          familyId,
          plan.AdministratorId,
          "savings.settle-weekly-interest",
          JsonSerializer.SerializeToElement(new { year = period.Year, week = period.Week })
        );

        try
        {
          await _familyCommands.HandleAsync(familyCommand, cancellationToken);
          settledPeriods++;
        }
        catch (FamilyAggregateConflictException)
        {
          deferredPeriods++;
        }
        catch (Exception exception) when (IsInvalidFamilyState(exception))
        {
          deferredPeriods++;
        }
      }
    }

    return new WeeklyInterestSettlementResult(
      attemptedPeriods,
      settledPeriods,
      deferredPeriods,
      failedFamilies
    );
  }

  private static Guid CreateCommandId(FamilyId familyId, string periodKey)
  {
    var source = Encoding.UTF8.GetBytes($"weekly-interest:{familyId.Value:D}:{periodKey}");
    var hash = SHA256.HashData(source);

    return new Guid(hash.AsSpan(0, 16));
  }

  private static bool IsInvalidFamilyState(Exception exception)
  {
    return exception is
      FamilyCommandException
      or DomainRuleException
      or ArgumentException
      or InvalidOperationException
      or JsonException
      or OverflowException;
  }
}
