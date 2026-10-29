namespace LadirchenApp.Application.Aggregates;

public sealed partial class GetFamilyAggregateQueryHandler
{
  private static FamilyAggregateSnapshot ProjectSavingsReadModel(
    FamilyAggregateSnapshot savings,
    FamilyAggregateSnapshot progression,
    FamilyAggregateSnapshot? profile,
    FamilyAggregateSnapshot? rewardShop
  )
  {
    if (profile is null)
    {
      return savings;
    }

    var state = SavingsReadModelSnapshotMapper.Read(savings, progression, profile, rewardShop);
    var projection = SavingsReadModelProjector.Project(state);

    return SavingsReadModelSnapshotMapper.Write(savings, projection);
  }
}
