using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal static class FamilyProgressionSnapshotMapper
{
  public static int ReadCurrentWeekTarget(JsonObject state)
  {
    return RequiredInt(state, "currentWeekTarget");
  }

  public static int ReadCompletedWeeklyStreak(JsonObject state)
  {
    return RequiredInt(state, "completedWeeklyStreak");
  }

  public static int ReadHouseLevel(JsonObject state)
  {
    return RequiredInt(state, "houseLevel");
  }

  public static int ReadFamilyEnergy(JsonObject state)
  {
    return RequiredInt(state, "familyEnergy");
  }

  public static Percentage ReadMaximumSavingsInterestRate(JsonObject state)
  {
    var value = OptionalDecimal(state, "maximumSavingsInterestRate")
      ?? LadirchenApp.Domain.Economy.SavingsInterestPolicy.Default.MaximumRate.Value;

    return new Percentage(value);
  }

  public static IReadOnlyDictionary<FamilyMemberId, int> ReadStoredWeeklyStreaks(JsonObject profile)
  {
    var result = new Dictionary<FamilyMemberId, int>();

    foreach (var item in RequiredArray(profile, "members").OfType<JsonObject>())
    {
      result[new FamilyMemberId(RequiredString(item, "id"))] = RequiredInt(item, "weeklyStreak");
    }

    return result;
  }

  public static void RestoreClosedWeeks(JsonObject state, FamilyHouse house)
  {
    ArgumentNullException.ThrowIfNull(state);
    ArgumentNullException.ThrowIfNull(house);
    var outcomes = state["closedWeekOutcomes"] as JsonObject ?? new JsonObject();
    var weeks = state["weeklyEvaluations"] as JsonObject ?? new JsonObject();

    foreach (var storedOutcome in outcomes)
    {
      if (!IsoWeekPeriod.TryParse(storedOutcome.Key, out var period) || period is null)
      {
        throw new FamilyCommandException("A stored closed-week period is invalid.");
      }

      house.RestoreClosedWeek(ReadClosedWeek(outcomes, weeks, period));
    }
  }

  public static ClosedFamilyWeek ReadClosedWeek(JsonObject state, IsoWeekPeriod period)
  {
    ArgumentNullException.ThrowIfNull(state);
    ArgumentNullException.ThrowIfNull(period);
    var outcomes = state["closedWeekOutcomes"] as JsonObject
      ?? throw new FamilyCommandException("No closed family weeks are available.");
    var weeks = state["weeklyEvaluations"] as JsonObject
      ?? throw new FamilyCommandException("No closed-week evaluations are available.");

    return ReadClosedWeek(outcomes, weeks, period);
  }

  public static JsonObject WriteEvaluation(
    JsonObject original,
    FamilyProgressionEvaluation evaluation,
    FamilyHouse house
  )
  {
    ArgumentNullException.ThrowIfNull(original);
    ArgumentNullException.ThrowIfNull(evaluation);
    ArgumentNullException.ThrowIfNull(house);
    var result = original.DeepClone().AsObject();
    var members = new JsonObject();

    foreach (var member in evaluation.Members)
    {
      var personal = member.LadiEvaluation;
      members[personal.MemberId.Value] = new JsonObject
      {
        ["streakDays"] = personal.Participation.StreakDays,
        ["approvedDates"] = new JsonArray(
          member.ApprovedDates
            .Select(date => JsonValue.Create(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)))
            .ToArray<JsonNode?>()
        ),
        ["completionPercent"] = personal.Participation.EffectiveCompletionPercent,
        ["averageBasicRating"] = personal.Participation.AverageBasicRating,
        ["participationValue"] = personal.Value.Capped,
        ["ladiStageId"] = personal.Stage.Id,
        ["ladiStagePosition"] = personal.Stage.Position,
        ["energyContribution"] = member.EnergyContribution,
        ["savingsInterestRate"] = member.SavingsInterestRate.Value,
      };
    }

    result["memberProgressions"] = members;
    result["familyEnergy"] = evaluation.FamilyEnergy;
    result["houseVisualLevel"] = evaluation.HouseVisualLevel;
    result["maximumSavingsInterestRate"] = evaluation.MaximumSavingsInterestRate.Value;
    result["completedWeeklyStreak"] = house.CompletedWeeklyStreak;
    result["houseLevel"] = house.StoredLevel;
    WriteClosedWeeks(result, house, evaluation, members);

    return result;
  }

  public static void WriteParticipantStreaks(JsonObject profile, Family family, FamilyHouse house)
  {
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(family);
    ArgumentNullException.ThrowIfNull(house);
    var byId = RequiredArray(profile, "members")
      .OfType<JsonObject>()
      .ToDictionary(item => RequiredString(item, "id"), StringComparer.Ordinal);

    foreach (var member in family.Members.Where(IsWeeklyParticipant))
    {
      if (!byId.TryGetValue(member.Id.Value, out var storedMember))
      {
        throw new FamilyCommandException("A participating family member is missing from the profile snapshot.");
      }

      storedMember["weeklyStreak"] = house.StoredWeeklyStreakFor(member.Id);
    }
  }

  private static void WriteClosedWeeks(
    JsonObject progression,
    FamilyHouse house,
    FamilyProgressionEvaluation currentEvaluation,
    JsonObject currentMembers
  )
  {
    ArgumentNullException.ThrowIfNull(progression);
    var outcomes = progression["closedWeekOutcomes"] as JsonObject ?? new JsonObject();
    var evaluations = progression["weeklyEvaluations"] as JsonObject ?? new JsonObject();
    progression["closedWeekOutcomes"] = outcomes;
    progression["weeklyEvaluations"] = evaluations;

    foreach (var week in house.ClosedWeeks.OrderBy(item => item.Period.Start))
    {
      outcomes[week.Period.Key] = FormatOutcome(week.Outcome);

      if (evaluations[week.Period.Key] is not null)
      {
        continue;
      }

      var isCurrentEvaluation = currentEvaluation.Members.All(item =>
        item.LadiEvaluation.IsoYear == week.Period.Year
        && item.LadiEvaluation.IsoWeek == week.Period.Week
      );
      evaluations[week.Period.Key] = new JsonObject
      {
        ["evaluatedAt"] = week.EvaluatedAt.ToString("O", CultureInfo.InvariantCulture),
        ["members"] = isCurrentEvaluation
          ? currentMembers.DeepClone()
          : WriteMemberEvaluations(week.MemberEvaluations.Values),
      };
    }
  }

  public static bool IsWeeklyParticipant(FamilyMember member)
  {
    ArgumentNullException.ThrowIfNull(member);

    return !member.InvitationPending
      && (member.Role == MemberRole.Child || member.ParticipatesInWeeklyGoal);
  }

  private static IReadOnlyCollection<PersonalLadiEvaluation> ReadMemberEvaluations(
    JsonObject members,
    IsoWeekPeriod period
  )
  {
    var result = new List<PersonalLadiEvaluation>();

    foreach (var property in members)
    {
      var member = property.Value as JsonObject
        ?? throw new FamilyCommandException("Every stored Ladi evaluation must be an object.");
      var memberId = new FamilyMemberId(property.Key);
      var value = RequiredDecimal(member, "participationValue");
      result.Add(
        new PersonalLadiEvaluation(
          memberId,
          period.Year,
          period.Week,
          new PersonalParticipation(
            RequiredInt(member, "streakDays"),
            RequiredDecimal(member, "completionPercent"),
            RequiredDecimal(member, "averageBasicRating")
          ),
          new ParticipationValue(value, value),
          new LadiStage(
            RequiredString(member, "ladiStageId"),
            RequiredInt(member, "ladiStagePosition")
          )
        )
      );
    }

    return result;
  }

  private static ClosedFamilyWeek ReadClosedWeek(
    JsonObject outcomes,
    JsonObject weeks,
    IsoWeekPeriod period
  )
  {
    var storedOutcome = outcomes[period.Key]
      ?? throw new FamilyCommandException("The requested family week has not been closed.");
    var week = weeks[period.Key] as JsonObject
      ?? throw new FamilyCommandException("The requested week has no immutable Ladi evaluation.");

    return new ClosedFamilyWeek(
      period,
      ParseOutcome(RequiredString(storedOutcome)),
      RequiredInstant(week, "evaluatedAt"),
      ReadMemberEvaluations(RequiredObject(week, "members"), period)
    );
  }

  private static JsonObject WriteMemberEvaluations(
    IEnumerable<PersonalLadiEvaluation> evaluations
  )
  {
    var result = new JsonObject();

    foreach (var evaluation in evaluations)
    {
      result[evaluation.MemberId.Value] = new JsonObject
      {
        ["streakDays"] = evaluation.Participation.StreakDays,
        ["completionPercent"] = evaluation.Participation.EffectiveCompletionPercent,
        ["averageBasicRating"] = evaluation.Participation.AverageBasicRating,
        ["participationValue"] = evaluation.Value.Capped,
        ["ladiStageId"] = evaluation.Stage.Id,
        ["ladiStagePosition"] = evaluation.Stage.Position,
      };
    }

    return result;
  }

  private static FamilyWeekOutcome ParseOutcome(string value)
  {
    return value switch
    {
      "completed" => FamilyWeekOutcome.Completed,
      "failed" => FamilyWeekOutcome.Failed,
      _ => throw new FamilyCommandException("A stored family-week outcome is invalid."),
    };
  }

  private static string FormatOutcome(FamilyWeekOutcome outcome)
  {
    return outcome switch
    {
      FamilyWeekOutcome.Completed => "completed",
      FamilyWeekOutcome.Failed => "failed",
      _ => throw new ArgumentOutOfRangeException(nameof(outcome)),
    };
  }

  private static JsonObject RequiredObject(JsonObject value, string property)
  {
    return value[property] as JsonObject
      ?? throw new FamilyCommandException($"The stored {property} object is missing.");
  }

  private static DateTimeOffset RequiredInstant(JsonObject value, string property)
  {
    var text = RequiredString(value, property);

    if (!DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
    {
      throw new FamilyCommandException($"The stored {property} timestamp is invalid.");
    }

    return result;
  }

  private static JsonArray RequiredArray(JsonObject value, string property)
  {
    return value[property] as JsonArray
      ?? throw new FamilyCommandException($"The stored {property} collection is missing.");
  }

  private static string RequiredString(JsonObject value, string property)
  {
    var text = value[property]?.GetValue<string>();

    if (string.IsNullOrWhiteSpace(text))
    {
      throw new FamilyCommandException($"The stored {property} value is missing.");
    }

    return text;
  }

  private static int RequiredInt(JsonObject value, string property)
  {
    return value[property]?.GetValue<int>()
      ?? throw new FamilyCommandException($"The stored {property} value is missing.");
  }

  private static decimal RequiredDecimal(JsonObject value, string property)
  {
    return value[property]?.GetValue<decimal>()
      ?? throw new FamilyCommandException($"The stored {property} value is missing.");
  }

  private static string RequiredString(JsonNode? value)
  {
    var text = value?.GetValue<string>();

    if (string.IsNullOrWhiteSpace(text))
    {
      throw new FamilyCommandException("A stored string value is missing.");
    }

    return text;
  }

  private static decimal? OptionalDecimal(JsonObject value, string property)
  {
    return value[property]?.GetValue<decimal>();
  }
}
