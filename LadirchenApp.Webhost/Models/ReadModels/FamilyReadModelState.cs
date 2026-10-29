using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using LadirchenApp.Application.Aggregates;

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

  public static TState Deserialize<TState>(FamilyAggregateSnapshot snapshot)
    where TState : notnull
  {
    ArgumentNullException.ThrowIfNull(snapshot);

    return snapshot.State.Deserialize<TState>(_serializerOptions)
      ?? throw new InvalidOperationException($"The {snapshot.AggregateType} read model has no state.");
  }
}
