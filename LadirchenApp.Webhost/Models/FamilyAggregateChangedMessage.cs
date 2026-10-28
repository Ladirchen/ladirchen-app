using System;

using LadirchenApp.Application.Aggregates;

namespace LadirchenApp.Webhost.Models;

public sealed record FamilyAggregateChangedMessage(
  string FamilyId,
  string AggregateType,
  long Revision,
  string UpdatedBy,
  DateTimeOffset UpdatedAt
)
{
  public static FamilyAggregateChangedMessage FromSnapshot(FamilyAggregateSnapshot snapshot)
  {
    return new FamilyAggregateChangedMessage(
      snapshot.FamilyId.Value.ToString(),
      snapshot.AggregateType,
      snapshot.Revision,
      snapshot.UpdatedBy.Value,
      snapshot.UpdatedAt
    );
  }
}
