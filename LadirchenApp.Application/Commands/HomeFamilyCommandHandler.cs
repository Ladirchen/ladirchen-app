using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal sealed class HomeFamilyCommandHandler : FamilyCommandAreaHandler
{
  public HomeFamilyCommandHandler(
    IFamilyAggregateSnapshotRepository repository,
    TimeProvider timeProvider
  )
    : base(repository, timeProvider, "home.")
  {
  }

  public override async Task<FamilyCommandResult> HandleAsync(
    FamilyCommand command,
    FamilyAggregateSnapshot profileSnapshot,
    JsonObject profile,
    JsonObject payload,
    CancellationToken cancellationToken
  )
  {
    var family = FamilyProfileSnapshotMapper.Rehydrate(command.FamilyId, profile);
    var home = await RequireSnapshotAsync(command.FamilyId, "home-customization", cancellationToken);
    var homeState = RequireObject(home.State);
    var progression = await RequireSnapshotAsync(command.FamilyId, "family-progression", cancellationToken);
    var houseLevel = FamilyProgressionSnapshotMapper.ReadHouseLevel(RequireObject(progression.State));
    var isPurchase = IsHomePurchase(command.CommandType);
    FamilyAggregateSnapshot? savings = null;
    FamilyAggregateSnapshot? rewardShop = null;
    JsonObject? savingsState = null;
    FamilyEconomy? economy = null;
    var availableBalance = 0m;

    if (isPurchase)
    {
      savings = await RequireSnapshotAsync(command.FamilyId, "savings", cancellationToken);
      savingsState = RequireObject(savings.State);
      rewardShop = await RequireSnapshotAsync(command.FamilyId, "reward-shop", cancellationToken);
      var rewardShopState = RequireObject(rewardShop.State);
      economy = FamilyEconomySnapshotMapper.RehydrateRewardShop(
        family,
        rewardShopState,
        savingsState
      );
      availableBalance = economy.WalletFor(command.ActorId).Available;
    }

    var state = HomeCustomizationSnapshotMapper.Rehydrate(
      homeState,
      profile,
      houseLevel
    );
    var purchasePrice = ExecuteHomeCommand(
      state,
      family,
      command,
      payload,
      availableBalance
    );

    if (isPurchase)
    {
      economy!.PurchaseHouseContent(
        command.ActorId,
        new LadirchenAmount(purchasePrice),
        new HouseContentId($"{command.CommandType}:{RequiredString(payload, "id")}"),
        new TransactionId(command.CommandId.ToString("N", CultureInfo.InvariantCulture)),
        _timeProvider.GetUtcNow()
      );
    }

    var updatedHomeState = HomeCustomizationSnapshotMapper.Write(state);
    var saves = new List<PendingFamilyAggregateSave>
    {
      CreateSave(home, updatedHomeState, command.ActorId),
    };

    if (savings is not null && savingsState is not null)
    {
      saves.Add(
        CreateSave(
          savings,
          FamilyEconomySnapshotMapper.WriteWalletBalances(savingsState, economy!),
          command.ActorId
        )
      );
    }

    var revisionGuards = new List<FamilyAggregateRevisionGuard>
    {
      FamilyAggregateRevisionGuard.FromSnapshot(profileSnapshot),
      FamilyAggregateRevisionGuard.FromSnapshot(progression),
    };

    if (rewardShop is not null)
    {
      revisionGuards.Add(FamilyAggregateRevisionGuard.FromSnapshot(rewardShop));
    }

    var changed = await _repository.SaveBatchAsync(saves, revisionGuards, cancellationToken);

    return new FamilyCommandResult(changed);
  }

  private static decimal ExecuteHomeCommand(
    HomeCustomizationState state,
    Family family,
    FamilyCommand command,
    JsonObject payload,
    decimal availableBalance
  )
  {
    var actorIsChild = family.ActiveMember(command.ActorId).Role == MemberRole.Child;

    switch (command.CommandType)
    {
      case "home.purchase-accessory":
        var accessory = state.Accessory(RequiredString(payload, "id"));
        RequireHomeAction(
          state.Shop.PurchaseAccessory(accessory, actorIsChild, availableBalance),
          "The house accessory cannot be purchased."
        );

        return accessory.Price;
      case "home.purchase-furniture-set":
        var furnitureSet = state.FurnitureSet(RequiredString(payload, "id"));
        RequireHomeAction(
          state.Shop.PurchaseSet(furnitureSet, actorIsChild, availableBalance),
          "The furniture set cannot be purchased."
        );

        return furnitureSet.Price;
      case "home.toggle-accessory":
        ToggleAccessory(state, payload);
        break;
      case "home.store-accessory":
        StoreAccessory(state, payload);
        break;
      case "home.place-accessory":
        PlaceAccessory(state, payload);
        break;
      case "home.purchase-theme":
        return PurchaseTheme(state, payload, actorIsChild, availableBalance);
      case "home.select-theme":
        SelectTheme(state, payload);
        break;
      case "home.purchase-room-design":
        return PurchaseRoomDesign(state, payload, actorIsChild, availableBalance);
      case "home.select-room-design":
        SelectRoomDesign(state, payload);
        break;
      case "home.move-entity":
        MoveHouseEntity(state, payload);
        break;
      case "home.reset-entity":
        ResetHouseEntity(state, payload);
        break;
      default:
        throw new FamilyCommandException("The home-customization command is not supported.");
    }

    return 0;
  }

  private static void ToggleAccessory(HomeCustomizationState state, JsonObject payload)
  {
    var item = state.Accessory(RequiredString(payload, "id"));
    RequireHomeAction(state.Shop.Toggle(item), "The house accessory cannot be toggled.");
    state.UpdateFurniturePlacement(item);
  }

  private static void StoreAccessory(HomeCustomizationState state, JsonObject payload)
  {
    var item = state.Accessory(RequiredString(payload, "id"));
    RequireHomeAction(state.Shop.Store(item), "The house accessory cannot be stored.");
    state.UpdateFurniturePlacement(item);
  }

  private static void PlaceAccessory(HomeCustomizationState state, JsonObject payload)
  {
    var item = state.Accessory(RequiredString(payload, "id"));
    RequireHomeAction(
      state.Shop.Place(item, RequiredString(payload, "zoneId")),
      "The house accessory cannot be placed."
    );
    state.UpdateFurniturePlacement(item);
  }

  private static decimal PurchaseTheme(
    HomeCustomizationState state,
    JsonObject payload,
    bool actorIsChild,
    decimal availableBalance
  )
  {
    var theme = state.Theme(RequiredString(payload, "id"));
    RequireHomeAction(
      state.Shop.PurchaseTheme(theme, state.RoomDesigns, actorIsChild, availableBalance),
      "The house theme cannot be purchased."
    );

    return theme.Price;
  }

  private static void SelectTheme(HomeCustomizationState state, JsonObject payload)
  {
    var theme = state.Theme(RequiredString(payload, "id"));
    RequireHomeAction(
      state.Shop.SelectTheme(theme, state.RoomDesigns),
      "The house theme cannot be selected."
    );
  }

  private static decimal PurchaseRoomDesign(
    HomeCustomizationState state,
    JsonObject payload,
    bool actorIsChild,
    decimal availableBalance
  )
  {
    var design = state.RoomDesign(RequiredString(payload, "id"));
    RequireHomeAction(
      state.Shop.PurchaseDesign(
        design,
        state.ThemeFor(design),
        state.RoomDesigns,
        actorIsChild,
        availableBalance
      ),
      "The room design cannot be purchased."
    );

    return design.Price;
  }

  private static void SelectRoomDesign(HomeCustomizationState state, JsonObject payload)
  {
    var design = state.RoomDesign(RequiredString(payload, "id"));
    RequireHomeAction(
      state.Shop.SelectDesign(design, state.ThemeFor(design), state.RoomDesigns),
      "The room design cannot be selected."
    );
  }

  private static void MoveHouseEntity(HomeCustomizationState state, JsonObject payload)
  {
    var entity = state.Placement(RequiredString(payload, "placementId"));
    var placement = state.Shop.ArrangeEntity(
      entity,
      RequiredString(payload, "zoneId"),
      RequiredDecimal(payload, "x"),
      RequiredDecimal(payload, "y"),
      state.SubscriptionTier,
      state.FurnitureFor(entity),
      state.Placements
    );
    RequireHomeAction(placement is not null, "The house entity cannot be moved.");
    state.UpdatePlacement(entity, placement!);
  }

  private static void ResetHouseEntity(HomeCustomizationState state, JsonObject payload)
  {
    var entity = state.Placement(RequiredString(payload, "placementId"));
    var placement = state.Shop.ResetEntity(
      entity,
      state.InitialPlacement(entity.Id),
      state.SubscriptionTier,
      state.FurnitureFor(entity),
      state.Placements
    );
    RequireHomeAction(placement is not null, "The house entity cannot be reset.");
    state.UpdatePlacement(entity, placement!);
  }

  private static bool IsHomePurchase(string commandType)
  {
    return commandType is
      "home.purchase-accessory"
      or "home.purchase-furniture-set"
      or "home.purchase-theme"
      or "home.purchase-room-design";
  }

  private static void RequireHomeAction(bool accepted, string message)
  {
    if (!accepted)
    {
      throw new FamilyCommandException(message);
    }
  }
}
