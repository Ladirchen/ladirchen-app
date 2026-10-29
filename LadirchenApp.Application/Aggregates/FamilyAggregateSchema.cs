using System;
using System.Collections.Generic;

namespace LadirchenApp.Application.Aggregates;

public static class FamilyAggregateSchema
{
  private static readonly IReadOnlyDictionary<string, int> _schemaVersions =
    new Dictionary<string, int>(StringComparer.Ordinal)
    {
      ["contributions"] = 2,
      ["family-profile"] = 2,
      ["family-progression"] = 4,
      ["home-customization"] = 3,
      ["reward-shop"] = 1,
      ["savings"] = 4,
    };

  public static int GetVersion(string aggregateType)
  {
    if (!_schemaVersions.TryGetValue(aggregateType, out var schemaVersion))
    {
      throw new ArgumentException("The aggregate type is not supported.", nameof(aggregateType));
    }

    return schemaVersion;
  }

  public static bool TryGetVersion(string aggregateType, out int schemaVersion)
  {
    return _schemaVersions.TryGetValue(aggregateType, out schemaVersion);
  }
}
