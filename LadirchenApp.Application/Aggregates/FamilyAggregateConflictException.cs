using System;

namespace LadirchenApp.Application.Aggregates;

public sealed class FamilyAggregateConflictException : Exception
{
  public FamilyAggregateConflictException(
    string aggregateType,
    long expectedRevision,
    long actualRevision,
    Exception? innerException = null
  )
    : this(
      $"Aggregate '{aggregateType}' expected revision {expectedRevision}, but the current revision is {actualRevision}.",
      aggregateType,
      expectedRevision,
      actualRevision,
      innerException
    )
  {
  }

  private FamilyAggregateConflictException(
    string message,
    string? aggregateType,
    long? expectedRevision,
    long? actualRevision,
    Exception? innerException
  )
    : base(message, innerException)
  {
    AggregateType = aggregateType;
    ExpectedRevision = expectedRevision;
    ActualRevision = actualRevision;
  }

  public string? AggregateType
  {
    get;
  }

  public long? ExpectedRevision
  {
    get;
  }

  public long? ActualRevision
  {
    get;
  }

  public static FamilyAggregateConflictException ConcurrentChange(Exception innerException)
  {
    ArgumentNullException.ThrowIfNull(innerException);

    return new FamilyAggregateConflictException(
      "The family state changed concurrently. Reload it and retry the command.",
      null,
      null,
      null,
      innerException
    );
  }

  public static FamilyAggregateConflictException UnknownActualRevision(
    string aggregateType,
    long expectedRevision,
    Exception innerException
  )
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(aggregateType);
    ArgumentNullException.ThrowIfNull(innerException);

    return new FamilyAggregateConflictException(
      $"Aggregate '{aggregateType}' expected revision {expectedRevision}, but a concurrent database change prevented reading the current revision.",
      aggregateType,
      expectedRevision,
      null,
      innerException
    );
  }
}