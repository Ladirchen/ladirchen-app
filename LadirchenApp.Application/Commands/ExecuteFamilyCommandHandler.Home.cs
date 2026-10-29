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

public sealed partial class ExecuteFamilyCommandHandler
{
  private async Task<FamilyCommandResult> ExecuteHomeAsync(
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
      availableBalance,
      houseLevel
    );
    var balanceBefore = state.Shop.Balance;
    ExecuteHomeCommand(state, family, command, payload);

    if (isPurchase)
    {
      var price = balanceBefore - state.Shop.Balance;
      economy!.PurchaseHouseContent(
        command.ActorId,
        new LadirchenAmount(price),
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

  private static void ExecuteHomeCommand(
    HomeCustomizationState state,
    Family family,
    FamilyCommand command,
    JsonObject payload
  )
  {
    var actorIsChild = family.ActiveMember(command.ActorId).Role == MemberRole.Child;

    switch (command.CommandType)
    {
      case "home.purchase-accessory":
        RequireHomeAction(
          state.Shop.PurchaseAccessory(state.Accessory(RequiredString(payload, "id")), actorIsChild),
          "The house accessory cannot be purchased."
        );
        break;
      case "home.purchase-furniture-set":
        RequireHomeAction(
          state.Shop.PurchaseSet(state.FurnitureSet(RequiredString(payload, "id")), actorIsChild),
          "The furniture set cannot be purchased."
        );
        break;
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
        PurchaseTheme(state, payload, actorIsChild);
        break;
      case "home.select-theme":
        SelectTheme(state, payload);
        break;
      case "home.purchase-room-design":
        PurchaseRoomDesign(state, payload, actorIsChild);
        break;
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

  private static void PurchaseTheme(
    HomeCustomizationState state,
    JsonObject payload,
    bool actorIsChild
  )
  {
    var theme = state.Theme(RequiredString(payload, "id"));
    RequireHomeAction(
      state.Shop.PurchaseTheme(theme, state.RoomDesigns, actorIsChild),
      "The house theme cannot be purchased."
    );
  }

  private static void SelectTheme(HomeCustomizationState state, JsonObject payload)
  {
    var theme = state.Theme(RequiredString(payload, "id"));
    RequireHomeAction(
      state.Shop.SelectTheme(theme, state.RoomDesigns),
      "The house theme cannot be selected."
    );
  }

  private static void PurchaseRoomDesign(
    HomeCustomizationState state,
    JsonObject payload,
    bool actorIsChild
  )
  {
    var design = state.RoomDesign(RequiredString(payload, "id"));
    RequireHomeAction(
      state.Shop.PurchaseDesign(
        design,
        state.ThemeFor(design),
        state.RoomDesigns,
        actorIsChild
      ),
      "The room design cannot be purchased."
    );
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
