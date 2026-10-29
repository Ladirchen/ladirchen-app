using System;
using System.Collections.Generic;

namespace LadirchenApp.Application.Aggregates;

public static class FamilyAggregateSchema
{
  private static readonly IReadOnlyDictionary<string, int> SchemaVersions =
    new Dictionary<string, int>(StringComparer.Ordinal)
    {
      ["contributions"] = 1,
      ["family-profile"] = 2,
      ["family-progression"] = 3,
      ["home-customization"] = 3,
      ["reward-shop"] = 1,
      ["savings"] = 1,
    };

  public static bool TryGetVersion(string aggregateType, out int schemaVersion)
  {
    return SchemaVersions.TryGetValue(aggregateType, out schemaVersion);
  }
}
