using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Queries;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Tests.Application.Commands;

namespace LadirchenApp.Webhost.Tests.Application.Queries;

public sealed class SavingsQueryHandlerTests
{
  private static readonly FamilyId _familyId = new(Guid.Parse("9f210fed-fbc0-40a7-9764-ceaefc304485"));
  private static readonly FamilyMemberId _guardianId = new("guardian");
  private static readonly FamilyMemberId _childId = new("child");

  [Test]
  public async Task SavingsReadModelUsesTypedDomainStateAndLadiStage()
  {
    var repository = CreateRepository();
    var handler = new GetFamilyAggregateQueryHandler(repository, TimeProvider.System);

    var result = await handler.HandleAsync(
      new GetFamilyAggregateQuery(_familyId, "savings"),
      CancellationToken.None
    );

    var state = result?.State ?? throw new InvalidOperationException("The savings read model is missing.");
    var personalGoal = state.GetProperty("goals").EnumerateArray()
      .Single(goal => goal.GetProperty("id").GetString() == "bicycle");
    var limits = personalGoal.GetProperty("commandLimits");
    var voucher = state.GetProperty("vouchers").EnumerateArray().Single();
    await Assert.That(state.GetProperty("availableBalances").GetProperty(_childId.Value).GetDecimal())
      .IsEqualTo(20);
    await Assert.That(personalGoal.GetProperty("projectedInterestCredit").GetDecimal())
      .IsEqualTo(3);
    await Assert.That(personalGoal.GetProperty("remainingAmount").GetDecimal())
      .IsEqualTo(50);
    await Assert.That(personalGoal.GetProperty("withdrawableBalance").GetDecimal())
      .IsEqualTo(45);
    await Assert.That(
      personalGoal.GetProperty("visibleToMemberIds").EnumerateArray().Select(item => item.GetString()!)
    ).IsEquivalentTo([_guardianId.Value, _childId.Value]);
    await Assert.That(limits.GetProperty(_childId.Value).GetProperty("maximumDepositAmount").GetDecimal())
      .IsEqualTo(20);
    await Assert.That(limits.GetProperty(_guardianId.Value).GetProperty("maximumSupportAmount").GetDecimal())
      .IsEqualTo(50);
    await Assert.That(state.GetProperty("sharedSavedBalances").GetProperty(_childId.Value).GetDecimal())
      .IsEqualTo(50);
    await Assert.That(
      voucher.GetProperty("consentableByMemberIds").EnumerateArray().Select(item => item.GetString()!)
    ).IsEquivalentTo([_guardianId.Value]);
    await Assert.That(voucher.GetProperty("redeemableByMemberIds").GetArrayLength())
      .IsEqualTo(0);
  }

  [Test]
  public async Task SavingsInterestSimulationUsesTypedGoalsAndConfiguredMaximumRate()
  {
    var repository = CreateRepository();
    var handler = new SimulateSavingsInterestQueryHandler(repository);

    var result = await handler.HandleAsync(
      new SimulateSavingsInterestQuery(_familyId, _childId, 7, 100, 5),
      CancellationToken.None
    );

    await Assert.That(result.LadiStageId).IsEqualTo("super-ladi");
    await Assert.That(result.LadiStagePosition).IsEqualTo(6);
    await Assert.That(result.InterestRate).IsEqualTo(8);
    await Assert.That(result.ProjectedCredit).IsEqualTo(4);
  }

  private static TestFamilyAggregateSnapshotRepository CreateRepository()
  {
    var repository = new TestFamilyAggregateSnapshotRepository();
    repository.Seed(Snapshot("family-profile", 2, new
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
    repository.Seed(Snapshot("family-progression", 4, new
    {
      maximumSavingsInterestRate = 8,
      memberProgressions = new
      {
        child = new
        {
          ladiStageId = "sun-ladi",
          ladiStagePosition = 4,
          savingsInterestRate = 100,
        },
      },
    }));
    repository.Seed(Snapshot("savings", 2, new
    {
      balances = new
      {
        guardian = 0,
        child = 100,
      },
      familyCurrencyCode = "CHF",
      ladirchenPerCurrencyUnit = 10,
      goals = new object[]
      {
        new
        {
          id = "bicycle",
          title = "Bicycle",
          icon = "bicycle-icon",
          ownerId = _childId.Value,
          target = 100,
          saved = 50,
          starterBonus = 5,
          interestEarned = 0,
          shared = false,
          visibility = "family",
          cheered = false,
          createdAt = "2026-10-01T10:00:00+00:00",
          participantIds = Array.Empty<string>(),
        },
        new
        {
          id = "family-trip",
          title = "Family trip",
          icon = "trip-icon",
          ownerId = "family",
          target = 20,
          saved = 20,
          starterBonus = 10,
          interestEarned = 0,
          shared = true,
          visibility = "family",
          cheered = false,
          createdAt = "2026-10-01T10:00:00+00:00",
          participantIds = new[] { _childId.Value, _guardianId.Value },
          voucherId = "family-trip",
        },
      },
      pendingGuardianGifts = Array.Empty<object>(),
      settledInterestPeriods = Array.Empty<string>(),
      vouchers = new object[]
      {
        new
        {
          id = "family-trip",
          goalId = "family-trip",
          amount = 20,
          ownerId = (string?)null,
          participantIds = new[] { _childId.Value, _guardianId.Value },
          consentedMemberIds = new[] { _childId.Value },
          status = "pending",
        },
      },
    }));
    repository.Seed(Snapshot("reward-shop", 1, new
    {
      rewards = new object[]
      {
        new
        {
          id = "reserved-reward",
          title = "Reserved reward",
          status = "requested",
          requesterId = _childId.Value,
          price = 80,
          category = "gift",
          quantity = 1,
          isVisible = true,
        },
      },
    }));

    return repository;
  }

  private static FamilyAggregateSnapshot Snapshot(string aggregateType, int schemaVersion, object state)
  {
    return new FamilyAggregateSnapshot(
      _familyId,
      aggregateType,
      schemaVersion,
      1,
      JsonSerializer.SerializeToElement(state),
      _guardianId,
      DateTimeOffset.UnixEpoch
    );
  }
}
