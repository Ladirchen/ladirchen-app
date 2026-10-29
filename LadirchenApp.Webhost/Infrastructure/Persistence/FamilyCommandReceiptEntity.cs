using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence;

public sealed class FamilyCommandReceiptEntity
{
  public Guid CommandId { get; set; }
  public Guid FamilyId { get; set; }
  public string ActorId { get; set; } = string.Empty;
  public string CommandType { get; set; } = string.Empty;
  public string RequestFingerprint { get; set; } = string.Empty;
  public string? ResultJson { get; set; }
  public DateTimeOffset CreatedAt { get; set; }
  public DateTimeOffset? CompletedAt { get; set; }
}
