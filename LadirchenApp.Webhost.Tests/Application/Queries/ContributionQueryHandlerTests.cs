using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Tests.Application.Commands;

namespace LadirchenApp.Webhost.Tests.Application.Queries;

public sealed class ContributionQueryHandlerTests
{
  private static readonly FamilyId _familyId = new(Guid.Parse("07155cf8-8894-4973-8956-c758a42ececd"));
  private static readonly FamilyMemberId _administratorId = new("administrator");
  private static readonly FamilyMemberId _assigneeId = new("assignee");
  private static readonly FamilyMemberId _supporterId = new("supporter");
  private static readonly FamilyMemberId _siblingId = new("sibling");

  [Test]
  public async Task QueryProjectsContributionRulesFromTypedFamilyEconomyState()
  {
    var repository = CreateRepository();
    var handler = new GetFamilyAggregateQueryHandler(
      repository,
      new FixedTimeProvider(new DateTimeOffset(2026, 10, 29, 10, 0, 0, TimeSpan.Zero))
    );

    var result = await handler.HandleAsync(
      new GetFamilyAggregateQuery(_familyId, "contributions"),
      CancellationToken.None
    );

    var state = result?.State ?? throw new InvalidOperationException("The contribution read model is missing.");
    var extra = state.GetProperty("contributions").EnumerateArray()
      .Single(item => item.GetProperty("id").GetString() == "extra-task");
    var approved = state.GetProperty("contributions").EnumerateArray()
      .Single(item => item.GetProperty("id").GetString() == "approved-task");
    var promotion = state.GetProperty("promotions").EnumerateArray().Single();
    await Assert.That(state.GetProperty("perfectRatingBonusPercent").GetDecimal())
      .IsEqualTo(25);
    await Assert.That(extra.GetProperty("projectedReward").GetDecimal())
      .IsEqualTo(25);
    await Assert.That(
      extra.GetProperty("partnerableMemberIds").EnumerateArray().Select(item => item.GetString()!)
    ).IsEquivalentTo([_administratorId.Value, _supporterId.Value, _siblingId.Value]);
    await Assert.That(approved.GetProperty("partnerableMemberIds").GetArrayLength())
      .IsEqualTo(0);
    await Assert.That(promotion.GetProperty("available").GetBoolean())
      .IsTrue();
    await Assert.That(
      state.GetProperty("todayEarnedByMember").GetProperty(_siblingId.Value).GetDecimal()
    ).IsEqualTo(22);
    await Assert.That(
      state.GetProperty("currentWeekEarnedByMember").GetProperty(_siblingId.Value).GetDecimal()
    ).IsEqualTo(30);
    await Assert.That(
      state.GetProperty("currentWeekApprovedContributionCountByMember").GetProperty(_siblingId.Value).GetInt32()
    ).IsEqualTo(2);
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
          id = _administratorId.Value,
          name = "Administrator",
          role = "guardian",
          guardianAccess = "admin",
          invitationPending = false,
          participatesInWeeklyGoal = false,
        },
        new
        {
          id = _assigneeId.Value,
          name = "Assignee",
          role = "child",
          invitationPending = false,
          participatesInWeeklyGoal = true,
        },
        new
        {
          id = _supporterId.Value,
          name = "Supporter",
          role = "guardian",
          guardianAccess = "supporter",
          invitationPending = false,
          participatesInWeeklyGoal = false,
        },
        new
        {
          id = _siblingId.Value,
          name = "Sibling",
          role = "child",
          invitationPending = false,
          participatesInWeeklyGoal = true,
        },
        new
        {
          id = "pending-guardian",
          name = "Pending guardian",
          role = "guardian",
          guardianAccess = "supporter",
          invitationPending = true,
          participatesInWeeklyGoal = false,
        },
      },
    }));
    repository.Seed(Snapshot("contributions", 1, new
    {
      perfectRatingBonusPercent = 25,
      contributions = new object[]
      {
        new
        {
          id = "extra-task",
          title = "Extra task",
          description = "Work together",
          icon = "🤝",
          area = "Home",
          kind = "extra",
          status = "available",
          reward = 10,
          projectedReward = 999,
          energy = 10,
          assigneeId = _assigneeId.Value,
          dueLabel = "Optional",
          invitedChildIds = Array.Empty<string>(),
        },
        new
        {
          id = "approved-task",
          title = "Approved task",
          description = "Already done",
          icon = "✅",
          area = "Home",
          kind = "basic",
          status = "approved",
          reward = 10,
          energy = 20,
          assigneeId = _siblingId.Value,
          dueLabel = "Today",
          invitedChildIds = Array.Empty<string>(),
          stars = 5,
          earnedReward = 20,
          earnedRatingBonus = 2,
          earnedPromotionMultiplier = 2,
          approvedAt = "2026-10-29T08:00:00+00:00",
        },
        new
        {
          id = "approved-task-at-local-week-boundary",
          title = "Approved at local week boundary",
          description = "Included through the family timezone",
          icon = "✅",
          area = "Home",
          kind = "basic",
          status = "approved",
          reward = 5,
          energy = 10,
          assigneeId = _siblingId.Value,
          dueLabel = "This week",
          invitedChildIds = Array.Empty<string>(),
          stars = 4,
          earnedReward = 5,
          earnedRatingBonus = 3,
          earnedPromotionMultiplier = 1,
          approvedAt = "2026-10-25T23:30:00+00:00",
        },
        new
        {
          id = "approved-task-in-previous-local-week",
          title = "Approved in previous local week",
          description = "Excluded through the family timezone",
          icon = "✅",
          area = "Home",
          kind = "basic",
          status = "approved",
          reward = 99,
          energy = 10,
          assigneeId = _siblingId.Value,
          dueLabel = "Previous week",
          invitedChildIds = Array.Empty<string>(),
          stars = 4,
          earnedReward = 99,
          earnedRatingBonus = 0,
          earnedPromotionMultiplier = 1,
          approvedAt = "2026-10-25T22:30:00+00:00",
        },
      },
      promotions = new object[]
      {
        new
        {
          id = "double-reward",
          contributionId = "extra-task",
          title = "Double reward",
          multiplier = 2,
          deadline = "17:00",
          teamworkBonus = 5,
          active = true,
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
      _administratorId,
      DateTimeOffset.UnixEpoch
    );
  }
}
