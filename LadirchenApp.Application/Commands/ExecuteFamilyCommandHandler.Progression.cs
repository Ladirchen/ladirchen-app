using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

public sealed partial class ExecuteFamilyCommandHandler
{
  private async Task<FamilyCommandResult> ExecuteProgressionAsync(
    FamilyCommand command,
    FamilyAggregateSnapshot profile,
    JsonObject profileState,
    CancellationToken cancellationToken
  )
  {
    var family = FamilyProfileSnapshotMapper.Rehydrate(command.FamilyId, profileState);
    _ = family.ActiveMember(command.ActorId);
    var contributions = await RequireSnapshotAsync(command.FamilyId, "contributions", cancellationToken);
    var contributionState = RequireObject(contributions.State);
    var economy = FamilyEconomySnapshotMapper.RehydrateContributions(family, contributionState);
    var progression = await RequireSnapshotAsync(command.FamilyId, "family-progression", cancellationToken);
    var storedProgression = RequireObject(progression.State);
    var house = RestoreHouse(family, profileState, storedProgression);
    var zone = family.TimeZone;
    var currentPeriod = IsoWeekPeriod.For(zone.LocalDate(_timeProvider.GetUtcNow()));
    var maximumRate = FamilyProgressionSnapshotMapper.ReadMaximumSavingsInterestRate(storedProgression);
    var evaluation = EvaluateProgression(
      family,
      economy,
      profileState,
      storedProgression,
      currentPeriod,
      maximumRate
    );
    var progressionState = FamilyProgressionSnapshotMapper.WriteEvaluation(
      storedProgression,
      evaluation,
      house
    );
    var saves = new List<PendingFamilyAggregateSave>();
    var profileIsWritten = false;

    switch (command.CommandType)
    {
      case "progression.refresh":
        break;
      case "progression.set-maximum-savings-rate":
        family.RequireAdministrator(command.ActorId);
        var requestedMaximum = Round(RequiredDecimal(RequireObject(command.Payload), "percent"));
        maximumRate = new SavingsInterestPolicy(new Percentage(requestedMaximum)).MaximumRate;
        evaluation = EvaluateProgression(
          family,
          economy,
          profileState,
          storedProgression,
          currentPeriod,
          maximumRate
        );
        progressionState = FamilyProgressionSnapshotMapper.WriteEvaluation(
          storedProgression,
          evaluation,
          house
        );
        break;
      case "progression.complete-week":
        family.RequireAdministrator(command.ActorId);
        Require(
          house.CloseSuccessfulWeek(
            currentPeriod,
            evaluation.FamilyEnergy,
            evaluation.Members.Select(item => item.LadiEvaluation),
            _timeProvider.GetUtcNow()
          ),
          "Family energy is too low to complete the week."
        );
        FamilyProgressionSnapshotMapper.WriteParticipantStreaks(profileState, family, house);
        progressionState = FamilyProgressionSnapshotMapper.WriteEvaluation(
          progressionState,
          evaluation,
          house
        );
        saves.Add(CreateSave(profile, profileState, command.ActorId));
        profileIsWritten = true;
        break;
      case "progression.fail-week":
        family.RequireAdministrator(command.ActorId);
        house.CloseFailedWeek(
          currentPeriod,
          evaluation.FamilyEnergy,
          evaluation.Members.Select(item => item.LadiEvaluation),
          _timeProvider.GetUtcNow()
        );
        FamilyProgressionSnapshotMapper.WriteParticipantStreaks(profileState, family, house);
        progressionState = FamilyProgressionSnapshotMapper.WriteEvaluation(
          progressionState,
          evaluation,
          house
        );
        saves.Add(CreateSave(profile, profileState, command.ActorId));
        profileIsWritten = true;
        break;
      default:
        throw new FamilyCommandException("The progression command is not supported.");
    }

    saves.Add(CreateProgressionSave(progression, progressionState, command.ActorId));
    var revisionGuards = new List<FamilyAggregateRevisionGuard>
    {
      FamilyAggregateRevisionGuard.FromSnapshot(contributions),
    };

    if (!profileIsWritten)
    {
      revisionGuards.Add(FamilyAggregateRevisionGuard.FromSnapshot(profile));
    }

    var changed = await _repository.SaveBatchAsync(saves, revisionGuards, cancellationToken);

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
    var storedProgression = RequireObject(progression.State);
    var family = FamilyProfileSnapshotMapper.Rehydrate(familyId, profileState);
    var economy = FamilyEconomySnapshotMapper.RehydrateContributions(family, contributionState);
    var house = RestoreHouse(family, profileState, storedProgression);
    var period = IsoWeekPeriod.For(family.TimeZone.LocalDate(_timeProvider.GetUtcNow()));
    var evaluation = EvaluateProgression(
      family,
      economy,
      profileState,
      storedProgression,
      period,
      FamilyProgressionSnapshotMapper.ReadMaximumSavingsInterestRate(storedProgression)
    );
    var state = FamilyProgressionSnapshotMapper.WriteEvaluation(storedProgression, evaluation, house);

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

    return new PendingFamilyAggregateSave(command, progression.SchemaVersion);
  }

  private static FamilyHouse RestoreHouse(
    Family family,
    JsonObject profile,
    JsonObject progression
  )
  {
    var house = new FamilyHouse();
    var storedStreaks = FamilyProgressionSnapshotMapper.ReadStoredWeeklyStreaks(profile);

    foreach (var member in family.Members)
    {
      house.RegisterParticipant(
        member.Id,
        FamilyProgressionSnapshotMapper.IsWeeklyParticipant(member),
        storedStreaks.TryGetValue(member.Id, out var storedStreak) ? storedStreak : 0
      );
    }

    house.Restore(
      FamilyProgressionSnapshotMapper.ReadHouseLevel(progression),
      FamilyProgressionSnapshotMapper.ReadCompletedWeeklyStreak(progression)
    );
    FamilyProgressionSnapshotMapper.RestoreClosedWeeks(progression, house);

    return house;
  }

  private static FamilyProgressionEvaluation EvaluateProgression(
    Family family,
    FamilyEconomy economy,
    JsonObject profile,
    JsonObject progression,
    IsoWeekPeriod period,
    Percentage maximumRate
  )
  {
    var weeklyTarget = FamilyProgressionSnapshotMapper.ReadCurrentWeekTarget(progression);
    var storedStreaks = FamilyProgressionSnapshotMapper.ReadStoredWeeklyStreaks(profile);
    var progressionPolicy = new LadiProgressionPolicy();
    var streakPolicy = new PersonalStreakPolicy();
    var energyPolicy = new HouseEnergyPolicy();
    var interestPolicy = new SavingsInterestPolicy(maximumRate);
    var memberEvaluations = new List<MemberProgressionEvaluation>();

    foreach (var member in family.Members.Where(FamilyProgressionSnapshotMapper.IsWeeklyParticipant))
    {
      var approvedDates = economy.Contributions
        .Where(item => item.AssigneeId == member.Id)
        .Where(item => item.Status == ContributionStatus.Approved && item.ApprovedAt is not null)
        .Select(item => family.TimeZone.LocalDate(item.ApprovedAt!.Value))
        .Where(period.Contains)
        .Distinct()
        .Order()
        .ToArray();
      var streakDays = streakPolicy.Calculate(
        storedStreaks.TryGetValue(member.Id, out var completedStreak) ? completedStreak : 0,
        weeklyTarget,
        approvedDates.Length
      );
      var participation = new PersonalParticipation(
        streakDays,
        ContributionParticipationPolicy.Completion(economy.Contributions, member.Id).Value,
        ContributionParticipationPolicy.AverageRating(economy.Contributions, member.Id)
      );
      var value = progressionPolicy.Calculate(participation);
      var stage = progressionPolicy.DetermineStage(value.Capped);
      var personalEvaluation = new PersonalLadiEvaluation(
        member.Id,
        period.Year,
        period.Week,
        participation,
        value,
        stage
      );
      memberEvaluations.Add(
        new MemberProgressionEvaluation(
          personalEvaluation,
          approvedDates,
          energyPolicy.ContributionFor(stage),
          interestPolicy.RateFor(personalEvaluation)
        )
      );
    }

    var energy = energyPolicy.Calculate(
      memberEvaluations.Select(item => item.LadiEvaluation.Stage)
    );

    return new FamilyProgressionEvaluation(
      memberEvaluations,
      energy,
      energyPolicy.VisualLevel(energy),
      maximumRate
    );
  }

}
