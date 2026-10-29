using System.Text.Json.Serialization;

using LadirchenApp.Application.Aggregates;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record ContributionsReadModelResponse(
  [property: JsonRequired] string ReadModelType,
  [property: JsonRequired] long Revision,
  [property: JsonRequired] ContributionsReadModelState State
)
{
  public static ContributionsReadModelResponse FromSnapshot(FamilyAggregateSnapshot snapshot)
  {
    return new ContributionsReadModelResponse(
      snapshot.AggregateType,
      snapshot.Revision,
      FamilyReadModelState.Deserialize<ContributionsReadModelState>(snapshot)
    );
  }
}
