using System;

namespace LadirchenApp.Application.Aggregates;

public sealed class FamilyAggregateConflictException : Exception
{
  public FamilyAggregateConflictException(string aggregateType, long expectedRevision, long actualRevision)
    : base(
      $"Aggregate '{aggregateType}' expected revision {expectedRevision}, but the current revision is {actualRevision}."
    )
  {
    AggregateType = aggregateType;
    ExpectedRevision = expectedRevision;
    ActualRevision = actualRevision;
  }

  public string AggregateType
  {
    get;
  }

  public long ExpectedRevision
  {
    get;
  }

  public long ActualRevision
  {
    get;
  }
}
