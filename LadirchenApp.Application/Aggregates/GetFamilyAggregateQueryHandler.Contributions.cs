namespace LadirchenApp.Application.Aggregates;

public sealed partial class GetFamilyAggregateQueryHandler
{
  private FamilyAggregateSnapshot ProjectContributionRewards(
    FamilyAggregateSnapshot contributions,
    FamilyAggregateSnapshot profile
  )
  {
    var state = ContributionReadModelSnapshotMapper.Read(contributions, profile);
    var projection = ContributionReadModelProjector.Project(state, _timeProvider.GetUtcNow());

    return ContributionReadModelSnapshotMapper.Write(contributions, projection);
  }
}