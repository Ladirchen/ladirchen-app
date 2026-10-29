using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Commands;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

public sealed class RewardShopCommandHandlerTests
{
  private static readonly FamilyId _familyId = new(Guid.Parse("a01156a8-dd90-4bf4-beb5-527ff0b29040"));
  private static readonly FamilyMemberId _administratorId = new("guardian");
  private static readonly FamilyMemberId _childId = new("child");

  [Test]
  public async Task CommandsUseDomainReservationsAndPreservePresentationFields()
  {
    var repository = CreateRepository();
    var handler = new ExecuteFamilyCommandHandler(
      repository,
      new ImmediateFamilyCommandExecutionCoordinator(),
      new FixedTimeProvider(new DateTimeOffset(2026, 10, 10, 15, 0, 0, TimeSpan.Zero))
    );
    var firstRewardId = "movie-night";
    var firstRequestCommandId = Guid.Parse("fb5bddc7-1efe-4daa-b73f-8cb3e57a7868");

    await ExecuteAsync(
      handler,
      _administratorId,
      "reward-shop.add",
      new
      {
        id = firstRewardId,
        title = "Movie night",
        description = "Choose a family movie",
        icon = "🎬",
        price = 80,
        category = "activity",
        quantity = 2,
        conditions = "Homework is complete",
        isVisible = true,
      }
    );
    await ExecuteAsync(
      handler,
      _childId,
      "reward-shop.request",
      new
      {
        id = firstRewardId
      },
      firstRequestCommandId
    );

    var requestedReward = Reward(repository, firstRewardId);
    await Assert.That(requestedReward.GetProperty("status").GetString()).IsEqualTo("requested");
    await Assert.That(requestedReward.GetProperty("requesterId").GetString()).IsEqualTo(_childId.Value);
    await Assert.That(requestedReward.GetProperty("requestId").GetString()).IsEqualTo(firstRequestCommandId.ToString("N"));
    await Assert.That(Balance(repository, _childId)).IsEqualTo(100);

    await ExecuteAsync(
      handler,
      _administratorId,
      "reward-shop.add",
      new
      {
        id = "small-gift",
        title = "Small gift",
        description = "Choose a small gift",
        icon = "🎁",
        price = 30,
        category = "gift",
        quantity = 1,
        conditions = "Available once",
        isVisible = true,
      }
    );
    var revisionBeforeRejectedRequest = repository.Required(_familyId, "reward-shop").Revision;

    await Assert.That(async () =>
      await ExecuteAsync(
        handler,
        _childId,
        "reward-shop.request",
        new
        {
          id = "small-gift"
        }
      )
    ).Throws<DomainRuleException>();
    await Assert.That(repository.Required(_familyId, "reward-shop").Revision)
      .IsEqualTo(revisionBeforeRejectedRequest);

    await ExecuteAsync(
      handler,
      _administratorId,
      "reward-shop.decide",
      new
      {
        id = firstRewardId,
        approved = true
      }
    );

    var approvedReward = Reward(repository, firstRewardId);
    await Assert.That(approvedReward.GetProperty("status").GetString()).IsEqualTo("available");
    await Assert.That(approvedReward.GetProperty("quantity").GetInt32()).IsEqualTo(1);
    await Assert.That(approvedReward.TryGetProperty("requesterId", out _)).IsFalse();
    await Assert.That(approvedReward.GetProperty("description").GetString())
      .IsEqualTo("Choose a family movie");
    await Assert.That(approvedReward.GetProperty("icon").GetString()).IsEqualTo("🎬");
    await Assert.That(approvedReward.GetProperty("conditions").GetString())
      .IsEqualTo("Homework is complete");
    await Assert.That(Balance(repository, _childId)).IsEqualTo(20);
  }

  [Test]
  public async Task VisibilityCancellationRejectionAndDeletionUseTheDomainModel()
  {
    var repository = CreateRepository();
    var handler = new ExecuteFamilyCommandHandler(
      repository,
      new ImmediateFamilyCommandExecutionCoordinator(),
      new FixedTimeProvider(new DateTimeOffset(2026, 10, 10, 15, 0, 0, TimeSpan.Zero))
    );
    var rewardId = "family-activity";

    await ExecuteAsync(
      handler,
      _administratorId,
      "reward-shop.add",
      new
      {
        id = rewardId,
        title = "Family activity",
        description = "Choose an activity",
        icon = "🎲",
        price = 40,
        category = "activity",
        quantity = 2,
        conditions = "Everyone agrees",
        isVisible = true,
      }
    );
    await ExecuteAsync(
      handler,
      _administratorId,
      "reward-shop.set-visibility",
      new
      {
        id = rewardId,
        isVisible = false
      }
    );
    await Assert.That(Reward(repository, rewardId).GetProperty("isVisible").GetBoolean())
      .IsFalse();
    await ExecuteAsync(
      handler,
      _administratorId,
      "reward-shop.set-visibility",
      new
      {
        id = rewardId,
        isVisible = true
      }
    );

    await ExecuteAsync(handler, _childId, "reward-shop.request", new
    {
      id = rewardId
    });
    await ExecuteAsync(handler, _childId, "reward-shop.cancel", new
    {
      id = rewardId
    });
    await Assert.That(Reward(repository, rewardId).GetProperty("status").GetString())
      .IsEqualTo("available");
    await Assert.That(Balance(repository, _childId)).IsEqualTo(100);

    await ExecuteAsync(handler, _childId, "reward-shop.request", new
    {
      id = rewardId
    });
    await ExecuteAsync(
      handler,
      _administratorId,
      "reward-shop.decide",
      new
      {
        id = rewardId,
        approved = false
      }
    );
    await Assert.That(Reward(repository, rewardId).GetProperty("status").GetString())
      .IsEqualTo("available");
    await Assert.That(Balance(repository, _childId)).IsEqualTo(100);

    await ExecuteAsync(handler, _childId, "reward-shop.request", new
    {
      id = rewardId
    });
    await ExecuteAsync(handler, _administratorId, "reward-shop.delete", new
    {
      id = rewardId
    });
    await Assert.That(
      repository.Required(_familyId, "reward-shop")
        .State
        .GetProperty("rewards")
        .EnumerateArray()
        .Any(item => item.GetProperty("id").GetString() == rewardId)
    ).IsFalse();
    await Assert.That(Balance(repository, _childId)).IsEqualTo(100);
  }

  private static TestFamilyAggregateSnapshotRepository CreateRepository()
  {
    var repository = new TestFamilyAggregateSnapshotRepository();
    repository.Seed(
      Snapshot(
        "family-profile",
        2,
        new
        {
          timeZone = "Europe/Zurich",
          pets = Array.Empty<object>(),
          members = new object[]
          {
            new
            {
              id = _administratorId.Value,
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
        }
      )
    );
    repository.Seed(Snapshot("reward-shop", 1, new
    {
      rewards = Array.Empty<object>()
    }));
    repository.Seed(
      Snapshot(
        "savings",
        2,
        new
        {
          balances = new
          {
            guardian = 0,
            child = 100,
          },
        }
      )
    );

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
      _administratorId,
      DateTimeOffset.UnixEpoch
    );
  }

  private static async Task ExecuteAsync(
    ExecuteFamilyCommandHandler handler,
    FamilyMemberId actorId,
    string commandType,
    object payload,
    Guid? commandId = null
  )
  {
    await handler.HandleAsync(
      new FamilyCommand(
        commandId ?? Guid.NewGuid(),
        _familyId,
        actorId,
        commandType,
        JsonSerializer.SerializeToElement(payload)
      ),
      CancellationToken.None
    );
  }

  private static JsonElement Reward(
    TestFamilyAggregateSnapshotRepository repository,
    string rewardId
  )
  {
    return repository.Required(_familyId, "reward-shop")
      .State
      .GetProperty("rewards")
      .EnumerateArray()
      .Single(item => item.GetProperty("id").GetString() == rewardId);
  }

  private static decimal Balance(
    TestFamilyAggregateSnapshotRepository repository,
    FamilyMemberId memberId
  )
  {
    return repository.Required(_familyId, "savings")
      .State
      .GetProperty("balances")
      .GetProperty(memberId.Value)
      .GetDecimal();
  }
}