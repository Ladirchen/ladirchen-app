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
  public static FamilyAggregateChangedMessage FromChange(FamilyAggregateChange change)
  {
    return new FamilyAggregateChangedMessage(
      change.FamilyId.Value.ToString(),
      change.AggregateType,
      change.Revision,
      change.UpdatedBy.Value,
      change.UpdatedAt
    );
  }
}
