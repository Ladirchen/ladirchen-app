using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Persistence;
using LadirchenApp.Application.Commands;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed partial class GetFamilyAggregateQueryHandler
{
  private async Task<SavingsFamilyReadModelProjection?> ProjectSavingsAsync(
    Family family,
    FamilyMemberId viewerId,
    CancellationToken cancellationToken
  )
  {
    var loadedEconomy = await _economyRepository.LoadAsync(family, cancellationToken);

    if (loadedEconomy is null)
    {
      return null;
    }

    var loadedProgression = RequiredDependency(
      await _progressionRepository.LoadAsync(family, cancellationToken),
      "family progression"
    );

    return ProjectSavings(family, loadedEconomy, loadedProgression, viewerId);
  }

  private SavingsFamilyReadModelProjection ProjectSavings(
    Family family,
    LoadedFamilyEconomy loadedEconomy,
    LoadedAggregate<Domain.House.FamilyHouse> loadedProgression,
    FamilyMemberId viewerId
  )
  {
    var period = IsoWeekPeriod.For(family.TimeZone.LocalDate(_timeProvider.GetUtcNow()));
    var progression = FamilyProgressionEvaluator.Evaluate(
      family,
      loadedEconomy.Root,
      loadedProgression.Root,
      period
    );
    var stages = progression.Members.ToDictionary(
      member => member.LadiEvaluation.MemberId,
      member => member.LadiEvaluation.Stage
    );
    var storedBalanceMemberIds = loadedEconomy.Root.Wallets
      .Select(wallet => wallet.Id)
      .ToHashSet();
    var calculatedState = SavingsReadModelProjector.Project(
      new SavingsReadModelState(
        family,
        loadedEconomy.Root,
        _timeProvider.GetUtcNow(),
        loadedProgression.Root.MaximumSavingsInterestRate,
        stages,
        storedBalanceMemberIds
      ),
      viewerId
    );

    return new SavingsFamilyReadModelProjection(
      loadedEconomy.Revision,
      loadedEconomy.Root,
      loadedEconomy.Presentation,
      calculatedState,
      family.Currency,
      family.ExchangeRate
    );
  }
}
