using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
      [new ContributionsReadModelResponse("contributions", 1, EmptyContributions())] =
        typeof(ContributionsReadModelState),
      [new FamilyProfileReadModelResponse("family-profile", 1, EmptyProfile())] =
        typeof(FamilyProfileReadModelState),
      [new FamilyProgressionReadModelResponse("family-progression", 1, EmptyProgression())] =
        typeof(FamilyProgressionReadModelState),
      [new HomeCustomizationReadModelResponse("home-customization", 1, EmptyHome())] =
        typeof(HomeCustomizationReadModelState),
      [new RewardShopReadModelResponse("reward-shop", 1, new(false, []))] =
        typeof(RewardShopReadModelState),
      [new SavingsReadModelResponse("savings", 1, EmptySavings())] =
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
    var goal = new SavingGoalReadModel(
      "family-trip",
      null,
      "Family trip",
      "trip",
      "family",
      100,
      20,
      5,
      null,
      0,
      1,
      80,
      15,
      ["child"],
      ["child"],
      null,
      new SavingGoalCancellationRequestReadModel("guardian", ["guardian"], ["child"]),
      new Dictionary<string, SavingsGoalCommandLimitsReadModel>
      {
        ["child"] = new(true, false, true, false, false, 0, 0),
      },
      "family",
      true,
      false
    );
    var response = new SavingsReadModelResponse(
      "savings",
      7,
      new SavingsReadModelState(
        new Dictionary<string, decimal>(),
        new Dictionary<string, decimal>(),
        "CHF",
        [goal],
        1,
        [],
        [],
        new Dictionary<string, decimal>(),
        []
      )
    );
    var json = JsonSerializer.SerializeToElement(response, _serializerOptions);
    var propertyNames = json.EnumerateObject().Select(property => property.Name).ToArray();
    var state = json.GetProperty("state");

    await Assert.That(propertyNames).IsEquivalentTo(["readModelType", "revision", "state"]);
    await Assert.That(response.ReadModelType).IsEqualTo("savings");
    await Assert.That(response.Revision).IsEqualTo(7);
    await Assert.That(state.TryGetProperty("ledger", out _)).IsFalse();
    await Assert.That(state.TryGetProperty("appliedOperationIds", out _)).IsFalse();
    await Assert.That(state.GetProperty("goals")[0].TryGetProperty("refundableContributions", out _))
      .IsFalse();
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

  private static ContributionsReadModelState EmptyContributions()
  {
    return new ContributionsReadModelState(
      [],
      new Dictionary<string, int>(),
      new Dictionary<string, decimal>(),
      15,
      [],
      new Dictionary<string, decimal>()
    );
  }

  private static FamilyProfileReadModelState EmptyProfile()
  {
    var catalog = new AvatarProfileCatalogReadModel([], [], [], [], [], [], [], [], [], [], []);

    return new FamilyProfileReadModelState(
      new AvatarCatalogReadModel(catalog, catalog),
      [],
      false,
      [],
      "free",
      "Europe/Zurich"
    );
  }

  private static FamilyProgressionReadModelState EmptyProgression()
  {
    return new FamilyProgressionReadModelState(
      false,
      0,
      7,
      0,
      "rainy",
      0,
      1,
      8,
      60,
      new Dictionary<string, MemberProgressionReadModel>()
    );
  }

  private static HomeCustomizationReadModelState EmptyHome()
  {
    return new HomeCustomizationReadModelState(
      new HomeCapabilitiesReadModel(false),
      new HomeCatalogReadModel([], [], [], [], [], []),
      [],
      [],
      [],
      [],
      [],
      "sunny-dollhouse"
    );
  }

  private static SavingsReadModelState EmptySavings()
  {
    return new SavingsReadModelState(
      new Dictionary<string, decimal>(),
      new Dictionary<string, decimal>(),
      "CHF",
      [],
      1,
      [],
      [],
      new Dictionary<string, decimal>(),
      []
    );
  }
}
