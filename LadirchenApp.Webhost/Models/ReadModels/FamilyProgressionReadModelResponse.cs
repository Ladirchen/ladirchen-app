using System.Text.Json.Serialization;

using LadirchenApp.Application.Aggregates;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record FamilyProgressionReadModelResponse(
  [property: JsonRequired] string ReadModelType,
  [property: JsonRequired] long Revision,
  [property: JsonRequired] FamilyProgressionReadModelState State
)
{
  public static FamilyProgressionReadModelResponse FromProjection(
    FamilyProgressionReadModelProjection projection
  )
  {
    return FamilyReadModelResponseMapper.Map(projection);
  }
}
