using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal static partial class FamilyEconomySnapshotMapper
{
  public static FamilyEconomy RehydrateContributions(
    Family family,
    JsonObject contributionState,
    JsonObject? savingsState = null
  )
  {
    ArgumentNullException.ThrowIfNull(family);
    ArgumentNullException.ThrowIfNull(contributionState);
    var economy = new FamilyEconomy(family);
    economy.RestoreContributionSettings(
      new ContributionSettings(
        new Percentage(
          OptionalDecimal(contributionState, "perfectRatingBonusPercent")
            ?? ContributionSettings.Default.PerfectRatingBonus.Value
        )
      )
    );
    RestoreWallets(economy, savingsState);
    RestoreEconomyHistory(economy, savingsState);

    foreach (var item in RequiredArray(contributionState, "contributions").OfType<JsonObject>())
    {
      var assignee = OptionalString(item, "assigneeId");
      var partners = OptionalArray(item, "invitedChildIds")
        .Select(RequiredString)
        .Select(value => new FamilyMemberId(value))
        .ToArray();
      var status = ParseContributionStatus(RequiredString(item, "status"));
      var rating = OptionalInt(item, "stars") is { } stars ? new StarRating(stars) : null;
      ContributionRewardBreakdown? earnedReward = null;

      if (status == ContributionStatus.Approved)
      {
        earnedReward = new ContributionRewardBreakdown(
          RequiredDecimal(item, "earnedReward"),
          OptionalDecimal(item, "earnedRatingBonus") ?? 0,
          OptionalDecimal(item, "earnedPromotionMultiplier") ?? 1
        );
      }

      economy.RestoreContribution(
        new ContributionId(RequiredString(item, "id")),
        new ContributionDefinition(
          RequiredString(item, "title"),
          ParseContributionKind(RequiredString(item, "kind")),
          new LadirchenAmount(RequiredDecimal(item, "reward")),
          new Percentage(RequiredDecimal(item, "energy")),
          assignee is null ? null : new FamilyMemberId(assignee)
        ),
        status,
        rating,
        earnedReward,
        OptionalInstant(item, "approvedAt"),
        partners
      );
    }

    foreach (var node in OptionalArray(contributionState, "usedContributionIds"))
    {
      economy.RestoreUsedContributionId(new ContributionId(RequiredString(node)));
    }

    foreach (var item in RequiredArray(contributionState, "promotions").OfType<JsonObject>())
    {
      if (!PromotionDeadline.TryParse(RequiredString(item, "deadline"), out var deadline))
      {
        throw new FamilyCommandException("The stored promotion deadline is invalid.");
      }

      economy.RestorePromotion(
        new PromotionId(RequiredString(item, "id")),
        new ContributionId(RequiredString(item, "contributionId")),
        new RewardMultiplier(RequiredDecimal(item, "multiplier")),
        new LadirchenAmount(RequiredDecimal(item, "teamworkBonus")),
        deadline!,
        OptionalBool(item, "active", true)
      );
    }

    return economy;
  }

  public static JsonObject WriteContributions(
    JsonObject original,
    FamilyEconomy economy,
    ContributionPresentation? addedContribution = null
  )
  {
    ArgumentNullException.ThrowIfNull(original);
    ArgumentNullException.ThrowIfNull(economy);
    var result = original.DeepClone().AsObject();
    result["perfectRatingBonusPercent"] = economy.ContributionSettings.PerfectRatingBonus.Value;
    var storedContributions = RequiredArray(result, "contributions");
    var byId = storedContributions
      .OfType<JsonObject>()
      .ToDictionary(item => RequiredString(item, "id"), StringComparer.Ordinal);
    var liveIds = economy.Contributions.Select(item => item.Id.Value).ToHashSet(StringComparer.Ordinal);

    foreach (var removed in byId.Where(item => !liveIds.Contains(item.Key)).Select(item => item.Value).ToArray())
    {
      storedContributions.Remove(removed);
    }

    foreach (var contribution in economy.Contributions)
    {
      if (!byId.TryGetValue(contribution.Id.Value, out var item))
      {
        if (addedContribution is null || addedContribution.Id != contribution.Id)
        {
          throw new FamilyCommandException("Presentation data for the new contribution is missing.");
        }

        item = CreateContributionItem(contribution, addedContribution);
        storedContributions.Add(item);
      }

      WriteContribution(item, contribution);
    }

    WritePromotions(result, economy);
    result["usedContributionIds"] = IdentifierArray(economy.UsedContributionIds);

    return result;
  }

  public static JsonObject WriteWalletBalances(JsonObject original, FamilyEconomy economy)
  {
    ArgumentNullException.ThrowIfNull(original);
    ArgumentNullException.ThrowIfNull(economy);
    var result = original.DeepClone().AsObject();
    var balances = RequiredObject(result, "balances");

    foreach (var wallet in economy.Wallets)
    {
      balances[wallet.Id.Value] = wallet.Balance;
    }

    WriteEconomyHistory(result, economy);

    return result;
  }

  public static void MarkContributionRewardCelebrated(JsonObject state, ContributionId contributionId)
  {
    ArgumentNullException.ThrowIfNull(state);
    ArgumentNullException.ThrowIfNull(contributionId);
    var contribution = RequiredArray(state, "contributions")
      .OfType<JsonObject>()
      .SingleOrDefault(item => RequiredString(item, "id") == contributionId.Value)
      ?? throw new FamilyCommandException("The contribution presentation does not exist.");
    contribution["rewardCelebrated"] = true;
  }

  private static void RestoreWallets(FamilyEconomy economy, JsonObject? savingsState)
  {
    if (savingsState is null)
    {
      return;
    }

    var balances = RequiredObject(savingsState, "balances");

    foreach (var wallet in economy.Wallets)
    {
      economy.RestoreWallet(wallet.Id, OptionalDecimal(balances, wallet.Id.Value) ?? 0, 0);
    }
  }

  private static JsonObject CreateContributionItem(
    Contribution contribution,
    ContributionPresentation presentation
  )
  {
    var kind = contribution.Kind == ContributionKind.Basic ? "basic" : "extra";

    return new JsonObject
    {
      ["id"] = contribution.Id.Value,
      ["title"] = contribution.Title,
      ["description"] = presentation.Description,
      ["icon"] = presentation.Icon,
      ["area"] = string.Empty,
      ["areaKey"] = "contributions.dynamic.familyArea",
      ["kind"] = kind,
      ["status"] = "available",
      ["reward"] = contribution.Reward.Value,
      ["energy"] = contribution.Energy.Value,
      ["dueLabel"] = string.Empty,
      ["dueLabelKey"] = kind == "basic" ? "contributions.dynamic.daily" : "contributions.dynamic.optional",
      ["worldEffect"] = kind == "basic" ? "sparkle" : null,
    };
  }

  private static void WriteContribution(JsonObject target, Contribution contribution)
  {
    target["title"] = contribution.Title;
    target["kind"] = contribution.Kind == ContributionKind.Basic ? "basic" : "extra";
    target["reward"] = contribution.Reward.Value;
    target["energy"] = contribution.Energy.Value;
    target["status"] = contribution.Status switch
    {
      ContributionStatus.Available => "available",
      ContributionStatus.Pending => "pending",
      ContributionStatus.Approved => "approved",
      _ => throw new ArgumentOutOfRangeException(nameof(contribution)),
    };
    SetOptional(target, "assigneeId", contribution.AssigneeId?.Value);
    target["invitedChildIds"] = new JsonArray(
      contribution.InvitedPartners.Select(item => JsonValue.Create(item.Value)).ToArray<JsonNode?>()
    );

    if (contribution.Rating is null || contribution.EarnedReward is null || contribution.ApprovedAt is null)
    {
      target.Remove("stars");
      target.Remove("earnedReward");
      target.Remove("earnedRatingBonus");
      target.Remove("earnedPromotionMultiplier");
      target.Remove("approvedAt");

      return;
    }

    target["stars"] = contribution.Rating.Value;
    target["earnedReward"] = contribution.EarnedReward.BaseReward;
    target["earnedRatingBonus"] = contribution.EarnedReward.RatingBonus;
    target["earnedPromotionMultiplier"] = contribution.EarnedReward.Multiplier;
    target["approvedAt"] = contribution.ApprovedAt.Value.ToString("O", CultureInfo.InvariantCulture);
    if (!target.ContainsKey("rewardCelebrated"))
    {
      target["rewardCelebrated"] = false;
    }
  }

  private static void WritePromotions(JsonObject state, FamilyEconomy economy)
  {
    var storedPromotions = RequiredArray(state, "promotions");
    var byId = storedPromotions
      .OfType<JsonObject>()
      .ToDictionary(item => RequiredString(item, "id"), StringComparer.Ordinal);
    var liveIds = economy.Promotions.Select(item => item.Id.Value).ToHashSet(StringComparer.Ordinal);

    foreach (var removed in byId.Where(item => !liveIds.Contains(item.Key)).Select(item => item.Value).ToArray())
    {
      storedPromotions.Remove(removed);
    }

    foreach (var promotion in economy.Promotions)
    {
      if (!byId.TryGetValue(promotion.Id.Value, out var item))
      {
        item = new JsonObject
        {
          ["id"] = promotion.Id.Value,
          ["title"] = string.Empty,
          ["titleKey"] = "contributions.promotions.multiplierTitle",
        };
        storedPromotions.Add(item);
      }

      item["contributionId"] = promotion.ContributionId.Value;
      item["multiplier"] = promotion.Multiplier.Value;
      item["deadline"] = promotion.Deadline.ToString();
      item["teamworkBonus"] = promotion.TeamworkBonus.Value;
      item["active"] = promotion.Active;
    }
  }

  private static ContributionKind ParseContributionKind(string value)
  {
    return value switch
    {
      "basic" => ContributionKind.Basic,
      "extra" => ContributionKind.Extra,
      _ => throw new FamilyCommandException("The stored contribution kind is invalid."),
    };
  }

  private static ContributionStatus ParseContributionStatus(string value)
  {
    return value switch
    {
      "available" => ContributionStatus.Available,
      "pending" => ContributionStatus.Pending,
      "approved" => ContributionStatus.Approved,
      _ => throw new FamilyCommandException("The stored contribution status is invalid."),
    };
  }

  private static JsonObject RequiredObject(JsonObject parent, string property)
  {
    return parent[property] as JsonObject
      ?? throw new FamilyCommandException($"The stored {property} object is missing.");
  }

  private static JsonArray RequiredArray(JsonObject parent, string property)
  {
    return parent[property] as JsonArray
      ?? throw new FamilyCommandException($"The stored {property} collection is missing.");
  }

  private static JsonArray OptionalArray(JsonObject parent, string property)
  {
    return parent[property] as JsonArray ?? new JsonArray();
  }

  private static string RequiredString(JsonNode? node)
  {
    var value = node?.GetValue<string>();

    if (string.IsNullOrWhiteSpace(value))
    {
      throw new FamilyCommandException("The stored identifier is missing.");
    }

    return value;
  }

  private static string RequiredString(JsonObject parent, string property)
  {
    var value = OptionalString(parent, property);

    if (string.IsNullOrWhiteSpace(value))
    {
      throw new FamilyCommandException($"The stored {property} value is missing.");
    }

    return value;
  }

  private static string? OptionalString(JsonObject parent, string property)
  {
    return parent[property]?.GetValue<string>();
  }

  private static decimal RequiredDecimal(JsonObject parent, string property)
  {
    return OptionalDecimal(parent, property)
      ?? throw new FamilyCommandException($"The stored {property} value is missing.");
  }

  private static decimal? OptionalDecimal(JsonObject parent, string property)
  {
    return parent[property]?.GetValue<decimal>();
  }

  private static int? OptionalInt(JsonObject parent, string property)
  {
    if (parent[property] is null)
    {
      return null;
    }

    if (parent[property] is JsonValue value && value.TryGetValue<int>(out var integer))
    {
      return integer;
    }

    throw new FamilyCommandException($"The stored {property} value must be an integer.");
  }

  private static bool OptionalBool(JsonObject parent, string property, bool fallback)
  {
    return parent[property]?.GetValue<bool>() ?? fallback;
  }

  private static DateTimeOffset? OptionalInstant(JsonObject parent, string property)
  {
    var value = OptionalString(parent, property);

    if (value is null)
    {
      return null;
    }

    if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
    {
      throw new FamilyCommandException($"The stored {property} value is invalid.");
    }

    return result;
  }

  private static void SetOptional(JsonObject target, string property, string? value)
  {
    if (value is null)
    {
      target.Remove(property);
    }
    else
    {
      target[property] = value;
    }
  }
}
