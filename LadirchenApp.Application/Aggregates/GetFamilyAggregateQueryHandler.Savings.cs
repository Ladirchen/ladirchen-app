namespace LadirchenApp.Application.Aggregates;

using LadirchenApp.Domain.Shared;

public sealed partial class GetFamilyAggregateQueryHandler
{
  private static FamilyAggregateSnapshot ProjectSavingsReadModel(
    FamilyAggregateSnapshot savings,
    FamilyAggregateSnapshot progression,
    FamilyAggregateSnapshot profile,
    FamilyAggregateSnapshot rewardShop,
    FamilyMemberId viewerId
  )
  {
    var state = SavingsReadModelSnapshotMapper.Read(savings, progression, profile, rewardShop);
    var projection = SavingsReadModelProjector.Project(state, viewerId);

    return SavingsReadModelSnapshotMapper.Write(savings, projection);
  }
}
