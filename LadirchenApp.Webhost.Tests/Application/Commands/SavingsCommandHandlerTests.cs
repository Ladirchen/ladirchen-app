using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Commands;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

public sealed class SavingsCommandHandlerTests
{
  private static readonly FamilyId _familyId = new(Guid.Parse("92e1f86d-7009-4c5f-9309-2d5d43cfdfb1"));
  private static readonly FamilyMemberId _guardianId = new("guardian");
  private static readonly FamilyMemberId _childId = new("child");

  [Test]
  public async Task DepositUsesReservedFundsAndIssuesPersonalVoucher()
  {
    var repository = CreateRepository(
      SavingsState(balance: 100),
      new
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
      }
    );
    var handler = CreateHandler(repository);

    await ExecuteAsync(handler, _childId, "savings.add-goal", new
    {
      id = "bike",
      ownerId = _childId.Value,
      title = "Bicycle",
      icon = "bike-icon",
      target = 20,
      visibility = "private",
    });
    await ExecuteAsync(handler, _childId, "savings.deposit", new
    {
      id = "bike",
      amount = 15,
    });

    var state = repository.Required(_familyId, "savings").State;
    var goal = state.GetProperty("goals").EnumerateArray().Single();
    var voucher = state.GetProperty("vouchers").EnumerateArray().Single();
    await Assert.That(state.GetProperty("balances").GetProperty(_childId.Value).GetDecimal()).IsEqualTo(85);
    await Assert.That(goal.GetProperty("saved").GetDecimal()).IsEqualTo(20);
    await Assert.That(goal.GetProperty("icon").GetString()).IsEqualTo("bike-icon");
    await Assert.That(voucher.GetProperty("amount").GetDecimal()).IsEqualTo(20);
    await Assert.That(voucher.GetProperty("ownerId").GetString()).IsEqualTo(_childId.Value);
    await Assert.That(voucher.GetProperty("status").GetString()).IsEqualTo("redeemable");

    await ExecuteAsync(handler, _childId, "savings.add-goal", new
    {
      id = "camera",
      ownerId = _childId.Value,
      title = "Camera",
      icon = "camera-icon",
      target = 20,
      visibility = "private",
    });
    await Assert.That(async () =>
      await ExecuteAsync(handler, _childId, "savings.deposit", new
      {
        id = "camera",
        amount = 6,
      })
    ).Throws<DomainRuleException>();
    await Assert.That(
      repository.Required(_familyId, "savings")
        .State
        .GetProperty("balances")
        .GetProperty(_childId.Value)
        .GetDecimal()
    ).IsEqualTo(85);
    await Assert.That(repository.LastRevisionGuards.Select(item => item.AggregateType))
      .IsEquivalentTo(["family-profile", "reward-shop"]);
  }

  [Test]
  public async Task GuardianCannotExecuteOwnerPrivateSavingsCommandAsChild()
  {
    var repository = CreateRepository(SavingsState(balance: 20), new { rewards = Array.Empty<object>() });
    var handler = CreateHandler(repository);
    await ExecuteAsync(handler, _childId, "savings.add-goal", new
    {
      id = "private-bike",
      ownerId = _childId.Value,
      title = "Private bicycle",
      icon = "bike-icon",
      target = 20,
      visibility = "private",
    });

    await Assert.That(async () =>
      await ExecuteAsync(
        handler,
        _childId,
        "savings.deposit",
        new { id = "private-bike", amount = 5 },
        _guardianId
      )
    ).Throws<DomainRuleException>();

    var state = repository.Required(_familyId, "savings").State;
    await Assert.That(state.GetProperty("balances").GetProperty(_childId.Value).GetDecimal())
      .IsEqualTo(20);
    await Assert.That(state.GetProperty("goals").EnumerateArray().Single().GetProperty("saved").GetDecimal())
      .IsEqualTo(5);
  }

  [Test]
  public async Task FamilyVoucherRequiresEverySelectedParticipantAndCanBeRedeemedOnce()
  {
    var repository = CreateRepository(SavingsState(balance: 0), new { rewards = Array.Empty<object>() });
    var handler = CreateHandler(repository);

    await ExecuteAsync(handler, _guardianId, "savings.add-goal", new
    {
      id = "family-trip",
      ownerId = "family",
      title = "Family trip",
      icon = "trip-icon",
      target = 20,
      visibility = "family",
      participantScope = "selected",
      participantIds = new[] { _childId.Value, _guardianId.Value },
    });
    await ExecuteAsync(handler, _guardianId, "savings.support-goal", new
    {
      id = "family-trip",
      amount = 10,
    });
    await ExecuteAsync(handler, _childId, "savings.consent-voucher", new
    {
      id = "family-trip",
    });

    await Assert.That(VoucherStatus(repository)).IsEqualTo("pending");
    await ExecuteAsync(handler, _guardianId, "savings.consent-voucher", new
    {
      id = "family-trip",
    });
    await Assert.That(VoucherStatus(repository)).IsEqualTo("redeemable");
    await ExecuteAsync(handler, _childId, "savings.redeem-voucher", new
    {
      id = "family-trip",
    });
    await Assert.That(VoucherStatus(repository)).IsEqualTo("redeemed");

    await Assert.That(async () =>
      await ExecuteAsync(handler, _childId, "savings.redeem-voucher", new
      {
        id = "family-trip",
      })
    ).Throws<DomainRuleException>();
  }

  [Test]
  public async Task WeeklySettlementUsesStoredLadiStageAndConfiguredMaximumRate()
  {
    var savings = SavingsState(
      balance: 0,
      goals:
      [
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
      ]
    );
    var repository = CreateRepository(savings, new { rewards = Array.Empty<object>() });
    repository.Seed(Snapshot("family-progression", 4, JsonSerializer.Deserialize<object>(
      $$"""
      {
        "maximumSavingsInterestRate": 8,
        "closedWeekOutcomes": {
          "2026-W41": "completed"
        },
        "weeklyEvaluations": {
          "2026-W41": {
            "evaluatedAt": "2026-10-12T10:00:00+00:00",
            "members": {
              "{{_childId.Value}}": {
                "streakDays": 0,
                "completionPercent": 0,
                "averageBasicRating": 0,
                "participationValue": 1,
                "ladiStageId": "sun-ladi",
                "ladiStagePosition": 4
              }
            }
          }
        }
      }
      """
    )!));
    var handler = CreateHandler(repository);

    await ExecuteAsync(handler, _guardianId, "savings.settle-weekly-interest", new
    {
      year = 2026,
      week = 41,
    });

    var goal = repository.Required(_familyId, "savings")
      .State
      .GetProperty("goals")
      .EnumerateArray()
      .Single();
    await Assert.That(goal.GetProperty("saved").GetDecimal()).IsEqualTo(53);
    await Assert.That(goal.GetProperty("lastInterestRate").GetDecimal()).IsEqualTo(5.33m);
    await Assert.That(goal.GetProperty("lastInterestPeriod").GetString()).IsEqualTo("2026-W41");
    await Assert.That(repository.LastRevisionGuards.Select(item => item.AggregateType))
      .IsEquivalentTo(["family-profile", "reward-shop", "family-progression"]);
  }

  [Test]
  public async Task FamilyGoalSnapshotWithoutParticipantsIsRejected()
  {
    var repository = CreateRepository(
      SavingsState(
        balance: 0,
        goals:
        [
          new
          {
            id = "legacy-family-goal",
            title = "Legacy family goal",
            ownerId = "family",
            target = 100,
            saved = 10,
            starterBonus = 10,
            interestEarned = 0,
            visibility = "family",
            cheered = false,
            createdAt = "2026-10-01T10:00:00+00:00",
          },
        ]
      ),
      new { rewards = Array.Empty<object>() }
    );
    var handler = CreateHandler(repository);

    await Assert.That(async () =>
      await ExecuteAsync(handler, _guardianId, "savings.toggle-cheer", new
      {
        id = "legacy-family-goal",
      })
    ).Throws<FamilyCommandException>();
  }

  [Test]
  public async Task FamilyGoalSnapshotWithoutStoredStarterBonusIsRejected()
  {
    var repository = CreateRepository(
      SavingsState(
        balance: 0,
        goals:
        [
          new
          {
            id = "legacy-family-goal",
            title = "Legacy family goal",
            ownerId = "family",
            target = 100,
            saved = 10,
            interestEarned = 0,
            visibility = "family",
            cheered = false,
            createdAt = "2026-10-01T10:00:00+00:00",
            participantIds = new[] { _childId.Value, _guardianId.Value },
          },
        ]
      ),
      new { rewards = Array.Empty<object>() }
    );
    var handler = CreateHandler(repository);

    await Assert.That(async () =>
      await ExecuteAsync(handler, _guardianId, "savings.toggle-cheer", new
      {
        id = "legacy-family-goal",
      })
    ).Throws<FamilyCommandException>();
  }

  [Test]
  public async Task StoredGoalWithUnattributedNonBonusSavingsIsRejected()
  {
    var repository = CreateRepository(
      SavingsState(
        balance: 20,
        goals:
        [
          new
          {
            id = "unattributed-goal",
            title = "Unattributed goal",
            ownerId = _childId.Value,
            target = 100,
            saved = 15,
            starterBonus = 5,
            interestEarned = 0,
            shared = false,
            visibility = "private",
            cheered = false,
            createdAt = "2026-10-01T10:00:00+00:00",
            participantIds = Array.Empty<string>(),
          },
        ]
      ),
      new { rewards = Array.Empty<object>() }
    );
    var handler = CreateHandler(repository);

    await Assert.That(async () =>
      await ExecuteAsync(handler, _childId, "savings.toggle-cheer", new
      {
        id = "unattributed-goal",
      })
    ).Throws<FamilyCommandException>();
  }

  [Test]
  public async Task RefundableDepositProvenanceSurvivesReloadAndCancellation()
  {
    var repository = CreateRepository(SavingsState(balance: 100), new { rewards = Array.Empty<object>() });
    var handler = CreateHandler(repository);

    await ExecuteAsync(handler, _childId, "savings.add-goal", new
    {
      id = "refundable-goal",
      ownerId = _childId.Value,
      title = "Refundable goal",
      icon = "goal-icon",
      target = 100,
      visibility = "private",
    });
    await ExecuteAsync(handler, _childId, "savings.deposit", new
    {
      id = "refundable-goal",
      amount = 30,
    });

    var afterDeposit = repository.Required(_familyId, "savings");
    var storedContribution = afterDeposit.State
      .GetProperty("goals")
      .EnumerateArray()
      .Single()
      .GetProperty("refundableContributions")
      .EnumerateArray()
      .Single();
    await Assert.That(afterDeposit.SchemaVersion).IsEqualTo(4);
    await Assert.That(storedContribution.GetProperty("memberId").GetString()).IsEqualTo(_childId.Value);
    await Assert.That(storedContribution.GetProperty("amount").GetDecimal()).IsEqualTo(30);

    await ExecuteAsync(handler, _childId, "savings.cancel-goal", new
    {
      id = "refundable-goal",
    });

    var afterCancellation = repository.Required(_familyId, "savings").State;
    var refund = afterCancellation.GetProperty("ledger")
      .EnumerateArray()
      .Single(item => item.GetProperty("reason").GetString() == "Goal cancellation refund");
    var forfeited = afterCancellation.GetProperty("ledger")
      .EnumerateArray()
      .Single(item => item.GetProperty("kind").GetString() == nameof(LedgerKind.BonusForfeited));
    await Assert.That(afterCancellation.GetProperty("balances").GetProperty(_childId.Value).GetDecimal())
      .IsEqualTo(100);
    await Assert.That(afterCancellation.GetProperty("goals").GetArrayLength()).IsEqualTo(0);
    await Assert.That(refund.GetProperty("memberId").GetString()).IsEqualTo(_childId.Value);
    await Assert.That(refund.GetProperty("amount").GetDecimal()).IsEqualTo(30);
    await Assert.That(forfeited.GetProperty("amount").GetDecimal()).IsEqualTo(5);
  }

  [Test]
  public async Task FamilyGoalCancellationRequestPersistsAndRequiresUnanimousApproval()
  {
    var repository = CreateRepository(SavingsState(balance: 100), new { rewards = Array.Empty<object>() });
    var handler = CreateHandler(repository);

    await ExecuteAsync(handler, _guardianId, "savings.add-goal", new
    {
      id = "family-goal",
      ownerId = "family",
      title = "Family goal",
      icon = "goal-icon",
      target = 100,
      visibility = "family",
      participantScope = "selected",
      participantIds = new[] { _childId.Value, _guardianId.Value },
    });
    await ExecuteAsync(handler, _childId, "savings.gift-to-goal", new
    {
      id = "family-goal",
      amount = 30,
    });

    await Assert.That(async () =>
      await ExecuteAsync(handler, _childId, "savings.cancel-goal", new
      {
        id = "family-goal",
      })
    ).Throws<DomainRuleException>();

    await ExecuteAsync(handler, _childId, "savings.request-goal-cancellation", new
    {
      id = "family-goal",
    });

    var requestedGoal = repository.Required(_familyId, "savings")
      .State
      .GetProperty("goals")
      .EnumerateArray()
      .Single();
    var request = requestedGoal.GetProperty("cancellationRequest");
    await Assert.That(request.GetProperty("requestedByMemberId").GetString()).IsEqualTo(_childId.Value);
    await Assert.That(
      request.GetProperty("consentedMemberIds").EnumerateArray().Select(item => item.GetString()!)
    ).IsEquivalentTo([_childId.Value]);

    await ExecuteAsync(handler, _guardianId, "savings.decide-goal-cancellation", new
    {
      id = "family-goal",
      approved = false,
    });

    var rejectedGoal = repository.Required(_familyId, "savings")
      .State
      .GetProperty("goals")
      .EnumerateArray()
      .Single();
    await Assert.That(rejectedGoal.TryGetProperty("cancellationRequest", out _)).IsFalse();
    await Assert.That(rejectedGoal.GetProperty("saved").GetDecimal()).IsEqualTo(40);

    await ExecuteAsync(handler, _childId, "savings.request-goal-cancellation", new
    {
      id = "family-goal",
    });
    await ExecuteAsync(handler, _guardianId, "savings.decide-goal-cancellation", new
    {
      id = "family-goal",
      approved = true,
    });

    var cancelled = repository.Required(_familyId, "savings").State;
    var refund = cancelled.GetProperty("ledger")
      .EnumerateArray()
      .Single(item => item.GetProperty("reason").GetString() == "Goal cancellation refund");
    var forfeited = cancelled.GetProperty("ledger")
      .EnumerateArray()
      .Single(item => item.GetProperty("kind").GetString() == nameof(LedgerKind.BonusForfeited));
    await Assert.That(cancelled.GetProperty("goals").GetArrayLength()).IsEqualTo(0);
    await Assert.That(cancelled.GetProperty("balances").GetProperty(_childId.Value).GetDecimal())
      .IsEqualTo(100);
    await Assert.That(refund.GetProperty("memberId").GetString()).IsEqualTo(_childId.Value);
    await Assert.That(refund.GetProperty("amount").GetDecimal()).IsEqualTo(30);
    await Assert.That(forfeited.GetProperty("amount").GetDecimal()).IsEqualTo(10);
  }

  [Test]
  public async Task LedgerAndAppliedOperationsSurviveReloadAndRejectAReplayedCommand()
  {
    var repository = CreateRepository(SavingsState(balance: 100), new { rewards = Array.Empty<object>() });
    var handler = CreateHandler(repository);
    var addGoalCommandId = Guid.Parse("d2993c54-2f08-46dc-97d4-2f37ce32d99c");
    var depositCommandId = Guid.Parse("a3285dcb-967b-40ca-a798-79182e8854a2");

    await ExecuteAsync(handler, _childId, "savings.add-goal", new
    {
      id = "durable-goal",
      ownerId = _childId.Value,
      title = "Durable goal",
      icon = "goal-icon",
      target = 100,
      visibility = "private",
    }, commandId: addGoalCommandId);
    await ExecuteAsync(handler, _childId, "savings.deposit", new
    {
      id = "durable-goal",
      amount = 10,
    }, commandId: depositCommandId);

    var afterDeposit = repository.Required(_familyId, "savings");
    var entryIds = afterDeposit.State.GetProperty("ledger")
      .EnumerateArray()
      .Select(item => item.GetProperty("entryId").GetString()
        ?? throw new InvalidOperationException("A ledger-entry ID is required."))
      .ToArray();
    var operationIds = afterDeposit.State.GetProperty("appliedOperationIds")
      .EnumerateArray()
      .Select(item => item.GetString() ?? throw new InvalidOperationException("An operation ID is required."))
      .ToArray();
    await Assert.That(afterDeposit.SchemaVersion)
      .IsEqualTo(FamilyAggregateSchema.GetVersion("savings"));
    await Assert.That(entryIds.Length).IsEqualTo(3);
    await Assert.That(entryIds.Distinct(StringComparer.Ordinal).Count()).IsEqualTo(3);
    await Assert.That(operationIds).IsEquivalentTo(
      [addGoalCommandId.ToString("N"), depositCommandId.ToString("N")]
    );

    await ExecuteAsync(handler, _childId, "savings.toggle-cheer", new
    {
      id = "durable-goal",
    });

    var afterSecondReload = repository.Required(_familyId, "savings");
    var reloadedEntryIds = afterSecondReload.State.GetProperty("ledger")
      .EnumerateArray()
      .Select(item => item.GetProperty("entryId").GetString()
        ?? throw new InvalidOperationException("A ledger-entry ID is required."))
      .ToArray();
    await Assert.That(reloadedEntryIds).IsEquivalentTo(entryIds);
    await Assert.That(
      afterSecondReload.State.GetProperty("appliedOperationIds")
        .EnumerateArray()
        .Select(item => item.GetString()
          ?? throw new InvalidOperationException("An operation ID is required."))
        .ToArray()
    ).IsEquivalentTo(operationIds);

    await Assert.That(async () =>
      await ExecuteAsync(handler, _childId, "savings.deposit", new
      {
        id = "durable-goal",
        amount = 10,
      }, commandId: depositCommandId)
    ).Throws<DomainRuleException>();

    var afterReplay = repository.Required(_familyId, "savings");
    await Assert.That(afterReplay.Revision).IsEqualTo(afterSecondReload.Revision);
    await Assert.That(afterReplay.State.GetProperty("ledger").GetArrayLength()).IsEqualTo(3);
  }

  [Test]
  public async Task SupportedLedgerSourceIdentifiersRoundTripWithoutChangingTheirTypes()
  {
    var sources = new[]
    {
      new { sourceType = "contribution", sourceId = "daily-basic" },
      new { sourceType = "family-member", sourceId = _guardianId.Value },
      new { sourceType = "house-content", sourceId = "cozy-sofa" },
      new { sourceType = "redemption-request", sourceId = "request-1" },
      new { sourceType = "saving-goal", sourceId = "stored-goal" },
    };
    var ledger = sources.Select((source, index) => new
    {
      entryId = Guid.Parse($"10000000-0000-0000-0000-{index + 1:D12}"),
      operationId = $"operation-{index + 1}",
      memberId = _childId.Value,
      goalId = (string?)null,
      kind = "Credit",
      amount = index + 1,
      source.sourceType,
      source.sourceId,
      reason = "Stored history",
      occurredAtUtc = "2026-10-12T10:00:00.0000000+00:00",
      initiatorId = (string?)null,
    }).ToArray();
    var savings = new
    {
      balances = new { guardian = 0, child = 20 },
      familyCurrencyCode = "CHF",
      ladirchenPerCurrencyUnit = 10,
      goals = new object[]
      {
        new
        {
          id = "stored-goal",
          title = "Stored goal",
          icon = "goal-icon",
          ownerId = _childId.Value,
          target = 100,
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
      },
      ledger,
      appliedOperationIds = ledger.Select(item => item.operationId).ToArray(),
      pendingGuardianGifts = Array.Empty<object>(),
      settledInterestPeriods = Array.Empty<string>(),
      vouchers = Array.Empty<object>(),
    };
    var repository = CreateRepository(savings, new { rewards = Array.Empty<object>() });
    var handler = CreateHandler(repository);

    await ExecuteAsync(handler, _childId, "savings.toggle-cheer", new
    {
      id = "stored-goal",
    });

    var storedSources = repository.Required(_familyId, "savings")
      .State
      .GetProperty("ledger")
      .EnumerateArray()
      .ToDictionary(
        item => item.GetProperty("sourceType").GetString()
          ?? throw new InvalidOperationException("A ledger source type is required."),
        item => item.GetProperty("sourceId").GetString(),
        StringComparer.Ordinal
      );
    await Assert.That(storedSources.Count).IsEqualTo(sources.Length);

    foreach (var source in sources)
    {
      await Assert.That(storedSources[source.sourceType]).IsEqualTo(source.sourceId);
    }
  }

  private static ExecuteFamilyCommandHandler CreateHandler(TestFamilyAggregateSnapshotRepository repository)
  {
    return new ExecuteFamilyCommandHandler(
      repository,
      new ImmediateFamilyCommandExecutionCoordinator(),
      new FixedTimeProvider(new DateTimeOffset(2026, 10, 12, 10, 0, 0, TimeSpan.Zero))
    );
  }

  private static TestFamilyAggregateSnapshotRepository CreateRepository(object savings, object rewardShop)
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
    repository.Seed(Snapshot("savings", 2, savings));
    repository.Seed(Snapshot("reward-shop", 1, rewardShop));

    return repository;
  }

  private static object SavingsState(decimal balance, object[]? goals = null)
  {
    return new
    {
      balances = new
      {
        guardian = 0,
        child = balance,
      },
      familyCurrencyCode = "CHF",
      ladirchenPerCurrencyUnit = 10,
      goals = goals ?? Array.Empty<object>(),
      pendingGuardianGifts = Array.Empty<object>(),
      settledInterestPeriods = Array.Empty<string>(),
      vouchers = Array.Empty<object>(),
    };
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

  private static async Task ExecuteAsync(
    ExecuteFamilyCommandHandler handler,
    FamilyMemberId actorId,
    string commandType,
    object payload,
    FamilyMemberId? authenticatedMemberId = null,
    Guid? commandId = null
  )
  {
    await handler.HandleAsync(
      new FamilyCommand(
        commandId ?? Guid.NewGuid(),
        _familyId,
        actorId,
        commandType,
        JsonSerializer.SerializeToElement(payload),
        authenticatedMemberId
      ),
      CancellationToken.None
    );
  }

  private static string? VoucherStatus(TestFamilyAggregateSnapshotRepository repository)
  {
    return repository.Required(_familyId, "savings")
      .State
      .GetProperty("vouchers")
      .EnumerateArray()
      .Single()
      .GetProperty("status")
      .GetString();
  }
}
