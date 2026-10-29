using System;

namespace LadirchenApp.Application.Persistence;

public sealed record TypedAggregateRevisionGuard
{
  public TypedAggregateRevisionGuard(TypedAggregateKind aggregateKind, long expectedRevision)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(expectedRevision);
    AggregateKind = aggregateKind;
    ExpectedRevision = expectedRevision;
  }

  public TypedAggregateKind AggregateKind { get; }
  public long ExpectedRevision { get; }
}
