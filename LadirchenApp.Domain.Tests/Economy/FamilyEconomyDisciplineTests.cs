using System;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Tests.Economy;

public sealed class FamilyEconomyDisciplineTests
{
  private static readonly FamilyId _familyId = new(
    Guid.Parse("725ca296-939f-4d43-91e5-f4397f9beef2")
  );
  private static readonly FamilyMemberId _guardianId = new("guardian");
  private static readonly FamilyMemberId _childId = new("child");
  private static readonly DateTimeOffset _now = DateTimeOffset.Parse("2026-11-03T10:00:00Z");

  [Test]
  public async Task WarningsProgressThroughTheDailyBehaviorLightAndTheGuardianMayResetIt()
  {
    var economy = CreateEconomy();
    economy.GiveSpecialGift(
      _guardianId,
      _childId,
      new LadirchenAmount(10),
      "Initial balance",
      new TransactionId("initial-balance"),
      _now
    );

    var yellow = IssueWarning(economy, 1, 0, _now);
    var orange = IssueWarning(economy, 2, 4, _now.AddMinutes(1));
    var red = IssueWarning(economy, 3, 0, _now.AddMinutes(2));

    await Assert.That(yellow.Level).IsEqualTo(BehaviorLightLevel.Yellow);
    await Assert.That(orange.Level).IsEqualTo(BehaviorLightLevel.Orange);
    await Assert.That(red.Level).IsEqualTo(BehaviorLightLevel.Red);
    await Assert.That(red.TasksBlocked).IsTrue();
    await Assert.That(economy.WalletFor(_childId).Available).IsEqualTo(6);
    await Assert.That(() => economy.ClaimContribution(
      _childId,
      new ContributionId("missing-task"),
      _now.AddMinutes(3)
    )).Throws<DomainRuleException>();

    var reset = economy.ResetBehaviorStatus(
      _guardianId,
      _childId,
      new TransactionId("reset-status"),
      _now.AddMinutes(4)
    );

    await Assert.That(reset.Level).IsEqualTo(BehaviorLightLevel.Green);
    await Assert.That(reset.TasksBlocked).IsFalse();
    await Assert.That(economy.WalletFor(_childId).Available).IsEqualTo(6);
  }

  [Test]
  public async Task BehaviorLightResetsAutomaticallyOnTheNextLocalDay()
  {
    var economy = CreateEconomy();
    IssueWarning(economy, 1, 0, _now);

    var nextDay = economy.BehaviorStatusFor(_childId, _now.AddDays(1));

    await Assert.That(nextDay.Level).IsEqualTo(BehaviorLightLevel.Green);
    await Assert.That(nextDay.WarningCount).IsEqualTo(0);
  }

  [Test]
  public async Task ChildCanAcknowledgeEachBehaviorNoticeExactlyOnce()
  {
    var economy = CreateEconomy();
    IssueWarning(economy, 1, 0, _now);
    var notice = economy.PendingBehaviorNoticesFor(_childId, _now)[0];

    economy.AcknowledgeBehaviorNotice(
      _childId,
      notice.Id,
      new TransactionId("acknowledge-warning"),
      _now.AddMinutes(1)
    );

    await Assert.That(economy.PendingBehaviorNoticesFor(_childId, _now)).IsEmpty();
  }

  [Test]
  public async Task ExternalGuardianCannotManageBehavior()
  {
    var family = CreateFamily();
    var externalGuardianId = new FamilyMemberId("external-guardian");
    family.AddMember(
      _guardianId,
      externalGuardianId,
      "External Guardian",
      MemberRole.Guardian,
      GuardianAccess.External,
      false,
      _now
    );
    var economy = new FamilyEconomy(family);

    await Assert.That(() => economy.IssueBehaviorWarning(
      externalGuardianId,
      _childId,
      new LadirchenAmount(0),
      "Not allowed",
      new BehaviorIncidentId("external-warning"),
      new TransactionId("external-warning"),
      _now
    )).Throws<DomainRuleException>();
  }

  private static ChildBehaviorStatus IssueWarning(
    FamilyEconomy economy,
    int number,
    decimal deduction,
    DateTimeOffset now
  )
  {
    return economy.IssueBehaviorWarning(
      _guardianId,
      _childId,
      new LadirchenAmount(deduction),
      $"Warning {number}",
      new BehaviorIncidentId($"warning-{number}"),
      new TransactionId($"warning-{number}"),
      now
    );
  }

  private static FamilyEconomy CreateEconomy()
  {
    return new FamilyEconomy(CreateFamily());
  }

  private static FamilyAggregate CreateFamily()
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

    return family;
  }
}
