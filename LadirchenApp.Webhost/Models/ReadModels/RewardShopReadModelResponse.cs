using System.Text.Json.Serialization;

using LadirchenApp.Application.Aggregates;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record RewardShopReadModelResponse(
  [property: JsonRequired] string ReadModelType,
  [property: JsonRequired] long Revision,
  [property: JsonRequired] RewardShopReadModelState State
)
{
  public static RewardShopReadModelResponse FromProjection(RewardShopReadModelProjection projection)
  {
    return FamilyReadModelResponseMapper.Map(projection);
  }
}
