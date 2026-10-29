using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Commands;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

public sealed class HomeCommandHandlerTests
{
  private static readonly FamilyId _familyId = new(Guid.Parse("cbe09dd1-2036-4e47-865b-da4cf973392a"));
  private static readonly FamilyMemberId _administratorId = new("guardian");
  private static readonly FamilyMemberId _childId = new("child");

  [Test]
  public async Task PurchaseUsesAvailableBalanceAndPreservesPresentationFields()
  {
    var repository = CreateRepository(balance: 100, reservedRewardPrice: 50, subscriptionTier: "pro");
    var handler = CreateHandler(repository);

    await ExecuteAsync(handler, _childId, "home.purchase-accessory", new
    {
      id = "wall-art"
    });

    var wallArt = Accessory(repository, "wall-art");
    await Assert.That(wallArt.GetProperty("owned").GetBoolean()).IsTrue();
    await Assert.That(wallArt.GetProperty("equipped").GetBoolean()).IsFalse();
    await Assert.That(wallArt.GetProperty("spriteKey").GetString()).IsEqualTo("wall-art-sprite");
    await Assert.That(Balance(repository)).IsEqualTo(55);
    await Assert.That(repository.Required(_familyId, "reward-shop").Revision).IsEqualTo(1);
    await Assert.That(repository.LastRevisionGuards.Select(item => item.AggregateType))
      .IsEquivalentTo(["family-profile", "family-progression", "reward-shop"]);

    var homeRevision = repository.Required(_familyId, "home-customization").Revision;
    var savingsRevision = repository.Required(_familyId, "savings").Revision;

    await Assert.That(async () =>
      await ExecuteAsync(handler, _childId, "home.purchase-accessory", new
      {
        id = "floor-lamp"
      })
    ).Throws<FamilyCommandException>();
    await Assert.That(repository.Required(_familyId, "home-customization").Revision)
      .IsEqualTo(homeRevision);
    await Assert.That(repository.Required(_familyId, "savings").Revision)
      .IsEqualTo(savingsRevision);
    await Assert.That(Balance(repository)).IsEqualTo(55);
  }

  [Test]
  public async Task LayoutUsesDomainCompatibilityBoundsCollisionsAndRevisionGuards()
  {
    var repository = CreateRepository(balance: 500, reservedRewardPrice: null, subscriptionTier: "pro");
    var handler = CreateHandler(repository);

    await ExecuteAsync(handler, _childId, "home.move-entity", new
    {
      placementId = "furniture-flower-boxes",
      zoneId = "living-room",
      x = 120,
      y = 110,
    });
    var flowerBoxes = Placement(repository, "furniture-flower-boxes");
    await Assert.That(flowerBoxes.GetProperty("zoneId").GetString()).IsEqualTo("garden");
    await Assert.That(flowerBoxes.GetProperty("x").GetDecimal()).IsEqualTo(97);
    await Assert.That(flowerBoxes.GetProperty("y").GetDecimal()).IsEqualTo(94);
    await Assert.That(repository.LastRevisionGuards.Select(item => item.AggregateType))
      .IsEquivalentTo(["family-profile", "family-progression"]);

    var revisionBeforeCollision = repository.Required(_familyId, "home-customization").Revision;

    await Assert.That(async () =>
      await ExecuteAsync(handler, _childId, "home.move-entity", new
      {
        placementId = "member-child",
        zoneId = "living-room",
        x = 70,
        y = 62,
      })
    ).Throws<FamilyCommandException>();
    await Assert.That(repository.Required(_familyId, "home-customization").Revision)
      .IsEqualTo(revisionBeforeCollision);

    await ExecuteAsync(handler, _childId, "home.move-entity", new
    {
      placementId = "member-child",
      zoneId = "living-room",
      x = -10,
      y = 2,
    });
    var child = Placement(repository, "member-child");
    await Assert.That(child.GetProperty("x").GetDecimal()).IsEqualTo(7);
    await Assert.That(child.GetProperty("y").GetDecimal()).IsEqualTo(62);
  }

  [Test]
  public async Task PurchasesAndArrangementEnforceRoleLevelAndSubscriptionInTheDomain()
  {
    var freeRepository = CreateRepository(balance: 500, reservedRewardPrice: null, subscriptionTier: "free");
    var freeHandler = CreateHandler(freeRepository);

    await Assert.That(async () =>
      await ExecuteAsync(freeHandler, _childId, "home.move-entity", new
      {
        placementId = "member-child",
        zoneId = "living-room",
        x = 40,
        y = 70,
      })
    ).Throws<FamilyCommandException>();

    await Assert.That(async () =>
      await ExecuteAsync(freeHandler, _childId, "home.purchase-accessory", new
      {
        id = "bookshelf"
      })
    ).Throws<FamilyCommandException>();

    await Assert.That(async () =>
      await ExecuteAsync(freeHandler, _administratorId, "home.purchase-accessory", new
      {
        id = "wall-art"
      })
    ).Throws<FamilyCommandException>();
  }

  [Test]
  public async Task HomeReadModelUsesTypedPurchaseCapabilitiesAndReservedBalances()
  {
    var repository = CreateRepository(balance: 100, reservedRewardPrice: 50, subscriptionTier: "pro");
    var handler = new GetFamilyAggregateQueryHandler(
      repository,
      new FixedTimeProvider(new DateTimeOffset(2026, 10, 10, 15, 0, 0, TimeSpan.Zero))
    );

    var snapshot = await handler.HandleAsync(
      new GetFamilyAggregateQuery(_familyId, "home-customization"),
      CancellationToken.None
    );

    await Assert.That(snapshot).IsNotNull();
    var state = snapshot!.State;
    await Assert.That(state.GetProperty("capabilities").GetProperty("canArrangeHouse").GetBoolean())
      .IsTrue();
    var catalog = state.GetProperty("catalog");
    var wallArt = catalog.GetProperty("accessories").EnumerateArray()
      .Single(item => item.GetProperty("id").GetString() == "wall-art");
    var floorLamp = catalog.GetProperty("accessories").EnumerateArray()
      .Single(item => item.GetProperty("id").GetString() == "floor-lamp");
    await Assert.That(wallArt.GetProperty("purchasableByMemberIds").EnumerateArray()
      .Select(item => item.GetString()!))
      .IsEquivalentTo([_childId.Value]);
    await Assert.That(floorLamp.GetProperty("purchasableByMemberIds").GetArrayLength()).IsEqualTo(0);
    var unavailableThemeDesign = catalog.GetProperty("roomDesigns").EnumerateArray()
      .First(item => item.GetProperty("themeId").GetString() == "halloween-night");
    await Assert.That(unavailableThemeDesign.GetProperty("purchasableByMemberIds").GetArrayLength())
      .IsEqualTo(0);
  }

  private static ExecuteFamilyCommandHandler CreateHandler(
    TestFamilyAggregateSnapshotRepository repository
  )
  {
    return new ExecuteFamilyCommandHandler(
      repository,
      new ImmediateFamilyCommandExecutionCoordinator(),
      new FixedTimeProvider(new DateTimeOffset(2026, 10, 10, 15, 0, 0, TimeSpan.Zero))
    );
  }

  private static TestFamilyAggregateSnapshotRepository CreateRepository(
    decimal balance,
    decimal? reservedRewardPrice,
    string subscriptionTier
  )
  {
    var repository = new TestFamilyAggregateSnapshotRepository();
    repository.Seed(Snapshot("family-profile", 2, new
    {
      timeZone = "Europe/Zurich",
      subscriptionTier,
      pets = Array.Empty<object>(),
      members = new object[]
      {
        new
        {
          id = _administratorId.Value,
          name = "Guardian",
          role = "guardian",
          guardianAccess = "admin",
          participatesInWeeklyGoal = false,
        },
        new
        {
          id = _childId.Value,
          name = "Child",
          role = "child",
          participatesInWeeklyGoal = true,
        },
      },
    }));
    repository.Seed(Snapshot("family-progression", 4, new
    {
      houseLevel = 0
    }));
    repository.Seed(Snapshot("savings", 2, new
    {
      balances = new
      {
        guardian = 0,
        child = balance,
      },
    }));
    repository.Seed(Snapshot("reward-shop", 1, RewardShop(reservedRewardPrice)));
    repository.Seed(Snapshot("home-customization", 3, HomeState()));

    return repository;
  }

  private static object RewardShop(decimal? reservedRewardPrice)
  {
    return reservedRewardPrice is null
      ? new
      {
        rewards = Array.Empty<object>()
      }
      : new
      {
        rewards = new object[]
        {
          new
          {
            id = "reserved-reward",
            title = "Reserved reward",
            category = "gift",
            price = reservedRewardPrice.Value,
            quantity = 1,
            isVisible = true,
            status = "requested",
            requestId = "reserved-request",
            requesterId = _childId.Value,
            requestedAt = "2026-10-10T12:00:00+00:00",
          },
        },
      };
  }

  private static JsonObject HomeState()
  {
    var initiallyOwned = new HashSet<string>(StringComparer.Ordinal)
    {
      "flower-boxes",
      "round-rug",
      "cozy-sofa",
      "kitchen-counter",
      "retro-fridge",
      "family-dining-table",
    };
    var initiallyOwnedDesigns = new HashSet<string>(StringComparer.Ordinal)
    {
      "garden-ladi-hills",
      "living-ladi-classic",
      "kitchen-ladi-classic",
    };
    var accessories = new JsonArray();
    var placements = new JsonArray();

    foreach (var item in HouseShopCatalog.Accessories)
    {
      accessories.Add(new JsonObject
      {
        ["id"] = item.Key,
        ["owned"] = initiallyOwned.Contains(item.Key),
        ["equipped"] = initiallyOwned.Contains(item.Key),
        ["spriteKey"] = $"{item.Key}-sprite",
      });
      placements.Add(new JsonObject
      {
        ["id"] = $"furniture-{item.Key}",
        ["entityId"] = item.Key,
        ["entityType"] = "furniture",
        ["zoneId"] = item.Value.Initial.Area,
        ["x"] = item.Value.Initial.X,
        ["y"] = item.Value.Initial.Y,
        ["scale"] = item.Value.Initial.Scale,
      });
    }

    return new JsonObject
    {
      ["accessories"] = accessories,
      ["editions"] = new JsonArray(
        HouseShopCatalog.Themes
          .Select(item => new JsonObject
          {
            ["id"] = item.Key,
            ["owned"] = item.Key == "sunny-dollhouse",
          })
          .ToArray<JsonNode?>()
      ),
      ["placements"] = placements,
      ["roomDesigns"] = new JsonArray(
        HouseShopCatalog.RoomDesigns
          .Select(item => new JsonObject
          {
            ["id"] = item.Key,
            ["owned"] = initiallyOwnedDesigns.Contains(item.Key),
          })
          .ToArray<JsonNode?>()
      ),
      ["selectedEditionId"] = "sunny-dollhouse",
      ["selectedRoomDesigns"] = new JsonArray
      {
        new JsonObject { ["designId"] = "garden-ladi-hills", ["zoneId"] = "garden" },
        new JsonObject { ["designId"] = "living-ladi-classic", ["zoneId"] = "living-room" },
        new JsonObject { ["designId"] = "kitchen-ladi-classic", ["zoneId"] = "kitchen" },
      },
      ["visualState"] = "preserved",
    };
  }

  private static FamilyAggregateSnapshot Snapshot(
    string aggregateType,
    int schemaVersion,
    object state
  )
  {
    var serialized = state is JsonNode node
      ? JsonDocument.Parse(node.ToJsonString()).RootElement.Clone()
      : JsonSerializer.SerializeToElement(state);

    return new FamilyAggregateSnapshot(
      _familyId,
      aggregateType,
      schemaVersion,
      1,
      serialized,
      _administratorId,
      DateTimeOffset.UnixEpoch
    );
  }

  private static async Task ExecuteAsync(
    ExecuteFamilyCommandHandler handler,
    FamilyMemberId actorId,
    string commandType,
    object payload
  )
  {
    await handler.HandleAsync(
      new FamilyCommand(
        Guid.NewGuid(),
        _familyId,
        actorId,
        commandType,
        JsonSerializer.SerializeToElement(payload)
      ),
      CancellationToken.None
    );
  }

  private static JsonElement Accessory(
    TestFamilyAggregateSnapshotRepository repository,
    string id
  )
  {
    return repository.Required(_familyId, "home-customization")
      .State
      .GetProperty("accessories")
      .EnumerateArray()
      .Single(item => item.GetProperty("id").GetString() == id);
  }

  private static JsonElement Placement(
    TestFamilyAggregateSnapshotRepository repository,
    string id
  )
  {
    return repository.Required(_familyId, "home-customization")
      .State
      .GetProperty("placements")
      .EnumerateArray()
      .Single(item => item.GetProperty("id").GetString() == id);
  }

  private static decimal Balance(TestFamilyAggregateSnapshotRepository repository)
  {
    return repository.Required(_familyId, "savings")
      .State
      .GetProperty("balances")
      .GetProperty(_childId.Value)
      .GetDecimal();
  }
}
