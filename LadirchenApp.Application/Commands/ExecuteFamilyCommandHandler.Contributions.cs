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
  private async Task<FamilyCommandResult> ExecuteContributionsAsync(
    FamilyCommand command,
    JsonObject profileState,
    JsonArray members,
    JsonObject actor,
    JsonObject payload,
    CancellationToken cancellationToken
  )
  {
    var contributions = await RequireSnapshotAsync(command.FamilyId, "contributions", cancellationToken);
    var contributionState = RequireObject(contributions.State);
    var contributionItems = RequireArray(contributionState, "contributions");
    var promotionItems = RequireArray(contributionState, "promotions");
    JsonObject? savingsState = null;
    FamilyAggregateSnapshot? savings = null;
    var now = _timeProvider.GetUtcNow();

    switch (command.CommandType)
    {
      case "contributions.set-perfect-rating-bonus":
        RequireAdministrator(actor);
        var perfectRatingBonus = RequiredDecimal(payload, "percent");
        Require(perfectRatingBonus >= 0 && perfectRatingBonus <= 100, "The five-star bonus must be between zero and 100 percent.");
        contributionState["perfectRatingBonusPercent"] = Round(perfectRatingBonus);
        break;
      case "contributions.submit":
        Submit(contributionItems, actor, payload);
        break;
      case "contributions.claim":
        Claim(contributionItems, actor, payload);
        break;
      case "contributions.assign":
        Assign(contributionItems, members, actor, payload);
        break;
      case "contributions.approve":
        savings = await RequireSnapshotAsync(command.FamilyId, "savings", cancellationToken);
        savingsState = RequireObject(savings.State);
        Approve(
          profileState,
          contributionItems,
          promotionItems,
          members,
          actor,
          savingsState,
          payload,
          new Percentage(
            DecimalValue(contributionState, "perfectRatingBonusPercent")
              ?? ContributionRewardPolicy.DefaultPerfectRatingBonus.Value
          ),
          now
        );
        break;
      case "contributions.return":
        ReturnContribution(contributionItems, members, actor, payload);
        break;
      case "contributions.add":
        AddContribution(contributionItems, members, actor, payload);
        break;
      case "contributions.delete":
        DeleteContribution(contributionItems, promotionItems, actor, payload);
        break;
      case "contributions.delete-promotion":
        DeletePromotion(promotionItems, actor, payload);
        break;
      case "contributions.set-partners":
        SetPartners(contributionItems, members, actor, payload);
        break;
      case "contributions.add-promotion":
        AddPromotion(contributionItems, promotionItems, actor, payload);
        break;
      case "contributions.acknowledge-reward":
        AcknowledgeReward(contributionItems, actor, payload);
        break;
      default:
        throw new FamilyCommandException("The family command is not supported.");
    }

    var pendingSaves = new List<PendingFamilyAggregateSave>
    {
      CreateSave(contributions, contributionState, command.ActorId),
    };

    if (savings is not null && savingsState is not null)
    {
      pendingSaves.Add(CreateSave(savings, savingsState, command.ActorId));
    }

    pendingSaves.Add(
      await CreateProgressionSaveAsync(
        command.FamilyId,
        profileState,
        contributionState,
        command.ActorId,
        cancellationToken
      )
    );

    var changed = await _repository.SaveBatchAsync(pendingSaves, cancellationToken);

    foreach (var snapshot in changed)
    {
      await _publisher.PublishAsync(snapshot, cancellationToken);
    }

    return new FamilyCommandResult(changed);

  }

  private static void Submit(JsonArray contributions, JsonObject actor, JsonObject payload)
  {
    var contribution = RequireContribution(contributions, RequiredString(payload, "id"));
    RequireStatus(contribution, "available");
    Require(
      StringValue(contribution, "assigneeId") == StringValue(actor, "id"),
      "Only the assignee may submit a contribution."
    );
    contribution["status"] = "pending";
  }

  private static void Claim(JsonArray contributions, JsonObject actor, JsonObject payload)
  {
    Require(StringValue(actor, "role") == "child", "Only a child may claim a contribution.");
    var contribution = RequireContribution(contributions, RequiredString(payload, "id"));
    RequireStatus(contribution, "available");
    Require(string.IsNullOrWhiteSpace(StringValue(contribution, "assigneeId")), "The contribution is already assigned.");
    contribution["assigneeId"] = RequiredString(actor, "id");
  }

  private static void Assign(JsonArray contributions, JsonArray members, JsonObject actor, JsonObject payload)
  {
    RequireAdministrator(actor);
    var contribution = RequireContribution(contributions, RequiredString(payload, "id"));
    RequireStatus(contribution, "available");
    var memberId = StringValue(payload, "memberId");

    if (!string.IsNullOrWhiteSpace(memberId))
    {
      _ = RequireMember(members, memberId);
      contribution["assigneeId"] = memberId;
    }
    else
    {
      contribution.Remove("assigneeId");
    }

    contribution["invitedChildIds"] = new JsonArray();
  }

  private static void Approve(
    JsonObject profile,
    JsonArray contributions,
    JsonArray promotions,
    JsonArray members,
    JsonObject actor,
    JsonObject savings,
    JsonObject payload,
    Percentage perfectRatingBonus,
    DateTimeOffset now
  )
  {
    var contribution = RequireContribution(contributions, RequiredString(payload, "id"));
    RequireStatus(contribution, "pending");
    var assigneeId = RequiredString(contribution, "assigneeId");
    var assignee = RequireMember(members, assigneeId);
    var actorIsAdministrator = IsAdministrator(actor);
    var actorCanReviewGuardian = StringValue(actor, "role") == "child" && StringValue(assignee, "role") == "guardian";
    Require(
      actorIsAdministrator && StringValue(assignee, "role") == "child" || actorCanReviewGuardian,
      "The member may not review this contribution."
    );
    var stars = RequiredInt(payload, "stars");
    var rating = new StarRating(stars);
    var promotion = FindActivePromotion(promotions, RequiredString(contribution, "id"), profile, now);
    var multiplier = promotion is null ? null : new RewardMultiplier(RequiredDecimal(promotion, "multiplier"));
    var teamworkBonus = promotion is null ? null : new LadirchenAmount(RequiredDecimal(promotion, "teamworkBonus"));
    var reward = ContributionRewardPolicy.Calculate(
      new LadirchenAmount(RequiredDecimal(contribution, "reward")),
      rating,
      perfectRatingBonus,
      multiplier,
      teamworkBonus
    );
    var balances = RequireObject(savings, "balances");
    var currentBalance = DecimalValue(balances, assigneeId) ?? 0;
    balances[assigneeId] = JsonValue.Create(currentBalance + reward.Total);
    contribution["status"] = "approved";
    contribution["stars"] = stars;
    contribution["earnedReward"] = JsonValue.Create(reward.BaseReward);
    contribution["earnedRatingBonus"] = JsonValue.Create(reward.RatingBonus);
    contribution["earnedPromotionMultiplier"] = JsonValue.Create(reward.Multiplier);
    contribution["approvedAt"] = now.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
    contribution["rewardCelebrated"] = false;
  }

  private static void ReturnContribution(
    JsonArray contributions,
    JsonArray members,
    JsonObject actor,
    JsonObject payload
  )
  {
    var contribution = RequireContribution(contributions, RequiredString(payload, "id"));
    RequireStatus(contribution, "pending");
    var assignee = RequireMember(members, RequiredString(contribution, "assigneeId"));
    Require(
      IsAdministrator(actor) && StringValue(assignee, "role") == "child"
        || StringValue(actor, "role") == "child" && StringValue(assignee, "role") == "guardian",
      "The member may not review this contribution."
    );
    contribution["status"] = "available";
  }

  private static void AddContribution(
    JsonArray contributions,
    JsonArray members,
    JsonObject actor,
    JsonObject payload
  )
  {
    var assigneeId = StringValue(payload, "assigneeId");
    Require(
      IsAdministrator(actor)
        || StringValue(actor, "role") == "guardian" && assigneeId == StringValue(actor, "id"),
      "The member may not create this contribution."
    );

    if (!string.IsNullOrWhiteSpace(assigneeId))
    {
      _ = RequireMember(members, assigneeId);
    }

    var id = RequiredString(payload, "id");
    Require(FindById(contributions, id) is null, "The contribution already exists.");
    var kind = RequiredString(payload, "kind");
    Require(kind is "basic" or "extra", "The contribution kind is invalid.");
    var reward = new LadirchenAmount(RequiredDecimal(payload, "reward"));
    var energy = new Percentage(RequiredDecimal(payload, "energy"));
    var created = new JsonObject
    {
      ["id"] = id,
      ["title"] = RequiredString(payload, "title"),
      ["description"] = RequiredString(payload, "description"),
      ["icon"] = RequiredString(payload, "icon"),
      ["area"] = string.Empty,
      ["areaKey"] = "contributions.dynamic.familyArea",
      ["kind"] = kind,
      ["status"] = "available",
      ["reward"] = reward.Value,
      ["energy"] = energy.Value,
      ["dueLabel"] = string.Empty,
      ["dueLabelKey"] = kind == "basic" ? "contributions.dynamic.daily" : "contributions.dynamic.optional",
    };

    if (!string.IsNullOrWhiteSpace(assigneeId))
    {
      created["assigneeId"] = assigneeId;
    }

    if (kind == "basic")
    {
      created["worldEffect"] = "sparkle";
    }

    contributions.Add(created);
  }

  private static void DeleteContribution(
    JsonArray contributions,
    JsonArray promotions,
    JsonObject actor,
    JsonObject payload
  )
  {
    RequireAdministrator(actor);
    var id = RequiredString(payload, "id");
    var contribution = RequireContribution(contributions, id);
    contributions.Remove(contribution);

    foreach (var promotion in promotions.OfType<JsonObject>().Where(item => StringValue(item, "contributionId") == id).ToArray())
    {
      promotions.Remove(promotion);
    }
  }

  private static void DeletePromotion(JsonArray promotions, JsonObject actor, JsonObject payload)
  {
    RequireAdministrator(actor);
    var promotion = FindById(promotions, RequiredString(payload, "id"))
      ?? throw new FamilyCommandException("The promotion does not exist.");
    promotions.Remove(promotion);
  }

  private static void SetPartners(
    JsonArray contributions,
    JsonArray members,
    JsonObject actor,
    JsonObject payload
  )
  {
    var contribution = RequireContribution(contributions, RequiredString(payload, "id"));
    RequireStatus(contribution, "available");
    Require(StringValue(contribution, "kind") == "extra", "Only extra contributions have partners.");
    Require(
      IsAdministrator(actor) || StringValue(contribution, "assigneeId") == StringValue(actor, "id"),
      "The member may not change contribution partners."
    );
    var partnerIds = RequireArray(payload, "partnerIds").Select(RequiredString).Distinct(StringComparer.Ordinal).ToArray();
    var assigneeId = StringValue(contribution, "assigneeId");
    var result = new JsonArray();

    foreach (var partnerId in partnerIds)
    {
      var partner = RequireMember(members, partnerId);
      Require(StringValue(partner, "role") == "child", "Only children may be contribution partners.");
      Require(partnerId != assigneeId, "The assignee cannot also be a partner.");
      result.Add(partnerId);
    }

    contribution["invitedChildIds"] = result;
  }

  private static void AddPromotion(
    JsonArray contributions,
    JsonArray promotions,
    JsonObject actor,
    JsonObject payload
  )
  {
    RequireAdministrator(actor);
    var contributionId = RequiredString(payload, "contributionId");
    _ = RequireContribution(contributions, contributionId);
    var id = RequiredString(payload, "id");
    Require(FindById(promotions, id) is null, "The promotion already exists.");
    var multiplier = RequiredDecimal(payload, "multiplier");
    Require(multiplier is 2 or 3, "A promotion supports only two-times or three-times rewards.");
    Require(
      PromotionDeadline.TryParse(RequiredString(payload, "deadline"), out var deadline),
      "The promotion deadline must use HH:mm."
    );
    var teamworkBonus = new LadirchenAmount(RequiredDecimal(payload, "teamworkBonus"));
    promotions.Add(
      new JsonObject
      {
        ["id"] = id,
        ["contributionId"] = contributionId,
        ["title"] = string.Empty,
        ["titleKey"] = "contributions.promotions.multiplierTitle",
        ["multiplier"] = multiplier,
        ["deadline"] = deadline!.ToString(),
        ["teamworkBonus"] = teamworkBonus.Value,
        ["active"] = true,
      }
    );
  }

  private static void AcknowledgeReward(JsonArray contributions, JsonObject actor, JsonObject payload)
  {
    var contribution = RequireContribution(contributions, RequiredString(payload, "id"));
    Require(StringValue(contribution, "assigneeId") == StringValue(actor, "id"), "Only the recipient may acknowledge a reward.");
    RequireStatus(contribution, "approved");
    contribution["rewardCelebrated"] = true;
  }

  private static JsonObject? FindActivePromotion(
    JsonArray promotions,
    string contributionId,
    JsonObject profile,
    DateTimeOffset now
  )
  {
    var zone = new IanaTimeZone(RequiredString(profile, "timeZone"));

    return promotions
      .OfType<JsonObject>()
      .FirstOrDefault(item =>
        StringValue(item, "contributionId") == contributionId
        && BoolValue(item, "active")
        && PromotionDeadline.TryParse(StringValue(item, "deadline"), out var deadline)
        && deadline!.IsOpen(now, zone)
      );
  }

  private static JsonObject RequireContribution(JsonArray contributions, string id)
  {
    return FindById(contributions, id) ?? throw new FamilyCommandException("The contribution does not exist.");
  }

}
