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
    var zone = new IanaTimeZone(RequiredString(profileState, "timeZone"));
    var currentPeriod = IsoWeekPeriod.For(zone.LocalDate(_timeProvider.GetUtcNow()));

    switch (command.CommandType)
    {
      case "progression.refresh":
        break;
      case "progression.set-maximum-savings-rate":
        RequireAdministrator(actor);
        var maximumSavingsRate = RequiredDecimal(RequireObject(command.Payload), "percent");
        Require(maximumSavingsRate > 0 && maximumSavingsRate <= 100, "The maximum savings rate must be above zero and at most 100 percent.");
        progressionState["maximumSavingsInterestRate"] = Round(maximumSavingsRate);
        progressionState = BuildProgressionState(profileState, contributionState, progressionState);
        break;
      case "progression.complete-week":
        RequireAdministrator(actor);
        RequireWeekNotClosed(progressionState, currentPeriod);
        Require(
          RequiredInt(progressionState, "familyEnergy") >= HouseEnergyPolicy.MinimumSuccessfulWeekEnergy,
          "Family energy is too low to complete the week."
        );
        var completedStreak = RequiredInt(progressionState, "completedWeeklyStreak") + 1;
        progressionState["completedWeeklyStreak"] = completedStreak;
        progressionState["houseLevel"] = Math.Min(4, RequiredInt(progressionState, "houseLevel") + 1);
        SetParticipantStreaks(members, completedStreak);
        StoreWeeklyEvaluation(progressionState, currentPeriod, "completed", _timeProvider.GetUtcNow());
        saves.Add(CreateSave(profile, profileState, command.ActorId));
        break;
      case "progression.fail-week":
        RequireAdministrator(actor);
        RequireWeekNotClosed(progressionState, currentPeriod);
        var familyEnergy = RequiredInt(progressionState, "familyEnergy");
        Require(
          familyEnergy < HouseEnergyPolicy.MinimumSuccessfulWeekEnergy,
          "A successful family week cannot be closed as failed."
        );
        var currentStreak = RequiredInt(progressionState, "completedWeeklyStreak");
        var failedStreak = familyEnergy < HouseEnergyPolicy.SevereFailureEnergyThreshold
          ? Math.Max(0, currentStreak - 1)
          : currentStreak;
        progressionState["completedWeeklyStreak"] = failedStreak;
        progressionState["houseLevel"] = Math.Max(0, RequiredInt(progressionState, "houseLevel") - 1);
        SetParticipantStreaks(members, failedStreak);
        StoreWeeklyEvaluation(progressionState, currentPeriod, "failed", _timeProvider.GetUtcNow());
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

    return new PendingFamilyAggregateSave(command, 4);
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
    var maximumRate = new Percentage(
      DecimalValue(current, "maximumSavingsInterestRate") ?? SavingsInterestPolicy.Default.MaximumRate.Value
    );
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
      var approvedDates = owned
        .Where(item => StringValue(item, "status") == "approved")
        .Select(item => ParseOptionalInstant(item, "approvedAt"))
        .Where(item => item is not null)
        .Select(item => zone.LocalDate(item!.Value))
        .Where(period.Contains)
        .Distinct()
        .Order()
        .ToArray();
      var streakDays = streakPolicy.Calculate(
        RequiredInt(member, "weeklyStreak"),
        weeklyTarget,
        approvedDates.Length
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
        ["approvedDates"] = new JsonArray(approvedDates
          .Select(date => JsonValue.Create(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)))
          .ToArray<JsonNode?>()),
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

  private static void RequireWeekNotClosed(JsonObject progression, IsoWeekPeriod period)
  {
    var outcomes = progression["closedWeekOutcomes"] as JsonObject ?? new JsonObject();
    progression["closedWeekOutcomes"] = outcomes;
    Require(outcomes[period.Key] is null, "The family week has already been closed.");
  }

  private static void StoreWeeklyEvaluation(
    JsonObject progression,
    IsoWeekPeriod period,
    string outcome,
    DateTimeOffset evaluatedAt
  )
  {
    var outcomes = progression["closedWeekOutcomes"] as JsonObject ?? new JsonObject();
    var evaluations = progression["weeklyEvaluations"] as JsonObject ?? new JsonObject();
    progression["closedWeekOutcomes"] = outcomes;
    progression["weeklyEvaluations"] = evaluations;
    outcomes[period.Key] = outcome;
    evaluations[period.Key] = new JsonObject
    {
      ["evaluatedAt"] = evaluatedAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
      ["members"] = progression["memberProgressions"]?.DeepClone() ?? new JsonObject(),
    };
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

}
