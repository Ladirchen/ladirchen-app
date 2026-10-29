using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence;

public sealed class FamilyAggregateOutboxMessageEntity
{
  public Guid MessageId { get; set; }
  public Guid? CommandId { get; set; }
  public Guid FamilyId { get; set; }
  public string AggregateType { get; set; } = string.Empty;
  public long Revision { get; set; }
  public string UpdatedBy { get; set; } = string.Empty;
  public DateTimeOffset UpdatedAt { get; set; }
  public DateTimeOffset OccurredAt { get; set; }
  public DateTimeOffset? PublishedAt { get; set; }
  public DateTimeOffset? NextAttemptAt { get; set; }
  public int PublishAttempts { get; set; }
  public string? LastError { get; set; }
}
