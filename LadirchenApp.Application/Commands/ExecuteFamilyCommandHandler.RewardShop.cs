using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

public sealed partial class ExecuteFamilyCommandHandler
{
  private async Task<FamilyCommandResult> ExecuteRewardShopAsync(
    FamilyCommand command,
    FamilyAggregateSnapshot profileSnapshot,
    JsonObject profileState,
    JsonObject payload,
    CancellationToken cancellationToken
  )
  {
    var family = FamilyProfileSnapshotMapper.Rehydrate(command.FamilyId, profileState);
    var shop = await RequireSnapshotAsync(command.FamilyId, "reward-shop", cancellationToken);
    var shopState = RequireObject(shop.State);
    FamilyAggregateSnapshot? savings = null;
    JsonObject? savingsState = null;

    if (RequiresWalletState(command.CommandType))
    {
      savings = await RequireSnapshotAsync(command.FamilyId, "savings", cancellationToken);
      savingsState = RequireObject(savings.State);
    }

    var economy = FamilyEconomySnapshotMapper.RehydrateRewardShop(family, shopState, savingsState);
    var now = _timeProvider.GetUtcNow();
    var operationId = new TransactionId(command.CommandId.ToString("N", CultureInfo.InvariantCulture));
    ShopRewardPresentation? addedReward = null;

    switch (command.CommandType)
    {
      case "reward-shop.add":
        addedReward = AddShopReward(economy, command.ActorId, payload, now);
        break;
      case "reward-shop.set-visibility":
        economy.SetShopRewardVisibility(
          command.ActorId,
          RewardId(payload),
          RequiredBool(payload, "isVisible"),
          now
        );
        break;
      case "reward-shop.delete":
        economy.DeleteShopReward(command.ActorId, RewardId(payload), operationId, now);
        break;
      case "reward-shop.request":
        economy.RequestShopReward(
          command.ActorId,
          RewardId(payload),
          new RedemptionRequestId(command.CommandId.ToString("N", CultureInfo.InvariantCulture)),
          operationId,
          now
        );
        break;
      case "reward-shop.cancel":
        economy.CancelShopRewardRequest(
          command.ActorId,
          PendingRequestId(economy, RewardId(payload)),
          operationId,
          now
        );
        break;
      case "reward-shop.decide":
        DecideShopReward(economy, command.ActorId, payload, operationId, now);
        break;
      default:
        throw new FamilyCommandException("The reward-shop command is not supported.");
    }

    var updatedShopState = FamilyEconomySnapshotMapper.WriteRewardShop(
      shopState,
      economy,
      addedReward
    );
    var saves = new List<PendingFamilyAggregateSave>
    {
      CreateSave(shop, updatedShopState, command.ActorId),
    };

    if (savings is not null && savingsState is not null)
    {
      var updatedSavingsState = FamilyEconomySnapshotMapper.WriteWalletBalances(
        savingsState,
        economy
      );
      saves.Add(CreateSave(savings, updatedSavingsState, command.ActorId));
    }

    var revisionGuards = new[]
    {
      FamilyAggregateRevisionGuard.FromSnapshot(profileSnapshot),
    };
    var changed = await _repository.SaveBatchAsync(saves, revisionGuards, cancellationToken);

    return new FamilyCommandResult(changed);
  }

  private static ShopRewardPresentation AddShopReward(
    FamilyEconomy economy,
    FamilyMemberId actorId,
    JsonObject payload,
    DateTimeOffset now
  )
  {
    var id = RewardId(payload);
    var icon = RequiredString(payload, "icon").Trim();
    var title = RequiredString(payload, "title").Trim();
    var description = RequiredString(payload, "description").Trim();
    var conditions = RequiredString(payload, "conditions").Trim();
    var price = RequiredDecimal(payload, "price");
    var quantity = RequiredInt(payload, "quantity");
    var from = OptionalCalendarDate(payload, "availableFrom");
    var until = OptionalCalendarDate(payload, "availableUntil");
    var catalogPolicy = new FamilyRewardCatalogPolicy();

    if (!catalogPolicy.IsValidNewReward(
      icon,
      title,
      description,
      conditions,
      price,
      quantity,
      from,
      until,
      from is not null,
      until is not null
    ))
    {
      throw new FamilyCommandException("The shop reward data is invalid.");
    }

    economy.AddShopReward(
      actorId,
      id,
      new ShopRewardDefinition(
        title,
        ParseShopRewardCategory(RequiredString(payload, "category")),
        new LadirchenAmount(price),
        quantity,
        BoolValueOrDefault(payload, "isVisible", true),
        new AvailabilityWindow(from, until)
      ),
      now
    );

    return new ShopRewardPresentation(id, description, icon, conditions);
  }

  private static void DecideShopReward(
    FamilyEconomy economy,
    FamilyMemberId actorId,
    JsonObject payload,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    var requestId = PendingRequestId(economy, RewardId(payload));

    if (RequiredBool(payload, "approved"))
    {
      economy.ApproveShopReward(actorId, requestId, operationId, now);
    }
    else
    {
      economy.RejectShopReward(actorId, requestId, operationId, now);
    }
  }

  private static RedemptionRequestId PendingRequestId(
    FamilyEconomy economy,
    ShopRewardId rewardId
  )
  {
    return economy.RedemptionRequests
      .SingleOrDefault(item =>
        item.RewardId == rewardId && item.Status == RedemptionStatus.Pending
      )
      ?.Id
      ?? throw new FamilyCommandException("A pending reward request is required.");
  }

  private static ShopRewardId RewardId(JsonObject payload)
  {
    return new ShopRewardId(RequiredString(payload, "id"));
  }

  private static ShopRewardCategory ParseShopRewardCategory(string value)
  {
    return value.Trim().ToLowerInvariant() switch
    {
      "time" => ShopRewardCategory.Time,
      "activity" => ShopRewardCategory.Activity,
      "allowance" => ShopRewardCategory.Allowance,
      "gift" => ShopRewardCategory.Gift,
      "privilege" => ShopRewardCategory.Privilege,
      "custom" => ShopRewardCategory.Custom,
      _ => throw new FamilyCommandException("The shop reward category is invalid."),
    };
  }

  private static DateOnly? OptionalCalendarDate(JsonObject source, string property)
  {
    var value = StringValue(source, property);

    if (string.IsNullOrWhiteSpace(value))
    {
      return null;
    }

    if (!DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
    {
      throw new FamilyCommandException("The shop date is invalid.");
    }

    return result;
  }

  private static bool RequiresWalletState(string commandType)
  {
    return commandType is "reward-shop.delete"
      or "reward-shop.request"
      or "reward-shop.cancel"
      or "reward-shop.decide";
  }
}