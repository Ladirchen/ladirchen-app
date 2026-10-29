using System;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Queries;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Tests.Application.Commands;

namespace LadirchenApp.Webhost.Tests.Application.Queries;

public sealed class SimulateSavingsInterestQueryHandlerTests
{
  [Test]
  public async Task UsesTypedGoalsAndConfiguredMaximumRate()
  {
    var now = new DateTimeOffset(2026, 10, 12, 10, 0, 0, TimeSpan.Zero);
    var familyId = new FamilyId(Guid.Parse("9f210fed-fbc0-40a7-9764-ceaefc304485"));
    var guardianId = new FamilyMemberId("guardian");
    var childId = new FamilyMemberId("child");
    var family = new Family(
      familyId,
      guardianId,
      "Guardian",
      new IanaTimeZone("Europe/Zurich")
    );
    family.AddMember(
      guardianId,
      childId,
      "Child",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      now
    );
    var economy = new FamilyEconomy(family);
    economy.GiveSpecialGift(
      guardianId,
      childId,
      new LadirchenAmount(45),
      "Test balance",
      new TransactionId("initial-balance"),
      now
    );
    var goalId = new SavingGoalId("bicycle");
    economy.AddGoal(
      childId,
      goalId,
      new SavingGoalDefinition(
        GoalOwner.Member(childId),
        "Bicycle",
        new LadirchenAmount(100),
        GoalVisibility.Family
      ),
      new TransactionId("add-bicycle-goal"),
      now
    );
    economy.DepositToGoal(
      childId,
      goalId,
      new LadirchenAmount(45),
      new TransactionId("deposit-bicycle-goal"),
      now
    );
    var progression = FamilyHouse.CreateInitial(family);
    var store = new TestTypedAggregateStore(new FixedTimeProvider(now));
    store.Seed(family, economy, progression: progression);
    var handler = new SimulateSavingsInterestQueryHandler(store, store, store);

    var result = await handler.HandleAsync(
      new SimulateSavingsInterestQuery(familyId, childId, 7, 100, 5),
      CancellationToken.None
    );

    await Assert.That(result.LadiStageId).IsEqualTo("super-ladi");
    await Assert.That(result.LadiStagePosition).IsEqualTo(6);
    await Assert.That(result.InterestRate).IsEqualTo(8);
    await Assert.That(result.ProjectedCredit).IsEqualTo(4);
  }
}
