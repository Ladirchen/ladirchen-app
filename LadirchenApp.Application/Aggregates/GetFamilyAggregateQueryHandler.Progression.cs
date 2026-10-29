using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Commands;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed partial class GetFamilyAggregateQueryHandler
{
  private async Task<FamilyProgressionReadModelProjection?> ProjectFamilyProgressionAsync(
    Family family,
    CancellationToken cancellationToken
  )
  {
    var loadedProgression = await _progressionRepository.LoadAsync(family, cancellationToken);

    if (loadedProgression is null)
    {
      return null;
    }

    var loadedEconomy = RequiredDependency(
      await _economyRepository.LoadAsync(family, cancellationToken),
      "family economy"
    );
    var period = IsoWeekPeriod.For(family.TimeZone.LocalDate(_timeProvider.GetUtcNow()));
    var evaluation = FamilyProgressionEvaluator.Evaluate(
      family,
      loadedEconomy.Root,
      loadedProgression.Root,
      period
    );
    var energyPolicy = new HouseEnergyPolicy();
    var memberProgressions = evaluation.Members.ToDictionary(
      member => member.LadiEvaluation.MemberId,
      member => new MemberProgressionReadProjection(
        member.ApprovedDates,
        member.LadiEvaluation,
        member.EnergyContribution,
        member.SavingsInterestRate
      )
    );

    return new FamilyProgressionReadModelProjection(
      loadedProgression.Revision,
      evaluation.FamilyEnergy >= HouseEnergyPolicy.MinimumSuccessfulWeekEnergy,
      loadedProgression.Root.CompletedWeeklyStreak,
      loadedProgression.Root.CurrentWeekTarget,
      evaluation.FamilyEnergy,
      energyPolicy.Atmosphere(evaluation.FamilyEnergy),
      loadedProgression.Root.StoredLevel,
      evaluation.HouseVisualLevel,
      evaluation.MaximumSavingsInterestRate,
      HouseEnergyPolicy.MinimumSuccessfulWeekEnergy,
      memberProgressions
    );
  }
}
