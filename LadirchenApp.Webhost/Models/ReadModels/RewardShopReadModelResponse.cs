using System.Text.Json.Serialization;

using LadirchenApp.Application.Aggregates;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record RewardShopReadModelResponse(
  [property: JsonRequired] string ReadModelType,
  [property: JsonRequired] long Revision,
  [property: JsonRequired] RewardShopReadModelState State
)
{
  public static RewardShopReadModelResponse FromSnapshot(FamilyAggregateSnapshot snapshot)
  {
    return new RewardShopReadModelResponse(
      snapshot.AggregateType,
      snapshot.Revision,
      FamilyReadModelState.Deserialize<RewardShopReadModelState>(snapshot)
    );
  }
}
