using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Commands;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

public sealed class ProfileCommandHandlerTests
{
  private static readonly FamilyId _familyId = new(Guid.Parse("5f418b52-314f-4165-92a0-dbf2e282a7bc"));
  private static readonly FamilyMemberId _guardianId = new("guardian");
  private static readonly FamilyMemberId _childId = new("child");

  [Test]
  public async Task CompleteSetupKeepsServerStreaksAndPersistsOnlyAllowedPresentationFields()
  {
    var repository = CreateRepository();
    var handler = new ExecuteFamilyCommandHandler(
      repository,
      new ImmediateFamilyCommandExecutionCoordinator(),
      new FixedTimeProvider(new DateTimeOffset(2026, 10, 12, 10, 0, 0, TimeSpan.Zero))
    );

    await handler.HandleAsync(
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
              permissions = new { canManageFamily = true },
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
              permissions = new { canManageFamily = true },
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

    var profile = repository.Required(_familyId, "family-profile").State;
    var members = profile.GetProperty("members")
      .EnumerateArray()
      .ToDictionary(item => item.GetProperty("id").GetString() ?? string.Empty);
    var guardian = members[_guardianId.Value];
    var child = members[_childId.Value];
    var pet = profile.GetProperty("pets").EnumerateArray().Single();

    await Assert.That(guardian.GetProperty("weeklyStreak").GetInt32()).IsEqualTo(6);
    await Assert.That(child.GetProperty("weeklyStreak").GetInt32()).IsEqualTo(0);
    await Assert.That(guardian.GetProperty("avatar").GetString()).IsEqualTo("guardian-avatar");
    await Assert.That(child.GetProperty("color").GetString()).IsEqualTo("#abcdef");
    await Assert.That(guardian.TryGetProperty("unknownMemberField", out _)).IsFalse();
    await Assert.That(guardian.TryGetProperty("permissions", out _)).IsFalse();
    await Assert.That(child.TryGetProperty("unknownMemberField", out _)).IsFalse();
    await Assert.That(child.TryGetProperty("permissions", out _)).IsFalse();
    await Assert.That(
      guardian.GetProperty("appearance").TryGetProperty("unknownAppearanceField", out _)
    ).IsFalse();
    await Assert.That(
      child.GetProperty("appearance").TryGetProperty("unknownAppearanceField", out _)
    ).IsFalse();
    await Assert.That(pet.GetProperty("avatar").GetString()).IsEqualTo("pet-avatar");
    await Assert.That(pet.GetProperty("color").GetString()).IsEqualTo("#fedcba");
    await Assert.That(pet.TryGetProperty("unknownPetField", out _)).IsFalse();
    await Assert.That(profile.GetProperty("onboardingCompleted").GetBoolean()).IsTrue();
  }

  private static TestFamilyAggregateSnapshotRepository CreateRepository()
  {
    var repository = new TestFamilyAggregateSnapshotRepository();
    repository.Seed(Snapshot("family-profile", 2, new
    {
      timeZone = "Europe/Zurich",
      onboardingCompleted = false,
      subscriptionTier = "free",
      pets = Array.Empty<object>(),
      members = new object[]
      {
        new
        {
          id = _guardianId.Value,
          name = "Guardian",
          avatar = "stored-avatar",
          color = "#000000",
          role = "guardian",
          guardianAccess = "admin",
          invitationPending = false,
          participatesInWeeklyGoal = false,
          weeklyStreak = 6,
        },
      },
    }));
    repository.Seed(Snapshot("contributions", 1, new
    {
      contributions = Array.Empty<object>(),
      perfectRatingBonusPercent = 5,
      promotions = Array.Empty<object>(),
    }));
    repository.Seed(Snapshot("family-progression", 4, new
    {
      completedWeeklyStreak = 0,
      closedWeekOutcomes = new
      {
      },
      currentWeekTarget = 7,
      familyEnergy = 0,
      houseLevel = 0,
      houseVisualLevel = 0,
      maximumSavingsInterestRate = 8,
      memberProgressions = new
      {
      },
      weeklyEvaluations = new
      {
      },
    }));

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
      _guardianId,
      DateTimeOffset.UnixEpoch
    );
  }
}
