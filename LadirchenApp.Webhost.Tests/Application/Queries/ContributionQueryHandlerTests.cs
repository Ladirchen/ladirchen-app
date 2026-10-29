using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Tests.Application.Commands;

namespace LadirchenApp.Webhost.Tests.Application.Queries;

public sealed class ContributionQueryHandlerTests
{
  private static readonly FamilyId _familyId = new(
    Guid.Parse("07155cf8-8894-4973-8956-c758a42ececd")
  );
  private static readonly FamilyMemberId _administratorId = new("administrator");
  private static readonly FamilyMemberId _assigneeId = new("assignee");
  private static readonly FamilyMemberId _supporterId = new("supporter");
  private static readonly FamilyMemberId _siblingId = new("sibling");

  [Test]
  public async Task QueryProjectsContributionRulesFromTypedFamilyEconomyState()
  {
    var store = CreateStore();
    var handler = new GetFamilyAggregateQueryHandler(
      store,
      store,
      store,
      store,
      new FixedTimeProvider(new DateTimeOffset(2026, 10, 29, 10, 0, 0, TimeSpan.Zero))
    );

    var result = await handler.HandleAsync(
      new GetFamilyAggregateQuery(_familyId, "contributions", _administratorId),
      CancellationToken.None
    ) as ContributionsFamilyReadModelProjection
      ?? throw new InvalidOperationException("The contribution read model is missing.");
    var extra = result.Economy.Contributions.Single(item => item.Id.Value == "extra-task");
    var approved = result.Economy.Contributions.Single(item => item.Id.Value == "approved-task");
    var promotion = result.Economy.Promotions.Single();

    await Assert.That(result.CalculatedState.PerfectRatingBonusPercent).IsEqualTo(25);
    await Assert.That(result.CalculatedState.Contributions[extra.Id].ProjectedReward).IsEqualTo(25);
    await Assert.That(result.CalculatedState.Contributions[extra.Id].PartnerableMemberIds)
      .IsEquivalentTo([_administratorId, _supporterId, _siblingId]);
    await Assert.That(result.CalculatedState.Contributions[approved.Id].PartnerableMemberIds)
      .IsEmpty();
    await Assert.That(result.CalculatedState.PromotionAvailability[promotion.Id]).IsTrue();
    await Assert.That(result.CalculatedState.TodayEarnedByMember[_siblingId]).IsEqualTo(22);
    await Assert.That(result.CalculatedState.CurrentWeekEarnedByMember[_siblingId]).IsEqualTo(30);
    await Assert.That(
      result.CalculatedState.CurrentWeekApprovedContributionCountByMember[_siblingId]
    ).IsEqualTo(2);
  }

  private static TestTypedAggregateStore CreateStore()
  {
    var family = Family.Reconstitute(new FamilyState(
      _familyId,
      new IanaTimeZone("Europe/Zurich"),
      FamilyCurrency.CHF,
      new ExchangeRate(1),
      _assigneeId,
      false,
      [
        Member(_administratorId, "Administrator", MemberRole.Guardian, GuardianAccess.Admin),
        Member(_assigneeId, "Assignee", MemberRole.Child, GuardianAccess.Supporter),
        Member(_supporterId, "Supporter", MemberRole.Guardian, GuardianAccess.Supporter),
        Member(_siblingId, "Sibling", MemberRole.Child, GuardianAccess.Supporter),
        new FamilyMemberState(
          new FamilyMemberId("pending-guardian"),
          "Pending guardian",
          MemberRole.Guardian,
          GuardianAccess.Supporter,
          true,
          null,
          null,
          false
        ),
      ],
      []
    ));
    var contributions = new[]
    {
      Contribution(
        "extra-task",
        "Extra task",
        ContributionKind.Extra,
        10,
        10,
        _assigneeId,
        ContributionStatus.Available
      ),
      Contribution(
        "approved-task",
        "Approved task",
        ContributionKind.Basic,
        10,
        20,
        _siblingId,
        ContributionStatus.Approved,
        5,
        new ContributionRewardBreakdown(20, 2, 2),
        new DateTimeOffset(2026, 10, 29, 8, 0, 0, TimeSpan.Zero)
      ),
      Contribution(
        "approved-task-at-local-week-boundary",
        "Approved at local week boundary",
        ContributionKind.Basic,
        5,
        10,
        _siblingId,
        ContributionStatus.Approved,
        4,
        new ContributionRewardBreakdown(5, 3, 1),
        new DateTimeOffset(2026, 10, 25, 23, 30, 0, TimeSpan.Zero)
      ),
      Contribution(
        "approved-task-in-previous-local-week",
        "Approved in previous local week",
        ContributionKind.Basic,
        99,
        10,
        _siblingId,
        ContributionStatus.Approved,
        4,
        new ContributionRewardBreakdown(99, 0, 1),
        new DateTimeOffset(2026, 10, 25, 22, 30, 0, TimeSpan.Zero)
      ),
    };
    var promotionId = new PromotionId("double-reward");
    var economy = FamilyEconomy.Reconstitute(family, new FamilyEconomyState(
      _familyId,
      new ContributionSettings(new Percentage(25)),
      ActiveWallets(family),
      contributions,
      contributions.Select(item => item.Id).Append(new ContributionId("retired-task")).ToArray(),
      [
        new PromotionState(
          promotionId,
          new ContributionId("extra-task"),
          new RewardMultiplier(2),
          new LadirchenAmount(5),
          new PromotionDeadline(new TimeOnly(17, 0)),
          true
        ),
      ],
      [],
      [],
      [],
      [],
      [],
      [],
      [],
      []
    ));
    var presentation = new FamilyEconomyPresentationState(
      contributions.Select(item => new ContributionPresentationState(
        item.Id,
        item.Id.Value,
        "✅",
        false
      )),
      [],
      [],
      []
    );
    var store = new TestTypedAggregateStore(TimeProvider.System);
    store.Seed(family, economy, presentation);

    return store;
  }

  private static FamilyMemberState Member(
    FamilyMemberId id,
    string name,
    MemberRole role,
    GuardianAccess access
  )
  {
    return new FamilyMemberState(
      id,
      name,
      role,
      access,
      false,
      null,
      null,
      role == MemberRole.Child
    );
  }

  private static ContributionState Contribution(
    string id,
    string title,
    ContributionKind kind,
    decimal reward,
    decimal energy,
    FamilyMemberId assigneeId,
    ContributionStatus status,
    int? stars = null,
    ContributionRewardBreakdown? earnedReward = null,
    DateTimeOffset? approvedAt = null
  )
  {
    return new ContributionState(
      new ContributionId(id),
      title,
      kind,
      new LadirchenAmount(reward),
      new Percentage(energy),
      assigneeId,
      status,
      stars is null ? null : new StarRating(stars.Value),
      earnedReward,
      approvedAt,
      []
    );
  }

  private static MemberWalletState[] ActiveWallets(Family family)
  {
    return family.Members
      .Where(member => !member.InvitationPending)
      .Select(member => new MemberWalletState(member.Id, 0, 0))
      .ToArray();
  }
}
