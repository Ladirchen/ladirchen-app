using System.Text.Json.Serialization;

using LadirchenApp.Application.Aggregates;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record FamilyProfileReadModelResponse(
  [property: JsonRequired] string ReadModelType,
  [property: JsonRequired] long Revision,
  [property: JsonRequired] FamilyProfileReadModelState State
)
{
  public static FamilyProfileReadModelResponse FromProjection(
    FamilyProfileReadModelProjection projection
  )
  {
    return FamilyReadModelResponseMapper.Map(projection);
  }
}
