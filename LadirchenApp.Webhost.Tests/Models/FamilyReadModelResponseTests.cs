using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Models.ReadModels;

namespace LadirchenApp.Webhost.Tests.Models;

public sealed class FamilyReadModelResponseTests
{
  private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
  {
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
  };

  [Test]
  public async Task EveryReadModelResponseExposesStructuredState()
  {
    var responses = new Dictionary<object, Type>
    {
      [ContributionsReadModelResponse.FromSnapshot(Snapshot("contributions", ContributionsState()))] =
        typeof(ContributionsReadModelState),
      [FamilyProfileReadModelResponse.FromSnapshot(Snapshot("family-profile", FamilyProfileState()))] =
        typeof(FamilyProfileReadModelState),
      [FamilyProgressionReadModelResponse.FromSnapshot(Snapshot("family-progression", FamilyProgressionState()))] =
        typeof(FamilyProgressionReadModelState),
      [HomeCustomizationReadModelResponse.FromSnapshot(Snapshot("home-customization", HomeCustomizationState()))] =
        typeof(HomeCustomizationReadModelState),
      [RewardShopReadModelResponse.FromSnapshot(Snapshot("reward-shop", RewardShopState()))] =
        typeof(RewardShopReadModelState),
      [SavingsReadModelResponse.FromSnapshot(Snapshot("savings", SavingsState()))] =
        typeof(SavingsReadModelState),
    };

    foreach (var response in responses)
    {
      var stateProperty = response.Key.GetType().GetProperty("State");

      await Assert.That(stateProperty).IsNotNull();
      await Assert.That(stateProperty!.PropertyType).IsEqualTo(response.Value);
      await Assert.That(stateProperty.PropertyType).IsNotEqualTo(typeof(JsonElement));
    }
  }

  [Test]
  public async Task SavingsResponseOmitsPersistenceOnlyEconomyHistoryAndRefundProvenance()
  {
    var response = SavingsReadModelResponse.FromSnapshot(Snapshot("savings", SavingsState()));
    var json = JsonSerializer.SerializeToElement(response, _serializerOptions);
    var propertyNames = json.EnumerateObject().Select(property => property.Name).ToArray();
    var state = json.GetProperty("state");

    await Assert.That(propertyNames).IsEquivalentTo(["readModelType", "revision", "state"]);
    await Assert.That(response.ReadModelType).IsEqualTo("savings");
    await Assert.That(response.Revision).IsEqualTo(7);
    await Assert.That(state.TryGetProperty("ledger", out _)).IsFalse();
    await Assert.That(state.TryGetProperty("appliedOperationIds", out _)).IsFalse();
    var goal = state.GetProperty("goals")[0];
    await Assert.That(goal.TryGetProperty("refundableContributions", out _)).IsFalse();
    var cancellationRequest = goal.GetProperty("cancellationRequest");
    var commandLimits = goal.GetProperty("commandLimits").GetProperty("child");
    await Assert.That(cancellationRequest.GetProperty("requestedByMemberId").GetString())
      .IsEqualTo("guardian");
    await Assert.That(cancellationRequest.GetProperty("consentedMemberIds")[0].GetString())
      .IsEqualTo("guardian");
    await Assert.That(cancellationRequest.GetProperty("pendingMemberIds")[0].GetString())
      .IsEqualTo("child");
    await Assert.That(commandLimits.GetProperty("canRequestCancellation").GetBoolean()).IsFalse();
    await Assert.That(commandLimits.GetProperty("canApproveCancellation").GetBoolean()).IsTrue();
    await Assert.That(commandLimits.GetProperty("canRejectCancellation").GetBoolean()).IsTrue();
  }

  [Test]
  public async Task MissingRequiredReadModelStateIsRejected()
  {
    var snapshot = Snapshot("savings", new { });

    await Assert.That(() => SavingsReadModelResponse.FromSnapshot(snapshot)).Throws<JsonException>();
  }

  [Test]
  public async Task GeneratedOpenApiClientHasTypedReadModelStates()
  {
    var repositoryRoot = FindRepositoryRoot();
    var clientPath = Path.Combine(
      repositoryRoot,
      "LadirchenApp.Webhost",
      "Apps",
      "ladirchen-app",
      "api",
      "LadirchenApp.Webhost.ts"
    );
    var client = await File.ReadAllTextAsync(clientPath);
    var expectedStateProperties = new[]
    {
      "state: ContributionsReadModelState;",
      "state: FamilyProfileReadModelState;",
      "state: FamilyProgressionReadModelState;",
      "state: HomeCustomizationReadModelState;",
      "state: RewardShopReadModelState;",
      "state: SavingsReadModelState;",
      "cancellationRequest?: SavingGoalCancellationRequestReadModel | null;",
      "canApproveCancellation: boolean;",
      "canRejectCancellation: boolean;",
      "canRequestCancellation: boolean;",
    };

    foreach (var stateProperty in expectedStateProperties)
    {
      await Assert.That(client.Contains(stateProperty, StringComparison.Ordinal)).IsTrue();
    }

    await Assert.That(client.Contains("state?: any;", StringComparison.Ordinal)).IsFalse();
  }

  private static FamilyAggregateSnapshot Snapshot(string readModelType, object state)
  {
    return new FamilyAggregateSnapshot(
      new FamilyId(Guid.Parse("d9dd1e2c-2ad6-4d95-b52d-3b1a6ff112b1")),
      readModelType,
      2,
      7,
      JsonSerializer.SerializeToElement(state, _serializerOptions),
      new FamilyMemberId("guardian"),
      DateTimeOffset.Parse("2026-10-29T12:00:00+00:00")
    );
  }

  private static string FindRepositoryRoot()
  {
    var directory = new DirectoryInfo(AppContext.BaseDirectory);

    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LadirchenApp.slnx")))
    {
      directory = directory.Parent;
    }

    return directory?.FullName
      ?? throw new InvalidOperationException("The repository root could not be located.");
  }

  private static object ContributionsState()
  {
    return new
    {
      contributions = Array.Empty<object>(),
      currentWeekApprovedContributionCountByMember = new { },
      currentWeekEarnedByMember = new { },
      promotions = Array.Empty<object>(),
      usedContributionIds = new[] { "internal-contribution" },
    };
  }

  private static object FamilyProfileState()
  {
    var avatarCatalog = new
    {
      accessoryIds = Array.Empty<string>(),
      ages = Array.Empty<string>(),
      faceIds = Array.Empty<string>(),
      faceShapeIds = Array.Empty<string>(),
      funAccessoryIds = Array.Empty<string>(),
      hairColorIds = Array.Empty<string>(),
      hairIds = Array.Empty<string>(),
      outfitColorIds = Array.Empty<string>(),
      outfitIds = Array.Empty<string>(),
      seasonalAccessoryIds = Array.Empty<string>(),
      skinToneIds = Array.Empty<string>(),
    };

    return new
    {
      avatarCatalog = new { child = avatarCatalog, guardian = avatarCatalog },
      members = Array.Empty<object>(),
      onboardingCompleted = false,
      pets = Array.Empty<object>(),
      subscriptionTier = "free",
      timeZone = "Europe/Zurich",
      activeChildId = "internal-child",
    };
  }

  private static object FamilyProgressionState()
  {
    return new
    {
      canCloseWeekSuccessfully = false,
      completedWeeklyStreak = 0,
      currentWeekTarget = 7,
      familyEnergy = 0,
      houseAtmosphere = "rainy",
      houseLevel = 0,
      houseVisualLevel = 1,
      maximumSavingsInterestRate = 8,
      minimumSuccessfulWeekEnergy = 60,
      memberProgressions = new { },
      closedWeekOutcomes = new { },
      weeklyEvaluations = new { },
    };
  }

  private static object HomeCustomizationState()
  {
    return new
    {
      capabilities = new { canArrangeHouse = false },
      catalog = new
      {
        accessories = Array.Empty<object>(),
        areas = Array.Empty<object>(),
        furnitureSets = Array.Empty<object>(),
        houseStages = Array.Empty<object>(),
        roomDesigns = Array.Empty<object>(),
        themes = Array.Empty<object>(),
      },
      accessories = Array.Empty<object>(),
      editions = Array.Empty<object>(),
      placements = Array.Empty<object>(),
      roomDesigns = Array.Empty<object>(),
      selectedRoomDesigns = Array.Empty<object>(),
      selectedEditionId = "sunny-dollhouse",
    };
  }

  private static object RewardShopState()
  {
    return new
    {
      redemptionOpen = true,
      rewards = Array.Empty<object>(),
    };
  }

  private static object SavingsState()
  {
    return new
    {
      availableBalances = new { },
      balances = new { },
      familyCurrencyCode = "CHF",
      goals = new[]
      {
        new
        {
          id = "family-trip",
          title = "Family trip",
          icon = "trip",
          ownerId = "family",
          target = 100,
          saved = 20,
          starterBonus = 5,
          participantIds = new[] { "child" },
          refundableContributions = new[] { new { memberId = "child", amount = 15 } },
          cancellationRequest = new
          {
            requestedByMemberId = "guardian",
            consentedMemberIds = new[] { "guardian" },
            pendingMemberIds = new[] { "child" },
          },
          commandLimits = new
          {
            child = new
            {
              canApproveCancellation = true,
              canCancel = false,
              canRejectCancellation = true,
              canRequestCancellation = false,
              canWithdraw = false,
              maximumDepositAmount = 0,
              maximumSupportAmount = 0,
            },
          },
          visibility = "family",
          shared = true,
          cheered = false,
        },
      },
      ladirchenPerCurrencyUnit = 1,
      pendingGuardianGifts = Array.Empty<object>(),
      settledInterestPeriods = Array.Empty<string>(),
      sharedSavedBalances = new { },
      vouchers = Array.Empty<object>(),
      ledger = new[] { new { id = "internal-entry" } },
      appliedOperationIds = new[] { "internal-operation" },
    };
  }
}
