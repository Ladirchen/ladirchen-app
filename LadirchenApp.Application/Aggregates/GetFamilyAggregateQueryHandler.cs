using System;
using System.Collections.Generic;
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

    var snapshots = await _repository.GetManyAsync(
      query.FamilyId,
      GetRequiredAggregateTypes(query.AggregateType),
      cancellationToken
    );

    if (!snapshots.TryGetValue(query.AggregateType, out var snapshot))
    {
      return null;
    }

    return query.AggregateType switch
    {
      "contributions" => ProjectContributions(snapshot, snapshots),
      "family-profile" => ProjectFamilyPermissions(snapshot),
      "family-progression" => ProjectProgressionCapabilities(snapshot),
      "home-customization" => ProjectHome(snapshot, snapshots),
      "reward-shop" => ProjectRewardShopSnapshot(snapshot, snapshots),
      "savings" => ProjectSavings(snapshot, snapshots),
      _ => snapshot,
    };
  }

  private static IReadOnlyCollection<string> GetRequiredAggregateTypes(string aggregateType)
  {
    return aggregateType switch
    {
      "contributions" => ["contributions", "family-profile"],
      "home-customization" =>
        ["home-customization", "family-progression", "family-profile", "savings", "reward-shop"],
      "reward-shop" => ["reward-shop", "family-profile", "savings"],
      "savings" => ["savings", "family-progression", "family-profile", "reward-shop"],
      _ => [aggregateType],
    };
  }

  private FamilyAggregateSnapshot ProjectContributions(
    FamilyAggregateSnapshot snapshot,
    IReadOnlyDictionary<string, FamilyAggregateSnapshot> snapshots
  )
  {
    snapshots.TryGetValue("family-profile", out var profile);

    return profile is null ? snapshot : ProjectContributionRewards(snapshot, profile);
  }

  private static FamilyAggregateSnapshot ProjectSavings(
    FamilyAggregateSnapshot snapshot,
    IReadOnlyDictionary<string, FamilyAggregateSnapshot> snapshots
  )
  {
    snapshots.TryGetValue("family-progression", out var progression);
    snapshots.TryGetValue("family-profile", out var profile);
    snapshots.TryGetValue("reward-shop", out var rewardShop);

    return progression is null
      ? snapshot
      : ProjectSavingsReadModel(snapshot, progression, profile, rewardShop);
  }

  private FamilyAggregateSnapshot ProjectRewardShopSnapshot(
    FamilyAggregateSnapshot snapshot,
    IReadOnlyDictionary<string, FamilyAggregateSnapshot> snapshots
  )
  {
    snapshots.TryGetValue("family-profile", out var profile);
    snapshots.TryGetValue("savings", out var savings);

    return profile is null || savings is null ? snapshot : ProjectRewardShop(snapshot, profile, savings);
  }

  private static FamilyAggregateSnapshot ProjectHome(
    FamilyAggregateSnapshot snapshot,
    IReadOnlyDictionary<string, FamilyAggregateSnapshot> snapshots
  )
  {
    snapshots.TryGetValue("family-progression", out var progression);
    snapshots.TryGetValue("family-profile", out var profile);
    snapshots.TryGetValue("savings", out var savings);
    snapshots.TryGetValue("reward-shop", out var rewardShop);

    return progression is null || profile is null || savings is null || rewardShop is null
      ? snapshot
      : ProjectHomeCatalog(snapshot, progression, profile, savings, rewardShop);
  }
}