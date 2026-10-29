using System.Text.Json.Serialization;

using LadirchenApp.Application.Aggregates;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record ContributionsReadModelResponse(
  [property: JsonRequired] string ReadModelType,
  [property: JsonRequired] long Revision,
  [property: JsonRequired] ContributionsReadModelState State
)
{
  public static ContributionsReadModelResponse FromProjection(
    ContributionsFamilyReadModelProjection projection
  )
  {
    return FamilyReadModelResponseMapper.Map(projection);
  }
}
