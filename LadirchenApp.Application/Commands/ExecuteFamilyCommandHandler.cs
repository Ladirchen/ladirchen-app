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
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

public sealed class ExecuteFamilyCommandHandler
{
  private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> FamilyLocks = new();
  private readonly IFamilyAggregateSnapshotRepository _repository;
  private readonly IFamilyAggregateChangePublisher _publisher;
  private readonly TimeProvider _timeProvider;

  public ExecuteFamilyCommandHandler(
    IFamilyAggregateSnapshotRepository repository,
    IFamilyAggregateChangePublisher publisher,
    TimeProvider timeProvider
  )
  {
    _repository = repository;
    _publisher = publisher;
    _timeProvider = timeProvider;
  }

  public async Task<FamilyCommandResult> HandleAsync(FamilyCommand command, CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(command);
    var familyLock = FamilyLocks.GetOrAdd(command.FamilyId.Value, _ => new SemaphoreSlim(1, 1));
    await familyLock.WaitAsync(cancellationToken);

    try
    {
      return await ExecuteLockedAsync(command, cancellationToken);
    }
    finally
    {
      familyLock.Release();
    }
  }

  private async Task<FamilyCommandResult> ExecuteLockedAsync(
    FamilyCommand command,
    CancellationToken cancellationToken
  )
  {
    var profile = await RequireSnapshotAsync(command.FamilyId, "family-profile", cancellationToken);
    var profileState = RequireObject(profile.State);
    var members = RequireArray(profileState, "members");
    var payload = RequireObject(command.Payload);
    var actor = RequireMember(members, command.ActorId.Value.ToString());

    if (command.CommandType.StartsWith("savings.", StringComparison.Ordinal))
    {
      return await ExecuteSavingsAsync(command, members, actor, payload, cancellationToken);
    }

    if (command.CommandType.StartsWith("reward-shop.", StringComparison.Ordinal))
    {
      return await ExecuteRewardShopAsync(
        command,
        profileState,
        members,
        actor,
        payload,
        cancellationToken
      );
    }

    if (command.CommandType.StartsWith("progression.", StringComparison.Ordinal))
    {
      return await ExecuteProgressionAsync(command, profile, profileState, members, actor, cancellationToken);
    }

    if (!command.CommandType.StartsWith("contributions.", StringComparison.Ordinal))
    {
      throw new FamilyCommandException("The family command is not supported.");
    }

    var contributions = await RequireSnapshotAsync(command.FamilyId, "contributions", cancellationToken);
    var contributionState = RequireObject(contributions.State);
    var contributionItems = RequireArray(contributionState, "contributions");
    var promotionItems = RequireArray(contributionState, "promotions");
    JsonObject? savingsState = null;
    FamilyAggregateSnapshot? savings = null;
    var now = _timeProvider.GetUtcNow();

    switch (command.CommandType)
    {
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
        Approve(profileState, contributionItems, promotionItems, members, actor, savingsState, payload, now);
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

  private async Task<FamilyCommandResult> ExecuteProgressionAsync(
    FamilyCommand command,
    FamilyAggregateSnapshot profile,
    JsonObject profileState,
    JsonArray members,
    JsonObject actor,
    CancellationToken cancellationToken
  )
  {
    var contributions = await RequireSnapshotAsync(command.FamilyId, "contributions", cancellationToken);
    var contributionState = RequireObject(contributions.State);
    var progression = await RequireSnapshotAsync(command.FamilyId, "family-progression", cancellationToken);
    var progressionState = BuildProgressionState(profileState, contributionState, RequireObject(progression.State));
    var saves = new List<PendingFamilyAggregateSave>();

    switch (command.CommandType)
    {
      case "progression.refresh":
        break;
      case "progression.complete-week":
        RequireAdministrator(actor);
        Require(RequiredInt(progressionState, "familyEnergy") >= 60, "Family energy is too low to complete the week.");
        var completedStreak = RequiredInt(progressionState, "completedWeeklyStreak") + 1;
        progressionState["completedWeeklyStreak"] = completedStreak;
        progressionState["houseLevel"] = Math.Min(4, RequiredInt(progressionState, "houseLevel") + 1);
        SetParticipantStreaks(members, completedStreak);
        saves.Add(CreateSave(profile, profileState, command.ActorId));
        break;
      case "progression.fail-week":
        RequireAdministrator(actor);
        var familyEnergy = RequiredInt(progressionState, "familyEnergy");
        Require(familyEnergy < 60, "A successful family week cannot be closed as failed.");
        var currentStreak = RequiredInt(progressionState, "completedWeeklyStreak");
        var failedStreak = familyEnergy < 30 ? Math.Max(0, currentStreak - 1) : currentStreak;
        progressionState["completedWeeklyStreak"] = failedStreak;
        progressionState["houseLevel"] = Math.Max(0, RequiredInt(progressionState, "houseLevel") - 1);
        SetParticipantStreaks(members, failedStreak);
        saves.Add(CreateSave(profile, profileState, command.ActorId));
        break;
      default:
        throw new FamilyCommandException("The progression command is not supported.");
    }

    saves.Add(CreateProgressionSave(progression, progressionState, command.ActorId));
    var changed = await _repository.SaveBatchAsync(saves, cancellationToken);

    foreach (var snapshot in changed)
    {
      await _publisher.PublishAsync(snapshot, cancellationToken);
    }

    return new FamilyCommandResult(changed);
  }

  private async Task<PendingFamilyAggregateSave> CreateProgressionSaveAsync(
    FamilyId familyId,
    JsonObject profileState,
    JsonObject contributionState,
    FamilyMemberId actorId,
    CancellationToken cancellationToken
  )
  {
    var progression = await RequireSnapshotAsync(familyId, "family-progression", cancellationToken);
    var state = BuildProgressionState(profileState, contributionState, RequireObject(progression.State));

    return CreateProgressionSave(progression, state, actorId);
  }

  private static PendingFamilyAggregateSave CreateProgressionSave(
    FamilyAggregateSnapshot progression,
    JsonObject state,
    FamilyMemberId actorId
  )
  {
    using var document = JsonDocument.Parse(state.ToJsonString());
    var command = new SaveFamilyAggregateCommand(
      progression.FamilyId,
      progression.AggregateType,
      progression.Revision,
      document.RootElement.Clone(),
      actorId
    );

    return new PendingFamilyAggregateSave(command, 3);
  }

  private JsonObject BuildProgressionState(
    JsonObject profile,
    JsonObject contributionsState,
    JsonObject current
  )
  {
    var members = RequireArray(profile, "members");
    var contributions = RequireArray(contributionsState, "contributions");
    var weeklyTarget = RequiredInt(current, "currentWeekTarget");
    var completedWeeklyStreak = RequiredInt(current, "completedWeeklyStreak");
    var zone = new IanaTimeZone(RequiredString(profile, "timeZone"));
    var today = zone.LocalDate(_timeProvider.GetUtcNow());
    var period = IsoWeekPeriod.For(today);
    var progressionPolicy = new LadiProgressionPolicy();
    var streakPolicy = new PersonalStreakPolicy();
    var energyPolicy = new HouseEnergyPolicy();
    var maximumRate = new Percentage(DecimalValue(current, "maximumSavingsInterestRate") ?? 8);
    var evaluations = new JsonObject();
    var participantStages = new List<LadiStage>();

    foreach (var member in members.OfType<JsonObject>().Where(IsWeeklyParticipant))
    {
      var memberId = RequiredString(member, "id");
      var owned = contributions
        .OfType<JsonObject>()
        .Where(item => StringValue(item, "assigneeId") == memberId)
        .ToArray();
      var basic = owned.Where(item => StringValue(item, "kind") == "basic").ToArray();
      var assignedBasicEnergy = basic.Sum(item => DecimalValue(item, "energy") ?? 0);
      var approvedBasicEnergy = basic
        .Where(item => StringValue(item, "status") == "approved")
        .Sum(item => DecimalValue(item, "energy") ?? 0);
      var approvedExtraEnergy = owned
        .Where(item => StringValue(item, "kind") == "extra" && StringValue(item, "status") == "approved")
        .Sum(item => DecimalValue(item, "energy") ?? 0);
      var completion = ContributionParticipationPolicy.EffectiveCompletion(
        assignedBasicEnergy,
        approvedBasicEnergy,
        approvedExtraEnergy
      );
      var ratings = basic
        .Where(item => DecimalValue(item, "stars") is not null)
        .Select(item => RequiredDecimal(item, "stars"))
        .ToArray();
      var averageRating = ratings.Length == 0 ? 0 : ratings.Average();
      var approvedDays = owned
        .Where(item => StringValue(item, "status") == "approved")
        .Select(item => ParseOptionalInstant(item, "approvedAt"))
        .Where(item => item is not null)
        .Select(item => zone.LocalDate(item!.Value))
        .Where(period.Contains)
        .Distinct()
        .Count();
      var streakDays = streakPolicy.Calculate(
        RequiredInt(member, "weeklyStreak"),
        weeklyTarget,
        approvedDays
      );
      var participation = new PersonalParticipation(streakDays, completion.Value, averageRating);
      var value = progressionPolicy.Calculate(participation);
      var stage = progressionPolicy.DetermineStage(value.Capped);
      var interestRate = SavingsInterestPolicy.CalculateRate(stage, maximumRate);
      var energyContribution = energyPolicy.ContributionFor(stage);
      participantStages.Add(stage);
      evaluations[memberId] = new JsonObject
      {
        ["streakDays"] = streakDays,
        ["completionPercent"] = completion.Value,
        ["averageBasicRating"] = averageRating,
        ["participationValue"] = value.Capped,
        ["ladiStageId"] = stage.Id,
        ["ladiStagePosition"] = stage.Position,
        ["energyContribution"] = energyContribution,
        ["savingsInterestRate"] = interestRate.Value,
      };
    }

    var familyEnergy = energyPolicy.Calculate(participantStages);
    current["memberProgressions"] = evaluations;
    current["familyEnergy"] = familyEnergy;
    current["houseVisualLevel"] = energyPolicy.VisualLevel(familyEnergy);
    current["maximumSavingsInterestRate"] = maximumRate.Value;
    current["completedWeeklyStreak"] = completedWeeklyStreak;

    return current;
  }

  private static bool IsWeeklyParticipant(JsonObject member)
  {
    return !BoolValue(member, "invitationPending")
      && (StringValue(member, "role") == "child" || BoolValue(member, "participatesInWeeklyGoal"));
  }

  private static void SetParticipantStreaks(JsonArray members, int weeklyStreak)
  {
    foreach (var member in members.OfType<JsonObject>().Where(IsWeeklyParticipant))
    {
      member["weeklyStreak"] = weeklyStreak;
    }
  }

  private static DateTimeOffset? ParseOptionalInstant(JsonObject source, string property)
  {
    var value = StringValue(source, property);

    if (string.IsNullOrWhiteSpace(value))
    {
      return null;
    }

    Require(
      DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result),
      "The contribution approval timestamp is invalid."
    );

    return result;
  }

  private async Task<FamilyCommandResult> ExecuteSavingsAsync(
    FamilyCommand command,
    JsonArray members,
    JsonObject actor,
    JsonObject payload,
    CancellationToken cancellationToken
  )
  {
    var savings = await RequireSnapshotAsync(command.FamilyId, "savings", cancellationToken);
    var state = RequireObject(savings.State);
    var goals = RequireArray(state, "goals");
    FamilyAggregateSnapshot? progression = null;
    JsonObject? progressionState = null;
    switch (command.CommandType)
    {
      case "savings.set-currency":
        RequireAdministrator(actor);
        var currency = RequiredString(payload, "currency");
        Require(currency is "CHF" or "EUR" or "HUF", "The family currency is invalid.");
        state["familyCurrencyCode"] = currency;
        break;
      case "savings.set-exchange-rate":
        RequireAdministrator(actor);
        var rate = RequiredDecimal(payload, "rate");
        Require(rate >= 1, "The exchange rate must be at least one.");
        state["ladirchenPerCurrencyUnit"] = Round(rate);
        break;
      case "savings.toggle-cheer":
        ToggleCheer(goals, actor, payload);
        break;
      case "savings.add-goal":
        AddGoal(goals, members, actor, payload);
        break;
      case "savings.update-goal":
        UpdateGoal(goals, actor, payload);
        break;
      case "savings.support-goal":
        SupportGoal(state, goals, actor, payload);
        break;
      case "savings.gift-to-goal":
        GiftToGoal(state, goals, members, actor, payload);
        break;
      case "savings.gift-to-member":
        GiftToMember(state, members, actor, payload);
        break;
      case "savings.special-gift":
        SpecialGift(state, members, actor, payload);
        break;
      case "savings.consume-guardian-gift":
        ConsumeGuardianGift(state, actor, payload);
        break;
      case "savings.deposit":
        Deposit(state, goals, actor, payload);
        break;
      case "savings.withdraw":
        Withdraw(state, goals, actor, payload);
        break;
      case "savings.cancel-goal":
        CancelGoal(state, goals, actor, payload);
        break;
      case "savings.credit-weekly-interest":
        progression = await RequireSnapshotAsync(command.FamilyId, "family-progression", cancellationToken);
        progressionState = RequireObject(progression.State);
        CreditWeeklyInterest(goals, progressionState);
        break;
      case "savings.simulate-interest":
        progression = await RequireSnapshotAsync(command.FamilyId, "family-progression", cancellationToken);
        progressionState = RequireObject(progression.State);
        CreditSimulatedInterest(goals, progressionState, actor, payload);
        break;
      default:
        throw new FamilyCommandException("The savings command is not supported.");
    }

    var changed = await _repository.SaveBatchAsync(
      new[] { CreateSave(savings, state, command.ActorId) },
      cancellationToken
    );

    foreach (var snapshot in changed)
    {
      await _publisher.PublishAsync(snapshot, cancellationToken);
    }

    return new FamilyCommandResult(changed);
  }

  private async Task<FamilyAggregateSnapshot> RequireSnapshotAsync(
    FamilyId familyId,
    string aggregateType,
    CancellationToken cancellationToken
  )
  {
    return await _repository.GetAsync(familyId, aggregateType, cancellationToken)
      ?? throw new FamilyCommandException($"The {aggregateType} aggregate has not been initialized.");
  }

  private static PendingFamilyAggregateSave CreateSave(
    FamilyAggregateSnapshot current,
    JsonObject state,
    FamilyMemberId actorId
  )
  {
    using var document = JsonDocument.Parse(state.ToJsonString());
    var command = new SaveFamilyAggregateCommand(
      current.FamilyId,
      current.AggregateType,
      current.Revision,
      document.RootElement.Clone(),
      actorId
    );

    return new PendingFamilyAggregateSave(command, current.SchemaVersion);
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
    var configuredBonus = DecimalValue(payload, "perfectRatingBonusPercent") ?? 5;
    var reward = ContributionRewardPolicy.Calculate(
      new LadirchenAmount(RequiredDecimal(contribution, "reward")),
      rating,
      new Percentage(configuredBonus),
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
      ["reward"] = RequiredDecimal(payload, "reward"),
      ["energy"] = kind == "basic" ? RequiredDecimal(payload, "energy") : 0,
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
    promotions.Add(
      new JsonObject
      {
        ["id"] = id,
        ["contributionId"] = contributionId,
        ["title"] = string.Empty,
        ["titleKey"] = "contributions.promotions.multiplierTitle",
        ["multiplier"] = multiplier,
        ["deadline"] = RequiredString(payload, "deadline"),
        ["teamworkBonus"] = RequiredDecimal(payload, "teamworkBonus"),
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

  private static void ToggleCheer(JsonArray goals, JsonObject actor, JsonObject payload)
  {
    var goal = RequireGoal(goals, RequiredString(payload, "id"));
    Require(CanViewGoal(actor, goal), "The savings goal is not visible to this member.");
    goal["cheered"] = !BoolValue(goal, "cheered");
  }

  private static void AddShopReward(JsonArray rewards, JsonObject actor, JsonObject payload)
  {
    RequireAdministrator(actor);
    var id = RequiredString(payload, "id");
    Require(FindById(rewards, id) is null, "The shop reward already exists.");
    var quantity = RequiredInt(payload, "quantity");
    Require(quantity >= 0, "The shop quantity cannot be negative.");
    var price = RequiredDecimal(payload, "price");
    Require(price >= 0, "The shop price cannot be negative.");
    var reward = new JsonObject
    {
      ["id"] = id,
      ["title"] = RequiredString(payload, "title"),
      ["description"] = RequiredString(payload, "description"),
      ["icon"] = RequiredString(payload, "icon"),
      ["price"] = price,
      ["category"] = RequiredString(payload, "category"),
      ["quantity"] = quantity,
      ["conditions"] = StringValue(payload, "conditions") ?? string.Empty,
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
    Require(ShopIsOpen(profile, now), "Shop requests close at 18:00 local time.");
    Require(ShopRewardIsPublished(profile, reward, now), "The reward is not published today.");
    var actorId = RequiredString(actor, "id");
    var balance = DecimalValue(RequireObject(savings, "balances"), actorId) ?? 0;
    Require(balance >= RequiredDecimal(reward, "price"), "The shop reward exceeds the available balance.");
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
      Require(ShopIsOpen(profile, now), "Shop approvals close at 18:00 local time.");
      var requesterId = RequiredString(reward, "requesterId");
      _ = RequireMember(members, requesterId);
      var price = RequiredDecimal(reward, "price");
      DebitBalance(savings, requesterId, price);
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
    return LocalDateTime(profile, now).Hour < 18;
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

  private static void AddGoal(JsonArray goals, JsonArray members, JsonObject actor, JsonObject payload)
  {
    var id = RequiredString(payload, "id");
    Require(FindById(goals, id) is null, "The savings goal already exists.");
    var ownerId = RequiredString(payload, "ownerId");
    var actorId = RequiredString(actor, "id");
    var ownerIsFamily = ownerId == "family";
    JsonObject? owner = null;

    if (!ownerIsFamily)
    {
      owner = RequireMember(members, ownerId);
    }

    Require(
      ownerIsFamily && (IsAdministrator(actor) || StringValue(actor, "role") == "child")
        || ownerId == actorId
        || IsAdministrator(actor) && StringValue(owner!, "role") == "child",
      "The member may not create this savings goal."
    );
    var target = RequiredDecimal(payload, "target");
    Require(target > 0, "The savings target must be positive.");
    var starterBonus = Math.Min(ownerIsFamily ? 5m * ActiveMemberCount(members) : 5m, target);
    var visibility = ownerIsFamily ? "family" : RequiredString(payload, "visibility");
    Require(visibility is "family" or "guardians" or "private", "The savings visibility is invalid.");
    goals.Add(
      new JsonObject
      {
        ["id"] = id,
        ["title"] = RequiredString(payload, "title"),
        ["icon"] = RequiredString(payload, "icon"),
        ["ownerId"] = ownerId,
        ["target"] = target,
        ["saved"] = starterBonus,
        ["starterBonus"] = starterBonus,
        ["shared"] = ownerIsFamily,
        ["visibility"] = visibility,
        ["cheered"] = false,
      }
    );
  }

  private static void UpdateGoal(JsonArray goals, JsonObject actor, JsonObject payload)
  {
    var goal = RequireGoal(goals, RequiredString(payload, "id"));
    var actorId = RequiredString(actor, "id");
    Require(
      StringValue(goal, "ownerId") == actorId || IsAdministrator(actor),
      "Only the owner or an administrator may update this savings goal."
    );
    var target = RequiredDecimal(payload, "target");
    Require(target >= RequiredDecimal(goal, "saved"), "The target cannot be lower than the saved amount.");
    var visibility = StringValue(goal, "ownerId") == "family" ? "family" : RequiredString(payload, "visibility");
    Require(visibility is "family" or "guardians" or "private", "The savings visibility is invalid.");
    goal["title"] = RequiredString(payload, "title");
    goal["icon"] = RequiredString(payload, "icon");
    goal["target"] = target;
    goal["visibility"] = visibility;
  }

  private static void SupportGoal(JsonObject state, JsonArray goals, JsonObject actor, JsonObject payload)
  {
    Require(StringValue(actor, "role") == "guardian", "Only guardians may support a savings goal.");
    var goal = RequireGoal(goals, RequiredString(payload, "id"));
    Require(CanViewGoal(actor, goal), "The savings goal is not visible to this member.");
    var amount = LimitedGoalAmount(goal, RequiredDecimal(payload, "amount"));
    goal["saved"] = RequiredDecimal(goal, "saved") + amount;

    if (StringValue(goal, "ownerId") != "family")
    {
      AddGuardianGift(state, payload, RequiredString(goal, "ownerId"), RequiredString(goal, "title"), "goal", amount);
    }
  }

  private static void GiftToGoal(
    JsonObject state,
    JsonArray goals,
    JsonArray members,
    JsonObject actor,
    JsonObject payload
  )
  {
    Require(StringValue(actor, "role") == "child", "Only children may transfer Ladirchen to a savings goal.");
    var goal = RequireGoal(goals, RequiredString(payload, "id"));
    var actorId = RequiredString(actor, "id");
    var ownerId = RequiredString(goal, "ownerId");
    Require(ownerId != actorId && StringValue(goal, "visibility") == "family", "The goal cannot receive this gift.");

    if (ownerId != "family")
    {
      var owner = RequireMember(members, ownerId);
      Require(StringValue(owner, "role") == "child", "A personal gift requires a child recipient.");
    }

    var amount = LimitedGoalAmount(goal, RequiredDecimal(payload, "amount"));
    DebitBalance(state, actorId, amount);
    goal["saved"] = RequiredDecimal(goal, "saved") + amount;

    if (ownerId != "family")
    {
      AddGuardianGift(state, payload, ownerId, RequiredString(goal, "title"), "goal", amount);
    }
  }

  private static void GiftToMember(JsonObject state, JsonArray members, JsonObject actor, JsonObject payload)
  {
    Require(StringValue(actor, "role") == "child", "Only children may transfer Ladirchen to a member.");
    var actorId = RequiredString(actor, "id");
    var recipientId = RequiredString(payload, "recipientId");
    var recipient = RequireMember(members, recipientId);
    Require(StringValue(recipient, "role") == "child" && recipientId != actorId, "A gift requires another child.");
    var amount = PositiveRoundedAmount(payload);
    DebitBalance(state, actorId, amount);
    CreditBalance(state, recipientId, amount);
    AddGuardianGift(state, payload, recipientId, string.Empty, "balance", amount);
  }

  private static void SpecialGift(JsonObject state, JsonArray members, JsonObject actor, JsonObject payload)
  {
    RequireAdministrator(actor);
    var childId = RequiredString(payload, "childId");
    var child = RequireMember(members, childId);
    Require(StringValue(child, "role") == "child", "A special gift requires a child recipient.");
    var amount = PositiveRoundedAmount(payload);
    Require(amount <= 10000, "A special gift must not exceed 10000 Ladirchen.");
    _ = RequiredString(payload, "reason");
    CreditBalance(state, childId, amount);
    AddGuardianGift(state, payload, childId, RequiredString(payload, "reason"), "balance", amount);
  }

  private static void ConsumeGuardianGift(JsonObject state, JsonObject actor, JsonObject payload)
  {
    var gifts = RequireArray(state, "pendingGuardianGifts");
    var gift = FindById(gifts, RequiredString(payload, "id"))
      ?? throw new FamilyCommandException("The guardian gift does not exist.");
    Require(StringValue(gift, "childId") == StringValue(actor, "id"), "Only the recipient may acknowledge this gift.");
    gifts.Remove(gift);
  }

  private static void Deposit(JsonObject state, JsonArray goals, JsonObject actor, JsonObject payload)
  {
    var goal = RequireGoal(goals, RequiredString(payload, "id"));
    var actorId = RequiredString(actor, "id");
    Require(StringValue(goal, "ownerId") == actorId, "Only the owner may deposit into this savings goal.");
    var amount = LimitedGoalAmount(goal, RequiredDecimal(payload, "amount"));
    DebitBalance(state, actorId, amount);
    goal["saved"] = RequiredDecimal(goal, "saved") + amount;
  }

  private static void Withdraw(JsonObject state, JsonArray goals, JsonObject actor, JsonObject payload)
  {
    var goal = RequireGoal(goals, RequiredString(payload, "id"));
    var actorId = RequiredString(actor, "id");
    Require(StringValue(goal, "ownerId") == actorId, "Only the owner may withdraw from this savings goal.");
    var saved = RequiredDecimal(goal, "saved");
    var completed = saved >= RequiredDecimal(goal, "target");
    var withdrawable = completed ? saved : Math.Max(0, saved - (DecimalValue(goal, "starterBonus") ?? 0));
    var requested = RequiredDecimal(payload, "amount");
    Require(requested > 0, "The withdrawal must be positive.");
    var amount = Math.Min(requested, withdrawable);
    Require(amount > 0, "The starter bonus is locked until completion.");
    goal["saved"] = saved - amount;
    CreditBalance(state, actorId, amount);
  }

  private static void CancelGoal(JsonObject state, JsonArray goals, JsonObject actor, JsonObject payload)
  {
    var goal = RequireGoal(goals, RequiredString(payload, "id"));
    var actorId = RequiredString(actor, "id");
    var ownerId = RequiredString(goal, "ownerId");
    Require(
      ownerId == actorId || ownerId == "family" && (IsAdministrator(actor) || StringValue(actor, "role") == "child"),
      "The member may not cancel this savings goal."
    );
    var saved = RequiredDecimal(goal, "saved");
    var completed = saved >= RequiredDecimal(goal, "target");
    var returned = completed ? saved : Math.Max(0, saved - (DecimalValue(goal, "starterBonus") ?? 0));
    CreditBalance(state, actorId, returned);
    goals.Remove(goal);
  }

  private static JsonObject RequireGoal(JsonArray goals, string id)
  {
    return FindById(goals, id) ?? throw new FamilyCommandException("The savings goal does not exist.");
  }

  private static void CreditWeeklyInterest(JsonArray goals, JsonObject progression)
  {
    var evaluations = RequireObject(progression, "memberProgressions");

    foreach (var goal in goals.OfType<JsonObject>())
    {
      var ownerId = RequiredString(goal, "ownerId");

      if (ownerId == "family" || evaluations[ownerId] is not JsonObject evaluation)
      {
        continue;
      }

      CreditGoalInterest(goal, RequiredDecimal(evaluation, "savingsInterestRate"));
    }
  }

  private static void CreditSimulatedInterest(
    JsonArray goals,
    JsonObject progression,
    JsonObject actor,
    JsonObject payload
  )
  {
    Require(StringValue(actor, "role") == "child", "Only a child may simulate personal savings interest.");
    var selectedRate = RequiredDecimal(payload, "selectedRate");
    var maximumRate = RequiredDecimal(progression, "maximumSavingsInterestRate");
    var appliedRate = Math.Max(0, Math.Min(maximumRate, selectedRate));
    var actorId = RequiredString(actor, "id");

    foreach (var goal in goals.OfType<JsonObject>().Where(item => StringValue(item, "ownerId") == actorId))
    {
      CreditGoalInterest(goal, appliedRate);
    }
  }

  private static void CreditGoalInterest(JsonObject goal, decimal rate)
  {
    var saved = RequiredDecimal(goal, "saved");
    var target = RequiredDecimal(goal, "target");
    var credit = SavingsInterestPolicy.CalculateCredit(
      new LadirchenAmount(saved),
      new LadirchenAmount(target),
      new Percentage(rate)
    );

    if (credit <= 0)
    {
      return;
    }

    goal["saved"] = saved + credit;
    goal["interestEarned"] = (DecimalValue(goal, "interestEarned") ?? 0) + credit;
  }

  private static bool CanViewGoal(JsonObject actor, JsonObject goal)
  {
    return StringValue(goal, "ownerId") == StringValue(actor, "id")
      || StringValue(goal, "visibility") == "family"
      || StringValue(goal, "visibility") == "guardians" && IsAdministrator(actor);
  }

  private static decimal LimitedGoalAmount(JsonObject goal, decimal requested)
  {
    Require(requested > 0, "The amount must be positive.");
    var amount = Math.Min(Round(requested), RequiredDecimal(goal, "target") - RequiredDecimal(goal, "saved"));
    Require(amount > 0, "The savings goal is already complete.");

    return amount;
  }

  private static decimal PositiveRoundedAmount(JsonObject payload)
  {
    var amount = Round(RequiredDecimal(payload, "amount"));
    Require(amount > 0, "The rounded amount must be positive.");

    return amount;
  }

  private static void DebitBalance(JsonObject state, string memberId, decimal amount)
  {
    var balances = RequireObject(state, "balances");
    var balance = DecimalValue(balances, memberId) ?? 0;
    Require(balance >= amount, "The transfer exceeds the available balance.");
    balances[memberId] = balance - amount;
  }

  private static void CreditBalance(JsonObject state, string memberId, decimal amount)
  {
    var balances = RequireObject(state, "balances");
    balances[memberId] = (DecimalValue(balances, memberId) ?? 0) + amount;
  }

  private static void AddGuardianGift(
    JsonObject state,
    JsonObject payload,
    string childId,
    string goalTitle,
    string destination,
    decimal amount
  )
  {
    RequireArray(state, "pendingGuardianGifts").Add(
      new JsonObject
      {
        ["id"] = RequiredString(payload, "giftId"),
        ["childId"] = childId,
        ["guardianName"] = RequiredString(payload, "senderName"),
        ["goalTitle"] = goalTitle,
        ["destination"] = destination,
        ["amount"] = amount,
      }
    );
  }

  private static int ActiveMemberCount(JsonArray members)
  {
    return members.OfType<JsonObject>().Count(member => !BoolValue(member, "invitationPending"));
  }

  private static decimal Round(decimal value)
  {
    return decimal.Round(value, 0, MidpointRounding.AwayFromZero);
  }

  private static JsonObject? FindActivePromotion(
    JsonArray promotions,
    string contributionId,
    JsonObject profile,
    DateTimeOffset now
  )
  {
    var timeZoneId = RequiredString(profile, "timeZone");
    var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    var localNow = TimeZoneInfo.ConvertTime(now, zone);

    return promotions
      .OfType<JsonObject>()
      .FirstOrDefault(item =>
        StringValue(item, "contributionId") == contributionId
        && BoolValue(item, "active")
        && TimeOnly.TryParseExact(StringValue(item, "deadline"), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var deadline)
        && TimeOnly.FromDateTime(localNow.DateTime) < deadline
      );
  }

  private static JsonObject RequireContribution(JsonArray contributions, string id)
  {
    return FindById(contributions, id) ?? throw new FamilyCommandException("The contribution does not exist.");
  }

  private static JsonObject RequireMember(JsonArray members, string id)
  {
    return FindById(members, id) ?? throw new FamilyCommandException("The family member does not exist.");
  }

  private static JsonObject? FindById(JsonArray items, string id)
  {
    return items.OfType<JsonObject>().FirstOrDefault(item => StringValue(item, "id") == id);
  }

  private static void RequireStatus(JsonObject contribution, string status)
  {
    Require(StringValue(contribution, "status") == status, $"The contribution must be {status}.");
  }

  private static void RequireAdministrator(JsonObject actor)
  {
    Require(IsAdministrator(actor), "Only a family administrator may perform this command.");
  }

  private static bool IsAdministrator(JsonObject member)
  {
    return StringValue(member, "role") == "guardian" && StringValue(member, "guardianAccess") == "admin";
  }

  private static JsonObject RequireObject(JsonElement element)
  {
    return JsonNode.Parse(element.GetRawText()) as JsonObject
      ?? throw new FamilyCommandException("A JSON object is required.");
  }

  private static JsonObject RequireObject(JsonObject parent, string property)
  {
    return parent[property] as JsonObject
      ?? throw new FamilyCommandException($"The {property} object is required.");
  }

  private static JsonArray RequireArray(JsonObject parent, string property)
  {
    return parent[property] as JsonArray
      ?? throw new FamilyCommandException($"The {property} collection is required.");
  }

  private static string RequiredString(JsonObject value, string property)
  {
    var result = StringValue(value, property);
    Require(!string.IsNullOrWhiteSpace(result), $"The {property} value is required.");

    return result!;
  }

  private static string RequiredString(JsonNode? value)
  {
    var result = value?.GetValue<string>();
    Require(!string.IsNullOrWhiteSpace(result), "A non-empty string value is required.");

    return result!;
  }

  private static string? StringValue(JsonObject value, string property)
  {
    return value[property]?.GetValue<string>();
  }

  private static int RequiredInt(JsonObject value, string property)
  {
    return value[property]?.GetValue<int>()
      ?? throw new FamilyCommandException($"The {property} value is required.");
  }

  private static decimal RequiredDecimal(JsonObject value, string property)
  {
    return DecimalValue(value, property)
      ?? throw new FamilyCommandException($"The {property} value is required.");
  }

  private static decimal? DecimalValue(JsonObject value, string property)
  {
    return value[property]?.GetValue<decimal>();
  }

  private static bool BoolValue(JsonObject value, string property)
  {
    return value[property]?.GetValue<bool>() ?? false;
  }

  private static bool BoolValueOrDefault(JsonObject value, string property, bool defaultValue)
  {
    return value[property]?.GetValue<bool>() ?? defaultValue;
  }

  private static bool RequiredBool(JsonObject value, string property)
  {
    return value[property]?.GetValue<bool>()
      ?? throw new FamilyCommandException($"The {property} value is required.");
  }

  private static void CopyOptionalString(JsonObject source, JsonObject target, string property)
  {
    var value = StringValue(source, property);

    if (!string.IsNullOrWhiteSpace(value))
    {
      target[property] = value;
    }
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new FamilyCommandException(message);
    }
  }
}
