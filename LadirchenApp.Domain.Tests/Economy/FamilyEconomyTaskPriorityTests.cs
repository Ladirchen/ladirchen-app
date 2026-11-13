using System;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Tests.Economy;

public sealed class FamilyEconomyTaskPriorityTests
{
  private static readonly FamilyId _familyId = new(
    Guid.Parse("22227330-8a4d-4110-a068-a41a7d1ef314")
  );
  private static readonly FamilyMemberId _guardianId = new("guardian");
  private static readonly FamilyMemberId _childId = new("child");
  private static readonly DateTimeOffset _now = DateTimeOffset.Parse("2026-11-13T10:00:00Z");

  [Test]
  public async Task ExtraContributionRequiresASubmittedBasicContributionInTheCurrentWeek()
  {
    var economy = CreateEconomy();
    AddContribution(economy, "extra", ContributionKind.Extra, _childId);

    await Assert.That(() => economy.SubmitContribution(
      _childId,
      new ContributionId("extra"),
      _now
    )).Throws<DomainRuleException>();

    SubmitBasicContribution(economy);
    economy.SubmitContribution(_childId, new ContributionId("extra"), _now);

    await Assert.That(economy.Contributions.Single(item => item.Id.Value == "extra").Status)
      .IsEqualTo(ContributionStatus.Pending);
  }

  [Test]
  public async Task OpenBasicContributionPreventsAChildFromRequestingAShopReward()
  {
    var economy = CreateEconomy();
    AddContribution(economy, "basic", ContributionKind.Basic, _childId);
    economy.GiveSpecialGift(
      _guardianId,
      _childId,
      new LadirchenAmount(20),
      "Initial balance",
      new TransactionId("initial-balance"),
      _now
    );
    economy.AddShopReward(
      _guardianId,
      new ShopRewardId("reward"),
      new ShopRewardDefinition(
        "Reward",
        ShopRewardCategory.Gift,
        new LadirchenAmount(10),
        1,
        true,
        new AvailabilityWindow(null, null)
      ),
      _now
    );

    await Assert.That(() => economy.RequestShopReward(
      _childId,
      new ShopRewardId("reward"),
      new RedemptionRequestId("request"),
      new TransactionId("request"),
      _now
    )).Throws<DomainRuleException>();
    await Assert.That(economy.GetShopRewardAvailability(new ShopRewardId("reward"), _now).RequestableByMemberIds)
      .DoesNotContain(_childId);

    economy.SubmitContribution(_childId, new ContributionId("basic"), _now);
    economy.RequestShopReward(
      _childId,
      new ShopRewardId("reward"),
      new RedemptionRequestId("request"),
      new TransactionId("request"),
      _now
    );

    await Assert.That(economy.RedemptionRequests).HasSingleItem();
  }

  [Test]
  public async Task SubmittingAContributionCountsForTheMembersStreakOnTheSameDay()
  {
    var economy = CreateEconomy();
    AddContribution(economy, "today", ContributionKind.Basic, _childId);

    economy.SubmitContribution(_childId, new ContributionId("today"), _now);
    var currentWeek = IsoWeekPeriod.For(new IanaTimeZone("Europe/Zurich").LocalDate(_now));
    var participation = economy.EvaluateParticipation(_childId, currentWeek, 0, 7);

    await Assert.That(participation.StreakDays).IsEqualTo(1);
  }

  [Test]
  public async Task ReactivatingAnApprovedContributionCreatesAFreshAssignedInstance()
  {
    var economy = CreateEconomy();
    var originalId = new ContributionId("completed");
    var reactivatedId = new ContributionId("reactivated");
    AddContribution(economy, originalId.Value, ContributionKind.Basic, _childId);
    economy.SubmitContribution(_childId, originalId, _now);
    economy.ApproveContribution(
      _guardianId,
      originalId,
      new StarRating(4),
      new TransactionId("approve-completed"),
      _now
    );

    economy.ReactivateContribution(_guardianId, originalId, reactivatedId, _now);

    var original = economy.Contributions.Single(item => item.Id == originalId);
    var reactivated = economy.Contributions.Single(item => item.Id == reactivatedId);
    await Assert.That(original.Status).IsEqualTo(ContributionStatus.Approved);
    await Assert.That(reactivated.Status).IsEqualTo(ContributionStatus.Available);
    await Assert.That(reactivated.AssigneeId).IsEqualTo(_childId);
    await Assert.That(reactivated.Title).IsEqualTo(original.Title);
  }

  private static FamilyEconomy CreateEconomy()
  {
    var family = new FamilyAggregate(
      _familyId,
      _guardianId,
      "Guardian",
      new IanaTimeZone("Europe/Zurich")
    );
    family.AddMember(
      _guardianId,
      _childId,
      "Child",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      _now
    );

    return new FamilyEconomy(family);
  }

  private static void AddContribution(
    FamilyEconomy economy,
    string id,
    ContributionKind kind,
    FamilyMemberId assigneeId
  )
  {
    economy.AddContribution(
      _guardianId,
      new ContributionId(id),
      new ContributionDefinition(
        id,
        kind,
        new LadirchenAmount(10),
        new Percentage(20),
        assigneeId
      ),
      _now
    );
  }

  private static void SubmitBasicContribution(FamilyEconomy economy)
  {
    AddContribution(economy, "completed-basic", ContributionKind.Basic, _childId);
    economy.SubmitContribution(_childId, new ContributionId("completed-basic"), _now);
  }
}
