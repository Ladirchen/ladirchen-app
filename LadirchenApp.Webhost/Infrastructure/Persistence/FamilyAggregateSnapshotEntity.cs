using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence;

public sealed class FamilyAggregateSnapshotEntity
{
  public Guid FamilyId { get; set; }
  public string AggregateType { get; set; } = string.Empty;
  public int SchemaVersion { get; set; }
  public long Revision { get; set; }
  public string StateJson { get; set; } = string.Empty;
  public string UpdatedBy { get; set; } = string.Empty;
  public DateTimeOffset UpdatedAt { get; set; }
}
