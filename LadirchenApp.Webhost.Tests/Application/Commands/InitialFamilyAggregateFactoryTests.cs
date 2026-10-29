using System;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

public sealed class InitialFamilyAggregateFactoryTests
{
  [Test]
  public async Task CreateBuildsACompleteTypedAggregateBatch()
  {
    var familyId = new FamilyId(Guid.Parse("74586dc6-2053-40f7-bf25-339e8f8f29e4"));
    var guardianId = new FamilyMemberId("guardian");

    var batch = InitialFamilyAggregateFactory.Create(familyId, guardianId, "Guardian");

    await Assert.That(batch.FamilyId).IsEqualTo(familyId);
    await Assert.That(batch.UpdatedBy).IsEqualTo(guardianId);
    await Assert.That(batch.RevisionGuards).IsEmpty();
    await Assert.That(batch.Family?.ExpectedRevision).IsEqualTo(0);
    await Assert.That(batch.FamilyEconomy?.ExpectedRevision).IsEqualTo(0);
    await Assert.That(batch.FamilyProgression?.ExpectedRevision).IsEqualTo(0);
    await Assert.That(batch.HouseCustomization?.ExpectedRevision).IsEqualTo(0);

    var family = batch.Family?.Root ?? throw new InvalidOperationException();
    var guardian = family.Members.Single();
    await Assert.That(guardian.Id).IsEqualTo(guardianId);
    await Assert.That(guardian.Name).IsEqualTo("Guardian");
    await Assert.That(guardian.IsAdministrator).IsTrue();
    await Assert.That(family.TimeZone.Value).IsEqualTo("Europe/Zurich");
    await Assert.That(family.DomainEvents).IsEmpty();

    var economy = batch.FamilyEconomy?.Root ?? throw new InvalidOperationException();
    await Assert.That(economy.ContributionSettings).IsEqualTo(ContributionSettings.Default);
    await Assert.That(economy.Wallets.Select(item => item.Id)).IsEquivalentTo([guardianId]);
    await Assert.That(economy.Contributions).IsEmpty();
    await Assert.That(economy.SavingsGoals).IsEmpty();
    await Assert.That(economy.ShopRewards).IsEmpty();
    await Assert.That(economy.DomainEvents).IsEmpty();
    await Assert.That(batch.FamilyEconomy!.Presentation.Contributions).IsEmpty();
    await Assert.That(batch.FamilyEconomy.Presentation.SavingGoals).IsEmpty();
    await Assert.That(batch.FamilyEconomy.Presentation.ShopRewards).IsEmpty();
    await Assert.That(batch.FamilyEconomy.Presentation.GuardianGifts).IsEmpty();

    var progression = batch.FamilyProgression?.Root ?? throw new InvalidOperationException();
    await Assert.That(progression.StoredLevel).IsEqualTo(0);
    await Assert.That(progression.CompletedWeeklyStreak).IsEqualTo(0);
    await Assert.That(progression.CurrentWeekTarget).IsEqualTo(PersonalStreakPolicy.DefaultWeeklyTargetDays);
    await Assert.That(progression.ClosedWeeks).IsEmpty();
    await Assert.That(progression.DomainEvents).IsEmpty();

    var house = batch.HouseCustomization?.Root ?? throw new InvalidOperationException();
    await Assert.That(house.SubscriptionTier).IsEqualTo(HouseSubscriptionTier.Free);
    await Assert.That(house.SelectedThemeId).IsEqualTo("sunny-dollhouse");
    await Assert.That(house.DomainEvents).IsEmpty();
  }
}
