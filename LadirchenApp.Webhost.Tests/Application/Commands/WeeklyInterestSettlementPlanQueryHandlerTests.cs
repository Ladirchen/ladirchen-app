using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Queries;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

public sealed class WeeklyInterestSettlementPlanQueryHandlerTests
{
  private static readonly FamilyId _familyId = new(Guid.Parse("25955ff8-5e55-453e-98ec-b614d4ace938"));
  private static readonly FamilyMemberId _administratorId = new("guardian");

  [Test]
  public async Task PlanUsesTypedClosedWeeksAndExcludesSettledPeriods()
  {
    var repository = new TestFamilyAggregateSnapshotRepository();
    repository.Seed(Snapshot("family-profile", 2, new
    {
      members = new[]
      {
        new
        {
          id = _administratorId.Value,
          name = "Guardian",
          role = "guardian",
          guardianAccess = "admin",
          participatesInWeeklyGoal = false,
        },
      },
      pets = Array.Empty<object>(),
      timeZone = "Europe/Zurich",
    }));
    repository.Seed(Snapshot("family-progression", 4, new
    {
      completedWeeklyStreak = 2,
      houseLevel = 1,
      closedWeekOutcomes = new Dictionary<string, string>
      {
        ["2026-W40"] = "completed",
        ["2026-W41"] = "failed",
      },
      weeklyEvaluations = new Dictionary<string, object>
      {
        ["2026-W40"] = new
        {
          evaluatedAt = "2026-10-05T10:00:00+00:00",
          members = new { },
        },
        ["2026-W41"] = new
        {
          evaluatedAt = "2026-10-12T10:00:00+00:00",
          members = new { },
        },
      },
    }));
    repository.Seed(Snapshot("reward-shop", 1, new { rewards = Array.Empty<object>() }));
    repository.Seed(Snapshot("savings", 2, new
    {
      balances = new { guardian = 0 },
      familyCurrencyCode = "CHF",
      goals = Array.Empty<object>(),
      ladirchenPerCurrencyUnit = 1,
      pendingGuardianGifts = Array.Empty<object>(),
      settledInterestPeriods = new[] { "2026-W40" },
      vouchers = Array.Empty<object>(),
    }));
    var handler = new GetWeeklyInterestSettlementPlanQueryHandler(repository);

    var plan = await handler.HandleAsync(_familyId, CancellationToken.None);

    await Assert.That(plan).IsNotNull();
    await Assert.That(plan!.AdministratorId).IsEqualTo(_administratorId);
    await Assert.That(plan.Periods.Select(period => period.Key)).IsEquivalentTo(["2026-W41"]);
  }

  private static FamilyAggregateSnapshot Snapshot(
    string aggregateType,
    int schemaVersion,
    object state
  )
  {
    return new FamilyAggregateSnapshot(
      _familyId,
      aggregateType,
      schemaVersion,
      1,
      JsonSerializer.SerializeToElement(state),
      _administratorId,
      DateTimeOffset.UnixEpoch
    );
  }

}
