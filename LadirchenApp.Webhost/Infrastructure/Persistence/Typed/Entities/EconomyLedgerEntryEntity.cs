using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class EconomyLedgerEntryEntity
{
  public Guid FamilyId { get; set; }
  public Guid EntryId { get; set; }
  public string OperationId { get; set; } = string.Empty;
  public string MemberId { get; set; } = string.Empty;
  public string? GoalId { get; set; }
  public int Kind { get; set; }
  public decimal Amount { get; set; }
  public int SourceKind { get; set; }
  public string SourceId { get; set; } = string.Empty;
  public string Reason { get; set; } = string.Empty;
  public DateTimeOffset OccurredAtUtc { get; set; }
  public string? InitiatorMemberId { get; set; }
  public FamilyEconomyRootEntity Economy { get; set; } = null!;
}
