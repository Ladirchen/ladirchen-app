using System;
using System.Collections.Generic;
using System.Linq;
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

internal sealed class FamilyProgressionApplicationService
{
  private readonly IFamilyAggregateSnapshotRepository _repository;
  private readonly TimeProvider _timeProvider;

  public FamilyProgressionApplicationService(
    IFamilyAggregateSnapshotRepository repository,
    TimeProvider timeProvider
  )
  {
    _repository = repository;
    _timeProvider = timeProvider;
  }

  public async Task<PendingFamilyAggregateSave> CreateSaveAsync(
    FamilyId familyId,
    JsonObject profileState,
    JsonObject contributionState,
    FamilyMemberId actorId,
    CancellationToken cancellationToken
  )
  {
    var progression = await _repository.GetAsync(
      familyId,
      "family-progression",
      cancellationToken
    ) ?? throw new FamilyCommandException(
      "The family-progression aggregate has not been initialized."
    );
    var storedProgression = FamilyCommandAreaHandler.RequireObject(progression.State);
    var family = FamilyProfileSnapshotMapper.Rehydrate(familyId, profileState);
    var economy = FamilyEconomySnapshotMapper.RehydrateContributions(family, contributionState);
    var house = RestoreHouse(family, profileState, storedProgression);
    var period = IsoWeekPeriod.For(family.TimeZone.LocalDate(_timeProvider.GetUtcNow()));
    var evaluation = Evaluate(
      family,
      economy,
      profileState,
      storedProgression,
      period,
      FamilyProgressionSnapshotMapper.ReadMaximumSavingsInterestRate(storedProgression)
    );
    var state = FamilyProgressionSnapshotMapper.WriteEvaluation(
      storedProgression,
      evaluation,
      house
    );

    return CreateSave(progression, state, actorId);
  }

  public static PendingFamilyAggregateSave CreateSave(
    FamilyAggregateSnapshot progression,
    JsonObject state,
    FamilyMemberId actorId
  )
  {
    return FamilyCommandAreaHandler.CreateSave(progression, state, actorId);
  }

  public static FamilyHouse RestoreHouse(
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

  public static FamilyProgressionEvaluation Evaluate(
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
