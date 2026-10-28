using System;

using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Shared;

public abstract record BusinessEvent(DateTimeOffset OccurredAtUtc) : IDomainEvent
{
  public Guid EventId { get; } = Guid.NewGuid();
}