using System;
using System.Threading;
using System.Threading.Tasks;

namespace LadirchenApp.Application.Aggregates;

public sealed partial class GetFamilyAggregateQueryHandler
{
  private readonly IFamilyAggregateSnapshotRepository _repository;
  private readonly TimeProvider _timeProvider;

  public GetFamilyAggregateQueryHandler(IFamilyAggregateSnapshotRepository repository, TimeProvider timeProvider)
  {
    _repository = repository;
    _timeProvider = timeProvider;
  }

  public async Task<FamilyAggregateSnapshot?> HandleAsync(
    GetFamilyAggregateQuery query,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(query);

    if (!FamilyAggregateSchema.TryGetVersion(query.AggregateType, out _))
    {
      throw new ArgumentException("The aggregate type is not supported.", nameof(query));
    }

    var snapshot = await _repository.GetAsync(query.FamilyId, query.AggregateType, cancellationToken);

    if (snapshot is null)
    {
      return null;
    }

    return query.AggregateType switch
    {
      "contributions" => await ProjectContributionsAsync(query, snapshot, cancellationToken),
      "family-profile" => ProjectFamilyPermissions(snapshot),
      "family-progression" => ProjectProgressionCapabilities(snapshot),
      "home-customization" => await ProjectHomeAsync(query, snapshot, cancellationToken),
      "reward-shop" => await ProjectRewardShopAsync(query, snapshot, cancellationToken),
      "savings" => await ProjectSavingsAsync(query, snapshot, cancellationToken),
      _ => snapshot,
    };
  }

  private async Task<FamilyAggregateSnapshot> ProjectContributionsAsync(
    GetFamilyAggregateQuery query,
    FamilyAggregateSnapshot snapshot,
    CancellationToken cancellationToken
  )
  {
    var profile = await _repository.GetAsync(query.FamilyId, "family-profile", cancellationToken);

    return profile is null ? snapshot : ProjectContributionRewards(snapshot, profile);
  }

  private async Task<FamilyAggregateSnapshot> ProjectSavingsAsync(
    GetFamilyAggregateQuery query,
    FamilyAggregateSnapshot snapshot,
    CancellationToken cancellationToken
  )
  {
    var related = await Task.WhenAll(
      _repository.GetAsync(query.FamilyId, "family-progression", cancellationToken),
      _repository.GetAsync(query.FamilyId, "family-profile", cancellationToken),
      _repository.GetAsync(query.FamilyId, "reward-shop", cancellationToken)
    );
    var progression = related[0];
    var profile = related[1];
    var rewardShop = related[2];

    return progression is null
      ? snapshot
      : ProjectSavingsReadModel(snapshot, progression, profile, rewardShop);
  }

  private async Task<FamilyAggregateSnapshot> ProjectRewardShopAsync(
    GetFamilyAggregateQuery query,
    FamilyAggregateSnapshot snapshot,
    CancellationToken cancellationToken
  )
  {
    var related = await Task.WhenAll(
      _repository.GetAsync(query.FamilyId, "family-profile", cancellationToken),
      _repository.GetAsync(query.FamilyId, "savings", cancellationToken)
    );
    var profile = related[0];
    var savings = related[1];

    return profile is null || savings is null ? snapshot : ProjectRewardShop(snapshot, profile, savings);
  }

  private async Task<FamilyAggregateSnapshot> ProjectHomeAsync(
    GetFamilyAggregateQuery query,
    FamilyAggregateSnapshot snapshot,
    CancellationToken cancellationToken
  )
  {
    var related = await Task.WhenAll(
      _repository.GetAsync(query.FamilyId, "family-progression", cancellationToken),
      _repository.GetAsync(query.FamilyId, "family-profile", cancellationToken),
      _repository.GetAsync(query.FamilyId, "savings", cancellationToken),
      _repository.GetAsync(query.FamilyId, "reward-shop", cancellationToken)
    );
    var progression = related[0];
    var profile = related[1];
    var savings = related[2];
    var rewardShop = related[3];

    return progression is null || profile is null || savings is null || rewardShop is null
      ? snapshot
      : ProjectHomeCatalog(snapshot, progression, profile, savings, rewardShop);
  }
}
