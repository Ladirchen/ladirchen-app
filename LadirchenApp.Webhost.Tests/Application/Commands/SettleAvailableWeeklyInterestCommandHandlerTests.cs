using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Commands;
using LadirchenApp.Application.Queries;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

public sealed class SettleAvailableWeeklyInterestCommandHandlerTests
{
  private static readonly FamilyId _familyId = new(Guid.Parse("aa71a979-52c4-4963-99dc-0298d735fbe7"));
  private static readonly FamilyMemberId _guardianId = new("guardian");
  private static readonly FamilyMemberId _childId = new("child");

  [Test]
  public async Task SettlesEachClosedWeekOnlyOnceAcrossRepeatedRuns()
  {
    var repository = CreateRepository(hasClosedWeek: true);
    var handler = CreateHandler(repository);

    var first = await handler.HandleAsync(
      new SettleAvailableWeeklyInterestCommand(),
      CancellationToken.None
    );
    var revisionAfterFirstRun = repository.Required(_familyId, "savings").Revision;
    var second = await handler.HandleAsync(
      new SettleAvailableWeeklyInterestCommand(),
      CancellationToken.None
    );

    await Assert.That(first.AttemptedPeriods).IsEqualTo(1);
    await Assert.That(first.SettledPeriods).IsEqualTo(1);
    await Assert.That(first.DeferredPeriods).IsEqualTo(0);
    await Assert.That(first.FailedFamilies).IsEqualTo(0);
    await Assert.That(second.AttemptedPeriods).IsEqualTo(0);
    await Assert.That(second.SettledPeriods).IsEqualTo(0);
    await Assert.That(second.DeferredPeriods).IsEqualTo(0);
    await Assert.That(second.FailedFamilies).IsEqualTo(0);

    var savings = repository.Required(_familyId, "savings");
    var goal = savings.State.GetProperty("goals").EnumerateArray().Single();
    var settledPeriods = savings.State
      .GetProperty("settledInterestPeriods")
      .EnumerateArray()
      .Select(item => item.GetString())
      .OfType<string>()
      .ToArray();
    await Assert.That(savings.Revision).IsEqualTo(revisionAfterFirstRun);
    await Assert.That(goal.GetProperty("saved").GetDecimal()).IsEqualTo(53m);
    await Assert.That(settledPeriods).IsEquivalentTo(["2026-W41"]);
  }

  [Test]
  public async Task DoesNothingWhenNoFamilyHasAnEligibleClosedWeek()
  {
    var repository = CreateRepository(hasClosedWeek: false);
    var handler = CreateHandler(repository);

    var result = await handler.HandleAsync(
      new SettleAvailableWeeklyInterestCommand(),
      CancellationToken.None
    );

    await Assert.That(result.AttemptedPeriods).IsEqualTo(0);
    await Assert.That(result.SettledPeriods).IsEqualTo(0);
    await Assert.That(result.DeferredPeriods).IsEqualTo(0);
    await Assert.That(result.FailedFamilies).IsEqualTo(0);
    await Assert.That(repository.Required(_familyId, "savings").Revision).IsEqualTo(1);
  }

  [Test]
  public async Task InvalidFamilyStateDoesNotPreventOtherFamiliesFromSettling()
  {
    var repository = CreateRepository(hasClosedWeek: true);
    var invalidFamilyId = new FamilyId(Guid.Parse("8916711c-6684-4449-a9dc-d0e89e5ff56e"));

    foreach (var aggregateType in new[] { "family-profile", "family-progression", "reward-shop", "savings" })
    {
      repository.Seed(Snapshot(invalidFamilyId, aggregateType, 42));
    }

    var handler = CreateHandler(repository);
    var result = await handler.HandleAsync(
      new SettleAvailableWeeklyInterestCommand(),
      CancellationToken.None
    );

    await Assert.That(result.SettledPeriods).IsEqualTo(1);
    await Assert.That(result.FailedFamilies).IsEqualTo(1);
    await Assert.That(repository.Required(_familyId, "savings").Revision).IsEqualTo(2);
  }

  private static SettleAvailableWeeklyInterestCommandHandler CreateHandler(
    TestFamilyAggregateSnapshotRepository repository
  )
  {
    var planner = new GetWeeklyInterestSettlementPlanQueryHandler(repository);
    var familyCommands = new ExecuteFamilyCommandHandler(
      repository,
      new ImmediateFamilyCommandExecutionCoordinator(),
      new FixedTimeProvider(new DateTimeOffset(2026, 10, 12, 10, 0, 0, TimeSpan.Zero))
    );

    return new SettleAvailableWeeklyInterestCommandHandler(repository, planner, familyCommands);
  }

  private static TestFamilyAggregateSnapshotRepository CreateRepository(bool hasClosedWeek)
  {
    var repository = new TestFamilyAggregateSnapshotRepository();
    var closedWeekOutcomes = new Dictionary<string, string>();
    var weeklyEvaluations = new Dictionary<string, object>();

    if (hasClosedWeek)
    {
      closedWeekOutcomes["2026-W41"] = "completed";
      weeklyEvaluations["2026-W41"] = new
      {
        evaluatedAt = "2026-10-12T10:00:00+00:00",
        members = new Dictionary<string, object>
        {
          [_childId.Value] = new
          {
            streakDays = 0,
            completionPercent = 0,
            averageBasicRating = 0,
            participationValue = 1,
            ladiStageId = "sun-ladi",
            ladiStagePosition = 4,
          },
        },
      };
    }

    repository.Seed(Snapshot("family-profile", new
    {
      timeZone = "Europe/Zurich",
      pets = Array.Empty<object>(),
      members = new object[]
      {
        new
        {
          id = _guardianId.Value,
          name = "Guardian",
          role = "guardian",
          guardianAccess = "admin",
          participatesInWeeklyGoal = false,
        },
        new
        {
          id = _childId.Value,
          name = "Child",
          role = "child",
          participatesInWeeklyGoal = true,
        },
      },
    }));
    repository.Seed(Snapshot("family-progression", new
    {
      completedWeeklyStreak = 1,
      houseLevel = 1,
      maximumSavingsInterestRate = 8,
      closedWeekOutcomes,
      weeklyEvaluations,
    }));
    repository.Seed(Snapshot("reward-shop", new { rewards = Array.Empty<object>() }));
    repository.Seed(Snapshot("savings", new
    {
      balances = new { guardian = 0, child = 0 },
      familyCurrencyCode = "CHF",
      goals = new object[]
      {
        new
        {
          id = "bicycle",
          title = "Bicycle",
          icon = "bike-icon",
          ownerId = _childId.Value,
          target = 100,
          saved = 50,
          starterBonus = 5,
          interestEarned = 0,
          shared = false,
          visibility = "private",
          cheered = false,
          createdAt = "2026-10-01T10:00:00+00:00",
          participantIds = Array.Empty<string>(),
          refundableContributions = new[]
          {
            new { memberId = _childId.Value, amount = 45 },
          },
        },
      },
      ladirchenPerCurrencyUnit = 10,
      pendingGuardianGifts = Array.Empty<object>(),
      settledInterestPeriods = Array.Empty<string>(),
      vouchers = Array.Empty<object>(),
    }));

    return repository;
  }

  private static FamilyAggregateSnapshot Snapshot(string aggregateType, object state)
  {
    return Snapshot(_familyId, aggregateType, state);
  }

  private static FamilyAggregateSnapshot Snapshot(
    FamilyId familyId,
    string aggregateType,
    object state
  )
  {
    return new FamilyAggregateSnapshot(
      familyId,
      aggregateType,
      FamilyAggregateSchema.GetVersion(aggregateType),
      1,
      JsonSerializer.SerializeToElement(state),
      _guardianId,
      DateTimeOffset.UnixEpoch
    );
  }
}
