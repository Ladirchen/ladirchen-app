using System.Text.Json.Serialization;

using LadirchenApp.Application.Aggregates;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record SavingsReadModelResponse(
  [property: JsonRequired] string ReadModelType,
  [property: JsonRequired] long Revision,
  [property: JsonRequired] SavingsReadModelState State
)
{
  public static SavingsReadModelResponse FromSnapshot(FamilyAggregateSnapshot snapshot)
  {
    return new SavingsReadModelResponse(
      snapshot.AggregateType,
      snapshot.Revision,
      FamilyReadModelState.Deserialize<SavingsReadModelState>(snapshot)
    );
  }
}
