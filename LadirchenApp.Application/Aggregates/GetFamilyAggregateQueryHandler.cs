using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Commands;
using LadirchenApp.Domain.Shared;

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

    if (!snapshots.TryGetValue("family-profile", out var profile))
    {
      return null;
    }

    var family = FamilyProfileSnapshotMapper.Rehydrate(query.FamilyId, ReadProfileState(profile));
    family.RequireActorAccess(query.AuthenticatedMemberId, query.ViewerId);

    return query.AggregateType switch
    {
      "contributions" => ProjectContributions(snapshot, snapshots),
      "family-profile" => ProjectFamilyPermissions(
        snapshot,
        query.AuthenticatedMemberId,
        query.ViewerId
      ),
      "family-progression" => ProjectProgressionCapabilities(snapshot),
      "home-customization" => ProjectHome(snapshot, snapshots, query.AuthenticatedMemberId),
      "reward-shop" => ProjectRewardShopSnapshot(
        snapshot,
        snapshots,
        query.AuthenticatedMemberId,
        query.ViewerId
      ),
      "savings" => ProjectSavings(snapshot, snapshots, query.AuthenticatedMemberId),
      _ => snapshot,
    };
  }

  private static IReadOnlyCollection<string> GetRequiredAggregateTypes(string aggregateType)
  {
    string[] required = aggregateType switch
    {
      "contributions" => ["contributions", "family-profile"],
      "home-customization" =>
        ["home-customization", "family-progression", "family-profile", "savings", "reward-shop"],
      "reward-shop" => ["reward-shop", "family-profile", "savings"],
      "savings" => ["savings", "family-progression", "family-profile", "reward-shop"],
      _ => [aggregateType],
    };

    return required.Append("family-profile").Distinct(StringComparer.Ordinal).ToArray();
  }

  private FamilyAggregateSnapshot ProjectContributions(
    FamilyAggregateSnapshot snapshot,
    IReadOnlyDictionary<string, FamilyAggregateSnapshot> snapshots
  )
  {
    snapshots.TryGetValue("family-profile", out var profile);

    return ProjectContributionRewards(
      snapshot,
      profile ?? throw new InvalidOperationException("The family profile read-model dependency is missing.")
    );
  }

  private static FamilyAggregateSnapshot ProjectSavings(
    FamilyAggregateSnapshot snapshot,
    IReadOnlyDictionary<string, FamilyAggregateSnapshot> snapshots,
    FamilyMemberId viewerId
  )
  {
    snapshots.TryGetValue("family-progression", out var progression);
    snapshots.TryGetValue("family-profile", out var profile);
    snapshots.TryGetValue("reward-shop", out var rewardShop);

    return ProjectSavingsReadModel(
      snapshot,
      progression ?? throw new InvalidOperationException("The family progression read-model dependency is missing."),
      profile ?? throw new InvalidOperationException("The family profile read-model dependency is missing."),
      rewardShop ?? throw new InvalidOperationException("The reward shop read-model dependency is missing."),
      viewerId
    );
  }

  private FamilyAggregateSnapshot ProjectRewardShopSnapshot(
    FamilyAggregateSnapshot snapshot,
    IReadOnlyDictionary<string, FamilyAggregateSnapshot> snapshots,
    FamilyMemberId authenticatedMemberId,
    FamilyMemberId viewerId
  )
  {
    snapshots.TryGetValue("family-profile", out var profile);
    snapshots.TryGetValue("savings", out var savings);

    return ProjectRewardShop(
      snapshot,
      profile ?? throw new InvalidOperationException("The family profile read-model dependency is missing."),
      savings ?? throw new InvalidOperationException("The savings read-model dependency is missing."),
      authenticatedMemberId,
      viewerId
    );
  }

  private static FamilyAggregateSnapshot ProjectHome(
    FamilyAggregateSnapshot snapshot,
    IReadOnlyDictionary<string, FamilyAggregateSnapshot> snapshots,
    FamilyMemberId viewerId
  )
  {
    snapshots.TryGetValue("family-progression", out var progression);
    snapshots.TryGetValue("family-profile", out var profile);
    snapshots.TryGetValue("savings", out var savings);
    snapshots.TryGetValue("reward-shop", out var rewardShop);

    return ProjectHomeCatalog(
      snapshot,
      progression ?? throw new InvalidOperationException("The family progression read-model dependency is missing."),
      profile ?? throw new InvalidOperationException("The family profile read-model dependency is missing."),
      savings ?? throw new InvalidOperationException("The savings read-model dependency is missing."),
      rewardShop ?? throw new InvalidOperationException("The reward shop read-model dependency is missing."),
      viewerId
    );
  }
}
