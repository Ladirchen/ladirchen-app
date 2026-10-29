using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

public sealed class RewardShopQueryHandlerTests
{
  private static readonly FamilyId _familyId = new(Guid.Parse("f1cd8ee7-52bd-488a-a9c0-9172da753f87"));
  private static readonly FamilyMemberId _administratorId = new("guardian");

  [Test]
  public async Task QueryProjectsCapabilitiesFromTypedFamilyEconomyState()
  {
    var repository = CreateRepository();
    var handler = new GetFamilyAggregateQueryHandler(
      repository,
      new FixedTimeProvider(new DateTimeOffset(2026, 10, 10, 10, 0, 0, TimeSpan.Zero))
    );

    var result = await handler.HandleAsync(
      new GetFamilyAggregateQuery(_familyId, "reward-shop", _administratorId),
      CancellationToken.None
    );
    var state = result?.State ?? throw new InvalidOperationException("The reward shop result is required.");
    var affordableReward = Reward(state, "affordable");
    var requestedReward = Reward(state, "requested");
    var scheduledReward = Reward(state, "scheduled");

    await Assert.That(state.GetProperty("redemptionOpen").GetBoolean()).IsTrue();
    await Assert.That(affordableReward.GetProperty("published").GetBoolean()).IsTrue();
    await Assert.That(affordableReward.GetProperty("publicationStatus").GetString())
      .IsEqualTo("visible");
    await Assert.That(MemberIds(affordableReward, "requestableByMemberIds"))
      .IsEmpty();
    await Assert.That(MemberIds(requestedReward, "cancellableByMemberIds")).IsEmpty();
    await Assert.That(MemberIds(requestedReward, "approvableByMemberIds"))
      .IsEquivalentTo([_administratorId.Value]);
    await Assert.That(scheduledReward.GetProperty("published").GetBoolean()).IsFalse();
    await Assert.That(scheduledReward.GetProperty("publicationStatus").GetString())
      .IsEqualTo("scheduled");
    await Assert.That(scheduledReward.GetProperty("nextStatusChangeAt").GetString())
      .IsEqualTo("2026-10-10T22:00:00.0000000+00:00");
  }

  [Test]
  public async Task ClosedShopRemovesRequestAndApprovalCapabilitiesButKeepsCancellation()
  {
    var repository = CreateRepository();
    var handler = new GetFamilyAggregateQueryHandler(
      repository,
      new FixedTimeProvider(new DateTimeOffset(2026, 10, 10, 16, 0, 0, TimeSpan.Zero))
    );

    var result = await handler.HandleAsync(
      new GetFamilyAggregateQuery(_familyId, "reward-shop", _administratorId),
      CancellationToken.None
    );
    var state = result?.State ?? throw new InvalidOperationException("The reward shop result is required.");
    var affordableReward = Reward(state, "affordable");
    var requestedReward = Reward(state, "requested");

    await Assert.That(state.GetProperty("redemptionOpen").GetBoolean()).IsFalse();
    await Assert.That(MemberIds(affordableReward, "requestableByMemberIds")).IsEmpty();
    await Assert.That(MemberIds(requestedReward, "approvableByMemberIds")).IsEmpty();
    await Assert.That(MemberIds(requestedReward, "cancellableByMemberIds")).IsEmpty();
  }

  [Test]
  public async Task OrdinaryMemberCannotRetrieveUnpublishedRewards()
  {
    var handler = new GetFamilyAggregateQueryHandler(
      CreateRepository(),
      new FixedTimeProvider(new DateTimeOffset(2026, 10, 10, 10, 0, 0, TimeSpan.Zero))
    );
    var childId = new FamilyMemberId("child");
    var result = await handler.HandleAsync(
      new GetFamilyAggregateQuery(_familyId, "reward-shop", childId),
      CancellationToken.None
    );
    var state = result?.State ?? throw new InvalidOperationException("The reward shop result is required.");

    await Assert.That(state.GetProperty("rewards").EnumerateArray()
      .Any(item => item.GetProperty("id").GetString() == "scheduled"))
      .IsFalse();
    await Assert.That(MemberIds(Reward(state, "affordable"), "requestableByMemberIds")).IsEmpty();
    await Assert.That(MemberIds(Reward(state, "requested"), "cancellableByMemberIds"))
      .IsEquivalentTo([childId.Value]);
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
              id = "supporter",
              name = "Supporter",
              role = "guardian",
              guardianAccess = "supporter",
              participatesInWeeklyGoal = false,
            },
            new
            {
              id = "child",
              name = "Child",
              role = "child",
              participatesInWeeklyGoal = true,
            },
            new
            {
              id = "sibling",
              name = "Sibling",
              role = "child",
              participatesInWeeklyGoal = true,
            },
            new
            {
              id = "invitedGuardian",
              name = "Invited guardian",
              role = "guardian",
              guardianAccess = "admin",
              invitationPending = true,
              participatesInWeeklyGoal = false,
            },
          },
        }
      )
    );
    repository.Seed(
      Snapshot(
        "reward-shop",
        1,
        new
        {
          rewards = new object[]
          {
            new
            {
              id = "affordable",
              title = "Affordable reward",
              category = "activity",
              price = 30,
              quantity = 2,
              isVisible = true,
              status = "available",
            },
            new
            {
              id = "requested",
              title = "Requested reward",
              category = "gift",
              price = 80,
              quantity = 1,
              isVisible = true,
              status = "requested",
              requestId = "pending-request",
              requesterId = "child",
              requestedAt = "2026-10-10T08:00:00+00:00",
            },
            new
            {
              id = "scheduled",
              title = "Scheduled reward",
              category = "time",
              price = 10,
              quantity = 1,
              isVisible = true,
              status = "available",
              availableFrom = "2026-10-11",
            },
          },
        }
      )
    );
    repository.Seed(
      Snapshot(
        "savings",
        1,
        new
        {
          balances = new
          {
            guardian = 0,
            supporter = 0,
            child = 100,
            sibling = 50,
            invitedGuardian = 1000,
          },
        }
      )
    );

    return repository;
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

  private static JsonElement Reward(JsonElement state, string rewardId)
  {
    return state.GetProperty("rewards")
      .EnumerateArray()
      .Single(item => item.GetProperty("id").GetString() == rewardId);
  }

  private static string[] MemberIds(JsonElement reward, string property)
  {
    return reward.GetProperty(property)
      .EnumerateArray()
      .Select(item => item.GetString() ?? throw new InvalidOperationException())
      .ToArray();
  }
}
