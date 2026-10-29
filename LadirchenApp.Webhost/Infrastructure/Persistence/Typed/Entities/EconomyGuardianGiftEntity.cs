using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class EconomyGuardianGiftEntity
{
  public Guid FamilyId { get; set; }
  public Guid StorageId { get; set; }
  public string? GiftId { get; set; }
  public string OwnerMemberId { get; set; } = string.Empty;
  public string? GuardianMemberId { get; set; }
  public decimal Amount { get; set; }
  public string? GoalId { get; set; }
  public int Destination { get; set; }
  public FamilyEconomyRootEntity Economy { get; set; } = null!;
}
