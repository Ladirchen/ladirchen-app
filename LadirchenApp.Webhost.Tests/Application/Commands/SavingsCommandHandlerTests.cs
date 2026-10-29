using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Commands;
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
    object payload
  )
  {
    await handler.HandleAsync(
      new FamilyCommand(
        Guid.NewGuid(),
        _familyId,
        actorId,
        commandType,
        JsonSerializer.SerializeToElement(payload)
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
