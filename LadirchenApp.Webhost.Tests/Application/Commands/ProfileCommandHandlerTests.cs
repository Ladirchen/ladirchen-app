using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Commands;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

public sealed class ProfileCommandHandlerTests
{
  private static readonly FamilyId _familyId = new(Guid.Parse("5f418b52-314f-4165-92a0-dbf2e282a7bc"));
  private static readonly FamilyMemberId _guardianId = new("guardian");
  private static readonly FamilyMemberId _childId = new("child");

  [Test]
  public async Task CompleteSetupKeepsServerStreaksAndPersistsOnlyTypedMemberData()
  {
    var store = CreateStore();
    var handler = store.CreateCommandHandler();

    var result = await handler.HandleAsync(
      new FamilyCommand(
        Guid.NewGuid(),
        _familyId,
        _guardianId,
        "profile.complete-setup",
        JsonSerializer.SerializeToElement(new
        {
          members = new object[]
          {
            new
            {
              id = _guardianId.Value,
              name = "Guardian",
              avatar = "guardian-avatar",
              color = "#123456",
              role = "guardian",
              guardianAccess = "admin",
              participatesInWeeklyGoal = false,
              weeklyStreak = 999,
              unknownMemberField = "must-not-be-stored",
              appearance = new
              {
                age = "adult",
                skinToneId = "skin-medium",
                faceShape = "soft",
                face = "happy",
                hair = "short",
                hairColorId = "hair-brown",
                outfit = "shirt",
                outfitColorId = "outfit-emerald",
                accessoryId = "none",
                funAccessoryId = "none",
                seasonalAccessoryId = "none",
                unknownAppearanceField = "must-not-be-stored",
              },
            },
            new
            {
              id = _childId.Value,
              name = "Child",
              avatar = "child-avatar",
              color = "#abcdef",
              role = "child",
              participatesInWeeklyGoal = true,
              weeklyStreak = 888,
              unknownMemberField = "must-not-be-stored",
              appearance = new
              {
                age = "child",
                skinToneId = "skin-light",
                faceShape = "round",
                face = "freckles",
                hair = "waves",
                hairColorId = "hair-auburn",
                outfit = "hoodie",
                outfitColorId = "outfit-blue",
                accessoryId = "none",
                funAccessoryId = "none",
                seasonalAccessoryId = "none",
                unknownAppearanceField = "must-not-be-stored",
              },
            },
          },
          pets = new object[]
          {
            new
            {
              id = "pet",
              name = "Milo",
              kind = "cat",
              avatar = "pet-avatar",
              color = "#fedcba",
              unknownPetField = "must-not-be-stored",
            },
          },
        })
      ),
      CancellationToken.None
    );

    var family = store.RequiredFamily();
    var progression = store.RequiredProgression();
    var economy = store.RequiredEconomy();
    var house = store.RequiredHouse();
    var members = family.Members.ToDictionary(item => item.Id);
    var guardian = members[_guardianId];
    var child = members[_childId];
    var pet = family.Pets.Single();
    await Assert.That(progression.StoredWeeklyStreakFor(_guardianId)).IsEqualTo(6);
    await Assert.That(progression.StoredWeeklyStreakFor(_childId)).IsEqualTo(0);
    await Assert.That(economy.WalletFor(_childId).Balance).IsEqualTo(0);
    await Assert.That(house.Placements.Any(item => item.EntityType == "member" && item.EntityId == _childId.Value)).IsTrue();
    await Assert.That(house.Placements.Any(item => item.EntityType == "pet" && item.EntityId == "pet")).IsTrue();
    await Assert.That(guardian.Avatar).IsEqualTo("guardian-avatar");
    await Assert.That(child.Color).IsEqualTo("#abcdef");
    await Assert.That(guardian.Appearance?.Hair).IsEqualTo("short");
    await Assert.That(child.Appearance?.Outfit).IsEqualTo("hoodie");
    await Assert.That(pet.Avatar).IsEqualTo("pet-avatar");
    await Assert.That(pet.Color).IsEqualTo("#fedcba");
    await Assert.That(family.OnboardingCompleted).IsTrue();
    var changedAggregateTypes = result.ChangedAggregates
      .Select(item => item.AggregateType)
      .ToArray();
    await Assert.That(changedAggregateTypes).Contains("family-profile");
    await Assert.That(changedAggregateTypes).Contains("contributions");
    await Assert.That(changedAggregateTypes).Contains("family-progression");
    await Assert.That(changedAggregateTypes).Contains("savings");
    await Assert.That(changedAggregateTypes).Contains("reward-shop");
    await Assert.That(changedAggregateTypes).Contains("home-customization");
    await Assert.That(store.Revision(TypedAggregateKind.Family)).IsEqualTo(2);
    await Assert.That(store.Revision(TypedAggregateKind.FamilyEconomy)).IsEqualTo(2);
    await Assert.That(store.Revision(TypedAggregateKind.FamilyProgression)).IsEqualTo(2);
    await Assert.That(store.Revision(TypedAggregateKind.HouseCustomization)).IsEqualTo(2);
  }

  [Test]
  public async Task CompleteSetupCannotRemoveAMemberWithWalletFundsAndDoesNotPartiallySave()
  {
    var timeProvider = new FixedTimeProvider(
      new DateTimeOffset(2026, 10, 12, 10, 0, 0, TimeSpan.Zero)
    );
    var siblingId = new FamilyMemberId("sibling");
    var family = new Family(
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
      timeProvider.GetUtcNow()
    );
    family.AddMember(
      _guardianId,
      siblingId,
      "Sibling",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      timeProvider.GetUtcNow()
    );
    var economy = new FamilyEconomy(family);
    economy.GiveSpecialGift(
      _guardianId,
      _childId,
      new LadirchenAmount(10),
      "Existing funds",
      new TransactionId("existing-funds"),
      timeProvider.GetUtcNow()
    );
    var progression = FamilyHouse.CreateInitial(family);
    var house = FamilyHouseShop.CreateInitial(
      family,
      progression.StoredLevel,
      HouseSubscriptionTier.Pro
    );
    var store = new TestTypedAggregateStore(timeProvider);
    store.Seed(
      family,
      economy,
      progression: progression,
      house: house,
      updatedBy: _guardianId
    );
    var handler = store.CreateCommandHandler();

    await Assert.That(async () =>
      await handler.HandleAsync(
        CompleteSetupCommand(_guardianId, siblingId),
        CancellationToken.None
      )
    ).Throws<DomainRuleException>();

    await Assert.That(store.RequiredFamily().Members.Select(item => item.Id)).Contains(_childId);
    await Assert.That(store.RequiredEconomy().WalletFor(_childId).Balance).IsEqualTo(10);
    await Assert.That(store.Revision(TypedAggregateKind.Family)).IsEqualTo(1);
    await Assert.That(store.Revision(TypedAggregateKind.FamilyEconomy)).IsEqualTo(1);
    await Assert.That(store.Revision(TypedAggregateKind.FamilyProgression)).IsEqualTo(1);
    await Assert.That(store.Revision(TypedAggregateKind.HouseCustomization)).IsEqualTo(1);
  }

  [Test]
  public async Task WeeklyParticipationInvalidatesEveryStageDependentReadModel()
  {
    var supporterId = new FamilyMemberId("supporter");
    var store = CreateEstablishedStore(supporterId);
    var result = await store.CreateCommandHandler().HandleAsync(
      new FamilyCommand(
        Guid.NewGuid(),
        _familyId,
        _guardianId,
        "profile.set-weekly-participation",
        JsonSerializer.SerializeToElement(new
        {
          memberId = supporterId.Value,
          participates = true,
        })
      ),
      CancellationToken.None
    );
    var changedReadModels = result.ChangedAggregates
      .Select(item => item.AggregateType)
      .ToArray();

    await Assert.That(changedReadModels).IsEquivalentTo(
    [
      "family-profile",
      "family-progression",
      "savings",
      "home-customization",
    ]);
  }

  [Test]
  public async Task GuardianAccessInvalidatesPermissionDependentReadModels()
  {
    var supporterId = new FamilyMemberId("supporter");
    var store = CreateEstablishedStore(supporterId);
    var result = await store.CreateCommandHandler().HandleAsync(
      new FamilyCommand(
        Guid.NewGuid(),
        _familyId,
        _guardianId,
        "profile.set-guardian-access",
        JsonSerializer.SerializeToElement(new
        {
          memberId = supporterId.Value,
          guardianAccess = "admin",
        })
      ),
      CancellationToken.None
    );
    var changedReadModels = result.ChangedAggregates
      .Select(item => item.AggregateType)
      .ToArray();

    await Assert.That(changedReadModels).IsEquivalentTo(
    [
      "family-profile",
      "savings",
      "reward-shop",
    ]);
  }

  private static TestTypedAggregateStore CreateStore()
  {
    var timeProvider = new FixedTimeProvider(
      new DateTimeOffset(2026, 10, 12, 10, 0, 0, TimeSpan.Zero)
    );
    var family = new Family(
      _familyId,
      _guardianId,
      "Guardian",
      new IanaTimeZone("Europe/Zurich")
    );
    var economy = new FamilyEconomy(family);
    var progression = FamilyHouse.CreateInitial(family);
    progression.RegisterParticipant(_guardianId, false, 6);
    var house = FamilyHouseShop.CreateInitial(
      family,
      progression.StoredLevel,
      HouseSubscriptionTier.Pro
    );
    var store = new TestTypedAggregateStore(timeProvider);
    store.Seed(
      family,
      economy,
      progression: progression,
      house: house,
      updatedBy: _guardianId
    );

    return store;
  }

  private static TestTypedAggregateStore CreateEstablishedStore(FamilyMemberId supporterId)
  {
    var timeProvider = new FixedTimeProvider(
      new DateTimeOffset(2026, 10, 12, 10, 0, 0, TimeSpan.Zero)
    );
    var family = new Family(
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
      timeProvider.GetUtcNow()
    );
    family.AddMember(
      _guardianId,
      supporterId,
      "Supporter",
      MemberRole.Guardian,
      GuardianAccess.Supporter,
      false,
      timeProvider.GetUtcNow()
    );
    var economy = new FamilyEconomy(family);
    var progression = FamilyHouse.CreateInitial(family);
    var house = FamilyHouseShop.CreateInitial(
      family,
      progression.StoredLevel,
      HouseSubscriptionTier.Pro
    );
    var store = new TestTypedAggregateStore(timeProvider);
    store.Seed(
      family,
      economy,
      progression: progression,
      house: house,
      updatedBy: _guardianId
    );

    return store;
  }

  private static FamilyCommand CompleteSetupCommand(
    FamilyMemberId guardianId,
    FamilyMemberId childId
  )
  {
    return new FamilyCommand(
      Guid.NewGuid(),
      _familyId,
      guardianId,
      "profile.complete-setup",
      JsonSerializer.SerializeToElement(new
      {
        members = new object[]
        {
          new
          {
            id = guardianId.Value,
            name = "Guardian",
            avatar = "guardian-avatar",
            color = "#123456",
            role = "guardian",
            guardianAccess = "admin",
            participatesInWeeklyGoal = false,
          },
          new
          {
            id = childId.Value,
            name = "Child",
            avatar = "child-avatar",
            color = "#abcdef",
            role = "child",
            participatesInWeeklyGoal = true,
          },
        },
        pets = Array.Empty<object>(),
      })
    );
  }
}
