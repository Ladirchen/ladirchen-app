using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal static partial class FamilyEconomySnapshotMapper
{
  public static FamilyEconomy RehydrateRewardShop(
    Family family,
    JsonObject rewardShopState,
    JsonObject? savingsState
  )
  {
    ArgumentNullException.ThrowIfNull(family);
    ArgumentNullException.ThrowIfNull(rewardShopState);
    var economy = new FamilyEconomy(family);

    foreach (var item in RequiredArray(rewardShopState, "rewards").OfType<JsonObject>())
    {
      var rewardId = new ShopRewardId(RequiredString(item, "id"));
      var status = ParseShopRewardStatus(RequiredString(item, "status"));
      var definition = new ShopRewardDefinition(
        RequiredString(item, "title"),
        ParseShopRewardCategory(RequiredString(item, "category")),
        new LadirchenAmount(RequiredDecimal(item, "price")),
        RequiredInt(item, "quantity"),
        OptionalBool(item, "isVisible", true),
        new AvailabilityWindow(
          OptionalCalendarDate(item, "availableFrom"),
          OptionalCalendarDate(item, "availableUntil")
        )
      );
      economy.RestoreShopReward(rewardId, definition, status);

      if (status == ShopRewardStatus.Requested)
      {
        var requestId = OptionalString(item, "requestId") is { } storedRequestId
          ? new RedemptionRequestId(storedRequestId)
          : CreateLegacyRequestId(rewardId);
        economy.RestorePendingShopRewardRequest(
          requestId,
          rewardId,
          new FamilyMemberId(RequiredString(item, "requesterId")),
          definition.Price,
          OptionalInstant(item, "requestedAt") ?? DateTimeOffset.UnixEpoch
        );
      }
    }

    RestoreShopWallets(economy, savingsState);
    RestoreEconomyHistory(economy, savingsState);

    return economy;
  }

  public static JsonObject WriteRewardShop(
    JsonObject original,
    FamilyEconomy economy,
    ShopRewardPresentation? addedReward = null
  )
  {
    ArgumentNullException.ThrowIfNull(original);
    ArgumentNullException.ThrowIfNull(economy);
    var result = original.DeepClone().AsObject();
    var storedRewards = RequiredArray(result, "rewards");
    var byId = storedRewards
      .OfType<JsonObject>()
      .ToDictionary(item => RequiredString(item, "id"), StringComparer.Ordinal);
    var liveIds = economy.ShopRewards.Select(item => item.Id.Value).ToHashSet(StringComparer.Ordinal);

    foreach (var removed in byId.Where(item => !liveIds.Contains(item.Key)).Select(item => item.Value).ToArray())
    {
      storedRewards.Remove(removed);
    }

    var pendingRequests = economy.RedemptionRequests
      .Where(item => item.Status == RedemptionStatus.Pending)
      .ToDictionary(item => item.RewardId);

    foreach (var reward in economy.ShopRewards)
    {
      if (!byId.TryGetValue(reward.Id.Value, out var item))
      {
        if (addedReward is null || addedReward.Id != reward.Id)
        {
          throw new FamilyCommandException("Presentation data for the new shop reward is missing.");
        }

        item = CreateRewardItem(reward, addedReward);
        storedRewards.Insert(0, item);
      }

      pendingRequests.TryGetValue(reward.Id, out var request);
      WriteReward(item, reward, request);
    }

    return result;
  }

  private static void RestoreShopWallets(FamilyEconomy economy, JsonObject? savingsState)
  {
    if (savingsState is null)
    {
      return;
    }

    var balances = RequiredObject(savingsState, "balances");
    var reservedByMember = economy.RedemptionRequests
      .Where(item => item.Status == RedemptionStatus.Pending)
      .GroupBy(item => item.RequesterId)
      .ToDictionary(
        group => group.Key,
        group => group.Sum(item => item.Price.Value)
      );

    foreach (var wallet in economy.Wallets)
    {
      economy.RestoreWallet(
        wallet.Id,
        OptionalDecimal(balances, wallet.Id.Value) ?? 0,
        reservedByMember.GetValueOrDefault(wallet.Id)
      );
    }
  }

  private static JsonObject CreateRewardItem(
    ShopReward reward,
    ShopRewardPresentation presentation
  )
  {
    return new JsonObject
    {
      ["id"] = reward.Id.Value,
      ["description"] = presentation.Description,
      ["icon"] = presentation.Icon,
      ["conditions"] = presentation.Conditions,
    };
  }

  private static void WriteReward(
    JsonObject target,
    ShopReward reward,
    RedemptionRequest? request
  )
  {
    target["id"] = reward.Id.Value;
    target["title"] = reward.Title;
    target["price"] = reward.Price.Value;
    target["category"] = FormatShopRewardCategory(reward.Category);
    target["quantity"] = reward.Quantity;
    target["isVisible"] = reward.IsVisible;
    target["status"] = FormatShopRewardStatus(reward.Status);
    SetOptional(
      target,
      "availableFrom",
      reward.Window.From?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
    );
    SetOptional(
      target,
      "availableUntil",
      reward.Window.Until?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
    );

    if (request is null)
    {
      target.Remove("requestId");
      target.Remove("requesterId");
      target.Remove("requestedAt");

      return;
    }

    target["requestId"] = request.Id.Value;
    target["requesterId"] = request.RequesterId.Value;
    target["requestedAt"] = request.RequestedAt.ToString("O", CultureInfo.InvariantCulture);
  }

  private static RedemptionRequestId CreateLegacyRequestId(ShopRewardId rewardId)
  {
    var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rewardId.Value));

    return new RedemptionRequestId($"legacy:{Convert.ToHexString(hash)}");
  }

  private static DateOnly? OptionalCalendarDate(JsonObject item, string property)
  {
    var value = OptionalString(item, property);

    if (value is null)
    {
      return null;
    }

    if (!DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
    {
      throw new FamilyCommandException($"The stored {property} value is invalid.");
    }

    return result;
  }

  private static int RequiredInt(JsonObject item, string property)
  {
    return item[property]?.GetValue<int>()
      ?? throw new FamilyCommandException($"The stored {property} value is missing.");
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
      _ => throw new FamilyCommandException("The stored shop reward category is invalid."),
    };
  }

  private static string FormatShopRewardCategory(ShopRewardCategory category)
  {
    return category switch
    {
      ShopRewardCategory.Time => "time",
      ShopRewardCategory.Activity => "activity",
      ShopRewardCategory.Allowance => "allowance",
      ShopRewardCategory.Gift => "gift",
      ShopRewardCategory.Privilege => "privilege",
      ShopRewardCategory.Custom => "custom",
      _ => throw new ArgumentOutOfRangeException(nameof(category)),
    };
  }

  private static ShopRewardStatus ParseShopRewardStatus(string value)
  {
    return value switch
    {
      "available" => ShopRewardStatus.Available,
      "requested" => ShopRewardStatus.Requested,
      "redeemed" => ShopRewardStatus.Redeemed,
      _ => throw new FamilyCommandException("The stored shop reward status is invalid."),
    };
  }

  private static string FormatShopRewardStatus(ShopRewardStatus status)
  {
    return status switch
    {
      ShopRewardStatus.Available => "available",
      ShopRewardStatus.Requested => "requested",
      ShopRewardStatus.Redeemed => "redeemed",
      _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };
  }
}
