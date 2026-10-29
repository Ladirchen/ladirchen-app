using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Queries;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

public sealed class WeeklyInterestSettlementPlanQueryHandlerTests
{
  private static readonly FamilyId _familyId = new(Guid.Parse("25955ff8-5e55-453e-98ec-b614d4ace938"));
  private static readonly FamilyMemberId _administratorId = new("guardian");

  [Test]
  public async Task PlanUsesTypedClosedWeeksAndExcludesSettledPeriods()
  {
    var now = new DateTimeOffset(2026, 10, 12, 10, 0, 0, TimeSpan.Zero);
    var family = new Family(
      _familyId,
      _administratorId,
      "Guardian",
      new IanaTimeZone("Europe/Zurich")
    );
    var economy = new FamilyEconomy(family);
    var progression = FamilyHouse.CreateInitial(family);
    var settledPeriod = new IsoWeekPeriod(2026, 40);
    var openPeriod = new IsoWeekPeriod(2026, 41);
    progression.CloseSuccessfulWeek(
      settledPeriod,
      HouseEnergyPolicy.MinimumSuccessfulWeekEnergy,
      Array.Empty<PersonalLadiEvaluation>(),
      now.AddDays(-7)
    );
    progression.CloseFailedWeek(
      openPeriod,
      0,
      Array.Empty<PersonalLadiEvaluation>(),
      now
    );
    economy.ApplyWeeklyInterest(
      _administratorId,
      settledPeriod,
      new Dictionary<FamilyMemberId, PersonalLadiEvaluation>(),
      SavingsInterestPolicy.Default,
      new TransactionId("settle-2026-W40"),
      now
    );
    var store = new TestTypedAggregateStore(new FixedTimeProvider(now));
    store.Seed(family, economy, progression: progression);
    var handler = new GetWeeklyInterestSettlementPlanQueryHandler(store, store, store);

    var plan = await handler.HandleAsync(_familyId, CancellationToken.None);

    await Assert.That(plan).IsNotNull();
    await Assert.That(plan!.AdministratorId).IsEqualTo(_administratorId);
    await Assert.That(plan.Periods.Select(period => period.Key)).IsEquivalentTo(["2026-W41"]);
  }
}
