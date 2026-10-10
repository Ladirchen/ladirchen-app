using System;

namespace LadirchenApp.Prototypes;

public interface IDomainEvent
{
  Guid EventId { get; }

  DateTimeOffset OccurredAtUtc { get; }
}
