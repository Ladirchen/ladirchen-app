using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Avatar;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

public sealed partial class ExecuteFamilyCommandHandler
{
  private async Task<FamilyCommandResult> ExecuteRewardShopAsync(
    FamilyCommand command,
    JsonObject profile,
    JsonArray members,
    JsonObject actor,
    JsonObject payload,
    CancellationToken cancellationToken
  )
  {
    var shop = await RequireSnapshotAsync(command.FamilyId, "reward-shop", cancellationToken);
    var shopState = RequireObject(shop.State);
    var rewards = RequireArray(shopState, "rewards");
    FamilyAggregateSnapshot? savings = null;
    JsonObject? savingsState = null;

    switch (command.CommandType)
    {
      case "reward-shop.add":
        AddShopReward(rewards, actor, payload);
        break;
      case "reward-shop.set-visibility":
        SetShopRewardVisibility(rewards, actor, payload);
        break;
      case "reward-shop.delete":
        DeleteShopReward(rewards, actor, payload);
        break;
      case "reward-shop.request":
        savings = await RequireSnapshotAsync(command.FamilyId, "savings", cancellationToken);
        savingsState = RequireObject(savings.State);
        RequestShopReward(profile, rewards, actor, savingsState, payload, _timeProvider.GetUtcNow());
        break;
      case "reward-shop.cancel":
        CancelShopReward(rewards, actor, payload);
        break;
      case "reward-shop.decide":
        savings = await RequireSnapshotAsync(command.FamilyId, "savings", cancellationToken);
        savingsState = RequireObject(savings.State);
        DecideShopReward(profile, rewards, members, actor, savingsState, payload, _timeProvider.GetUtcNow());
        break;
      default:
        throw new FamilyCommandException("The reward-shop command is not supported.");
    }

    var saves = new List<PendingFamilyAggregateSave> { CreateSave(shop, shopState, command.ActorId) };

    if (savings is not null && savingsState is not null)
    {
      saves.Add(CreateSave(savings, savingsState, command.ActorId));
    }

    var changed = await _repository.SaveBatchAsync(saves, cancellationToken);

    foreach (var snapshot in changed)
    {
      await _publisher.PublishAsync(snapshot, cancellationToken);
    }

    return new FamilyCommandResult(changed);
  }

  private static void AddShopReward(JsonArray rewards, JsonObject actor, JsonObject payload)
  {
    RequireAdministrator(actor);
    var id = RequiredString(payload, "id");
    Require(FindById(rewards, id) is null, "The shop reward already exists.");
    var quantity = RequiredInt(payload, "quantity");
    var price = RequiredDecimal(payload, "price");
    var icon = RequiredString(payload, "icon");
    var title = RequiredString(payload, "title");
    var description = RequiredString(payload, "description");
    var conditions = RequiredString(payload, "conditions");
    var category = RequiredString(payload, "category");
    Require(
      Enum.TryParse<ShopRewardCategory>(category, true, out _),
      "The shop reward category is invalid."
    );
    var availableFrom = ParseOptionalDate(payload, "availableFrom");
    var availableUntil = ParseOptionalDate(payload, "availableUntil");
    var catalogPolicy = new FamilyRewardCatalogPolicy();
    Require(
      catalogPolicy.IsValidNewReward(
        icon,
        title,
        description,
        conditions,
        price,
        quantity,
        availableFrom,
        availableUntil,
        availableFrom is not null,
        availableUntil is not null
      ),
      "The shop reward data is invalid."
    );
    var reward = new JsonObject
    {
      ["id"] = id,
      ["title"] = title,
      ["description"] = description,
      ["icon"] = icon,
      ["price"] = price,
      ["category"] = category,
      ["quantity"] = quantity,
      ["conditions"] = conditions,
      ["isVisible"] = BoolValueOrDefault(payload, "isVisible", true),
      ["status"] = quantity > 0 ? "available" : "redeemed",
    };
    CopyOptionalString(payload, reward, "availableFrom");
    CopyOptionalString(payload, reward, "availableUntil");
    rewards.Insert(0, reward);
  }

  private static void SetShopRewardVisibility(JsonArray rewards, JsonObject actor, JsonObject payload)
  {
    RequireAdministrator(actor);
    RequireShopReward(rewards, RequiredString(payload, "id"))["isVisible"] = RequiredBool(payload, "isVisible");
  }

  private static void DeleteShopReward(JsonArray rewards, JsonObject actor, JsonObject payload)
  {
    RequireAdministrator(actor);
    rewards.Remove(RequireShopReward(rewards, RequiredString(payload, "id")));
  }

  private static void RequestShopReward(
    JsonObject profile,
    JsonArray rewards,
    JsonObject actor,
    JsonObject savings,
    JsonObject payload,
    DateTimeOffset now
  )
  {
    Require(StringValue(actor, "role") == "child", "Only children may request a shop reward.");
    var reward = RequireShopReward(rewards, RequiredString(payload, "id"));
    Require(StringValue(reward, "status") == "available" && RequiredInt(reward, "quantity") > 0, "The reward is unavailable.");
    Require(
      ShopIsOpen(profile, now),
      $"Shop requests close at {ShopAvailabilityPolicy.ClosingHour:00}:00 local time."
    );
    Require(ShopRewardIsPublished(profile, reward, now), "The reward is not published today.");
    var actorId = RequiredString(actor, "id");
    RequireAvailableBalance(savings, rewards, actorId, RequiredDecimal(reward, "price"));
    reward["status"] = "requested";
    reward["requesterId"] = actorId;
  }

  private static void CancelShopReward(JsonArray rewards, JsonObject actor, JsonObject payload)
  {
    var reward = RequireShopReward(rewards, RequiredString(payload, "id"));
    Require(StringValue(reward, "status") == "requested", "A requested shop reward is required.");
    Require(StringValue(reward, "requesterId") == StringValue(actor, "id"), "Only the requester may cancel the reward.");
    reward["status"] = "available";
    reward.Remove("requesterId");
  }

  private static void DecideShopReward(
    JsonObject profile,
    JsonArray rewards,
    JsonArray members,
    JsonObject actor,
    JsonObject savings,
    JsonObject payload,
    DateTimeOffset now
  )
  {
    RequireAdministrator(actor);
    var reward = RequireShopReward(rewards, RequiredString(payload, "id"));
    Require(StringValue(reward, "status") == "requested", "A requested shop reward is required.");
    var approved = RequiredBool(payload, "approved");

    if (approved)
    {
      Require(
        ShopIsOpen(profile, now),
        $"Shop approvals close at {ShopAvailabilityPolicy.ClosingHour:00}:00 local time."
      );
      var requesterId = RequiredString(reward, "requesterId");
      _ = RequireMember(members, requesterId);
      var price = RequiredDecimal(reward, "price");
      DebitAvailableBalance(savings, rewards, requesterId, price, RequiredString(reward, "id"));
      var quantity = RequiredInt(reward, "quantity") - 1;
      Require(quantity >= 0, "The shop reward has no remaining stock.");
      reward["quantity"] = quantity;
      reward["status"] = quantity > 0 ? "available" : "redeemed";
    }
    else
    {
      reward["status"] = "available";
    }

    reward.Remove("requesterId");
  }

  private static JsonObject RequireShopReward(JsonArray rewards, string id)
  {
    return FindById(rewards, id) ?? throw new FamilyCommandException("The shop reward does not exist.");
  }

  private static bool ShopIsOpen(JsonObject profile, DateTimeOffset now)
  {
    return ShopAvailabilityPolicy.RedemptionIsOpen(
      now,
      new IanaTimeZone(RequiredString(profile, "timeZone"))
    );
  }

  private static bool ShopRewardIsPublished(JsonObject profile, JsonObject reward, DateTimeOffset now)
  {
    if (!BoolValueOrDefault(reward, "isVisible", true))
    {
      return false;
    }

    var current = DateOnly.FromDateTime(LocalDateTime(profile, now));
    var from = ParseOptionalDate(reward, "availableFrom");
    var until = ParseOptionalDate(reward, "availableUntil");

    return (from is null || current >= from) && (until is null || current <= until);
  }

  private static DateTime LocalDateTime(JsonObject profile, DateTimeOffset now)
  {
    var zone = TimeZoneInfo.FindSystemTimeZoneById(RequiredString(profile, "timeZone"));

    return TimeZoneInfo.ConvertTime(now, zone).DateTime;
  }

  private static DateOnly? ParseOptionalDate(JsonObject source, string property)
  {
    var value = StringValue(source, property);

    if (string.IsNullOrWhiteSpace(value))
    {
      return null;
    }

    Require(DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result), "The shop date is invalid.");

    return result;
  }
}
