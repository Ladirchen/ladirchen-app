using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal static class FamilyProgressionEvaluator
{
  public static FamilyProgressionEvaluation Evaluate(
    Family family,
    FamilyEconomy economy,
    FamilyHouse house,
    IsoWeekPeriod period
  )
  {
    ArgumentNullException.ThrowIfNull(family);
    ArgumentNullException.ThrowIfNull(economy);
    ArgumentNullException.ThrowIfNull(house);
    ArgumentNullException.ThrowIfNull(period);
    var progressionPolicy = new LadiProgressionPolicy();
    var energyPolicy = new HouseEnergyPolicy();
    var interestPolicy = new SavingsInterestPolicy(house.MaximumSavingsInterestRate);
    var memberEvaluations = new List<MemberProgressionEvaluation>();

    foreach (var member in family.WeeklyGoalParticipants)
    {
      var participation = economy.EvaluateParticipation(
        member.Id,
        period,
        house.StoredWeeklyStreakFor(member.Id),
        house.CurrentWeekTarget
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
          ApprovedDates(family, economy, member.Id, period),
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
      house.MaximumSavingsInterestRate
    );
  }

  private static IReadOnlyList<DateOnly> ApprovedDates(
    Family family,
    FamilyEconomy economy,
    FamilyMemberId memberId,
    IsoWeekPeriod period
  )
  {
    return economy.Contributions
      .Where(item => item.AssigneeId == memberId)
      .Where(item => item.Status == ContributionStatus.Approved && item.ApprovedAt is not null)
      .Select(item => family.TimeZone.LocalDate(item.ApprovedAt!.Value))
      .Where(period.Contains)
      .Distinct()
      .Order()
      .ToArray();
  }
}
