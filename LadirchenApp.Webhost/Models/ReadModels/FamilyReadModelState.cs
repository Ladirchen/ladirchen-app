using System.Text.Json;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

internal static class FamilyReadModelState
{
  private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
  {
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    RespectNullableAnnotations = true,
  };

  public static JsonSerializerOptions GetSerializerOptions()
  {
    return _serializerOptions;
  }
}
