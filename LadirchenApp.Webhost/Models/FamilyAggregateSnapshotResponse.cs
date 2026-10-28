using System;
using System.Text.Json;

using LadirchenApp.Application.Aggregates;

namespace LadirchenApp.Webhost.Models;

public sealed record FamilyAggregateSnapshotResponse(
  string FamilyId,
  string AggregateType,
  int SchemaVersion,
  long Revision,
  JsonElement State,
  string UpdatedBy,
  DateTimeOffset UpdatedAt
)
{
  public static FamilyAggregateSnapshotResponse FromSnapshot(FamilyAggregateSnapshot snapshot)
  {
    return new FamilyAggregateSnapshotResponse(
      snapshot.FamilyId.Value.ToString(),
      snapshot.AggregateType,
      snapshot.SchemaVersion,
      snapshot.Revision,
      snapshot.State,
      snapshot.UpdatedBy.Value,
      snapshot.UpdatedAt
    );
  }
}
