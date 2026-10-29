using System;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Family;

namespace LadirchenApp.Application.Aggregates;

public sealed partial class GetFamilyAggregateQueryHandler
{
  private readonly IFamilyRepository _familyRepository;
  private readonly IFamilyEconomyRepository _economyRepository;
  private readonly IFamilyProgressionRepository _progressionRepository;
  private readonly IHouseCustomizationRepository _houseCustomizationRepository;
  private readonly TimeProvider _timeProvider;

  public GetFamilyAggregateQueryHandler(
    IFamilyRepository familyRepository,
    IFamilyEconomyRepository economyRepository,
    IFamilyProgressionRepository progressionRepository,
    IHouseCustomizationRepository houseCustomizationRepository,
    TimeProvider timeProvider
  )
  {
    _familyRepository = familyRepository
      ?? throw new ArgumentNullException(nameof(familyRepository));
    _economyRepository = economyRepository
      ?? throw new ArgumentNullException(nameof(economyRepository));
    _progressionRepository = progressionRepository
      ?? throw new ArgumentNullException(nameof(progressionRepository));
    _houseCustomizationRepository = houseCustomizationRepository
      ?? throw new ArgumentNullException(nameof(houseCustomizationRepository));
    _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
  }

  public async Task<IFamilyReadModelProjection?> HandleAsync(
    GetFamilyAggregateQuery query,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(query);

    if (!IsSupportedReadModel(query.AggregateType))
    {
      throw new ArgumentException("The aggregate type is not supported.", nameof(query));
    }

    var loadedFamily = await _familyRepository.LoadAsync(query.FamilyId, cancellationToken);

    if (loadedFamily is null)
    {
      return null;
    }

    var family = loadedFamily.Root;
    family.RequireActorAccess(query.AuthenticatedMemberId, query.ViewerId);

    return query.AggregateType switch
    {
      "contributions" => await ProjectContributionsAsync(family, cancellationToken),
      "family-profile" => await ProjectFamilyProfileAsync(
        loadedFamily,
        query.AuthenticatedMemberId,
        query.ViewerId,
        cancellationToken
      ),
      "family-progression" => await ProjectFamilyProgressionAsync(family, cancellationToken),
      "home-customization" => await ProjectHomeCustomizationAsync(
        family,
        query.AuthenticatedMemberId,
        cancellationToken
      ),
      "reward-shop" => await ProjectRewardShopAsync(
        family,
        query.AuthenticatedMemberId,
        query.ViewerId,
        cancellationToken
      ),
      "savings" => await ProjectSavingsAsync(
        family,
        query.AuthenticatedMemberId,
        cancellationToken
      ),
      _ => throw new ArgumentException("The aggregate type is not supported.", nameof(query)),
    };
  }

  private static T RequiredDependency<T>(T? dependency, string description)
    where T : class
  {
    return dependency
      ?? throw new InvalidOperationException($"The {description} read-model dependency is missing.");
  }

  private static bool IsSupportedReadModel(string readModelType)
  {
    return readModelType is
      "contributions"
      or "family-profile"
      or "family-progression"
      or "home-customization"
      or "reward-shop"
      or "savings";
  }
}
