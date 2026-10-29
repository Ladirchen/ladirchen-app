using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed record FamilyAggregateRevisionGuard(
  FamilyId FamilyId,
  string AggregateType,
  long ExpectedRevision
)
{
  public static FamilyAggregateRevisionGuard FromSnapshot(FamilyAggregateSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);

    return new FamilyAggregateRevisionGuard(
      snapshot.FamilyId,
      snapshot.AggregateType,
      snapshot.Revision
    );
  }
}