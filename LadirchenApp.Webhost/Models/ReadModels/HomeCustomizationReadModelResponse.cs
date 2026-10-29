using System.Text.Json.Serialization;

using LadirchenApp.Application.Aggregates;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record HomeCustomizationReadModelResponse(
  [property: JsonRequired] string ReadModelType,
  [property: JsonRequired] long Revision,
  [property: JsonRequired] HomeCustomizationReadModelState State
)
{
  public static HomeCustomizationReadModelResponse FromSnapshot(FamilyAggregateSnapshot snapshot)
  {
    return new HomeCustomizationReadModelResponse(
      snapshot.AggregateType,
      snapshot.Revision,
      FamilyReadModelState.Deserialize<HomeCustomizationReadModelState>(snapshot)
    );
  }
}
