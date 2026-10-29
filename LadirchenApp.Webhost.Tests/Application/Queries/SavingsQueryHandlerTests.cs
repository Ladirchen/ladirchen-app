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
      new GetFamilyAggregateQuery(_familyId, "savings", _childId),
      CancellationToken.None
    );

    var state = result?.State ?? throw new InvalidOperationException("The savings read model is missing.");
    var personalGoal = state.GetProperty("goals").EnumerateArray()
      .Single(goal => goal.GetProperty("id").GetString() == "bicycle");
    var cancellingFamilyGoal = state.GetProperty("goals").EnumerateArray()
      .Single(goal => goal.GetProperty("id").GetString() == "family-cancellation");
    var limits = personalGoal.GetProperty("commandLimits");
    var cancellationRequest = cancellingFamilyGoal.GetProperty("cancellationRequest");
    var cancellationLimits = cancellingFamilyGoal.GetProperty("commandLimits").GetProperty(_childId.Value);
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
    ).IsEquivalentTo([_childId.Value]);
    await Assert.That(limits.GetProperty(_childId.Value).GetProperty("maximumDepositAmount").GetDecimal())
      .IsEqualTo(20);
    await Assert.That(limits.TryGetProperty(_guardianId.Value, out _)).IsFalse();
    await Assert.That(state.GetProperty("balances").TryGetProperty(_guardianId.Value, out _)).IsFalse();
    await Assert.That(state.GetProperty("availableBalances").TryGetProperty(_guardianId.Value, out _)).IsFalse();
    await Assert.That(state.GetProperty("goals").EnumerateArray()
      .Any(goal => goal.GetProperty("id").GetString() == "guardian-private"))
      .IsFalse();
    await Assert.That(state.GetProperty("vouchers").EnumerateArray()
      .Any(item => item.GetProperty("goalId").GetString() == "guardian-private"))
      .IsFalse();
    await Assert.That(state.GetProperty("sharedSavedBalances").GetProperty(_childId.Value).GetDecimal())
      .IsEqualTo(50);
    await Assert.That(
      voucher.GetProperty("consentableByMemberIds").EnumerateArray().Select(item => item.GetString()!)
    ).IsEmpty();
    await Assert.That(voucher.GetProperty("redeemableByMemberIds").GetArrayLength())
      .IsEqualTo(0);
    await Assert.That(state.TryGetProperty("appliedOperationIds", out _)).IsFalse();
    await Assert.That(state.TryGetProperty("ledger", out _)).IsFalse();
    await Assert.That(personalGoal.TryGetProperty("refundableContributions", out _)).IsFalse();
    await Assert.That(cancellingFamilyGoal.TryGetProperty("refundableContributions", out _)).IsFalse();
    await Assert.That(cancellationRequest.GetProperty("requestedByMemberId").GetString())
      .IsEqualTo(_childId.Value);
    await Assert.That(
      cancellationRequest.GetProperty("consentedMemberIds").EnumerateArray().Select(item => item.GetString()!)
    ).IsEquivalentTo([_childId.Value]);
    await Assert.That(
      cancellationRequest.GetProperty("pendingMemberIds").EnumerateArray().Select(item => item.GetString()!)
    ).IsEquivalentTo([_guardianId.Value]);
    await Assert.That(cancellationLimits.GetProperty("canRequestCancellation").GetBoolean()).IsFalse();
    await Assert.That(cancellationLimits.GetProperty("canApproveCancellation").GetBoolean()).IsFalse();
    await Assert.That(cancellationLimits.GetProperty("canRejectCancellation").GetBoolean()).IsFalse();
  }

  [Test]
  public async Task GuardianAdministratorCannotReadChildPrivateGoalThroughViewerSelection()
  {
    var handler = new GetFamilyAggregateQueryHandler(CreateRepository(), TimeProvider.System);
    var result = await handler.HandleAsync(
      new GetFamilyAggregateQuery(_familyId, "savings", _guardianId, _childId),
      CancellationToken.None
    );
    var state = result?.State ?? throw new InvalidOperationException("The savings read model is missing.");

    await Assert.That(state.GetProperty("goals").EnumerateArray()
      .Any(goal => goal.GetProperty("id").GetString() == "child-private"))
      .IsFalse();
    await Assert.That(state.GetProperty("goals").EnumerateArray()
      .Any(goal => goal.GetProperty("id").GetString() == "guardian-private"))
      .IsTrue();
    var cancellingFamilyGoal = state.GetProperty("goals").EnumerateArray()
      .Single(goal => goal.GetProperty("id").GetString() == "family-cancellation");
    var limits = cancellingFamilyGoal.GetProperty("commandLimits").GetProperty(_guardianId.Value);
    await Assert.That(limits.GetProperty("canRequestCancellation").GetBoolean()).IsFalse();
    await Assert.That(limits.GetProperty("canApproveCancellation").GetBoolean()).IsTrue();
    await Assert.That(limits.GetProperty("canRejectCancellation").GetBoolean()).IsTrue();
  }

  [Test]
  public async Task ChildCannotSelectGuardianAsReadModelViewer()
  {
    var handler = new GetFamilyAggregateQueryHandler(CreateRepository(), TimeProvider.System);

    await Assert.That(async () =>
      await handler.HandleAsync(
        new GetFamilyAggregateQuery(_familyId, "savings", _childId, _guardianId),
        CancellationToken.None
      )
    ).Throws<DomainRuleException>();
  }

  [Test]
  public async Task MissingProjectionDependencyNeverReturnsRawSavingsState()
  {
    var completeRepository = CreateRepository();
    var incompleteRepository = new TestFamilyAggregateSnapshotRepository();
    incompleteRepository.Seed(completeRepository.Required(_familyId, "family-profile"));
    incompleteRepository.Seed(completeRepository.Required(_familyId, "reward-shop"));
    incompleteRepository.Seed(completeRepository.Required(_familyId, "savings"));
    var handler = new GetFamilyAggregateQueryHandler(incompleteRepository, TimeProvider.System);

    await Assert.That(async () =>
      await handler.HandleAsync(
        new GetFamilyAggregateQuery(_familyId, "savings", _childId),
        CancellationToken.None
      )
    ).Throws<InvalidOperationException>();
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
    await Assert.That(result.ProjectedCredit).IsEqualTo(5);
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
          refundableContributions = new[]
          {
            new { memberId = _childId.Value, amount = 45 },
          },
        },
        new
        {
          id = "child-private",
          title = "Private child goal",
          icon = "private-icon",
          ownerId = _childId.Value,
          target = 10,
          saved = 5,
          starterBonus = 5,
          interestEarned = 0,
          shared = false,
          visibility = "private",
          cheered = false,
          createdAt = "2026-10-01T10:00:00+00:00",
          participantIds = Array.Empty<string>(),
          refundableContributions = Array.Empty<object>(),
        },
        new
        {
          id = "guardian-private",
          title = "Private guardian goal",
          icon = "private-icon",
          ownerId = _guardianId.Value,
          target = 5,
          saved = 5,
          starterBonus = 5,
          interestEarned = 0,
          shared = false,
          visibility = "private",
          cheered = false,
          createdAt = "2026-10-01T10:00:00+00:00",
          participantIds = Array.Empty<string>(),
          refundableContributions = Array.Empty<object>(),
          voucherId = "guardian-private",
        },
        new
        {
          id = "family-cancellation",
          title = "Family cancellation",
          icon = "goal-icon",
          ownerId = "family",
          target = 100,
          saved = 10,
          starterBonus = 10,
          interestEarned = 0,
          shared = true,
          visibility = "family",
          cheered = false,
          createdAt = "2026-10-01T10:00:00+00:00",
          participantIds = new[] { _childId.Value, _guardianId.Value },
          refundableContributions = Array.Empty<object>(),
          cancellationRequest = new
          {
            requestedByMemberId = _childId.Value,
            consentedMemberIds = new[] { _childId.Value },
          },
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
          refundableContributions = Array.Empty<object>(),
          voucherId = "family-trip",
        },
      },
      pendingGuardianGifts = Array.Empty<object>(),
      appliedOperationIds = Array.Empty<string>(),
      ledger = Array.Empty<object>(),
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
        new
        {
          id = "guardian-private",
          goalId = "guardian-private",
          amount = 5,
          ownerId = _guardianId.Value,
          participantIds = Array.Empty<string>(),
          consentedMemberIds = Array.Empty<string>(),
          status = "redeemable",
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
