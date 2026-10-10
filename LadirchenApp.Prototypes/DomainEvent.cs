using System;

namespace LadirchenApp.Prototypes;

public abstract record DomainEvent : IDomainEvent
{
  public Guid EventId { get; } = Guid.NewGuid();

  public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}