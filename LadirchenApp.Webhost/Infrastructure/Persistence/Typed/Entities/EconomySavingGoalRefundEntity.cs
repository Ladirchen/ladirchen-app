using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class EconomySavingGoalRefundEntity
{
  public Guid FamilyId { get; set; }
  public string GoalId { get; set; } = string.Empty;
  public string MemberId { get; set; } = string.Empty;
  public decimal Amount { get; set; }
  public EconomySavingGoalEntity Goal { get; set; } = null!;
}
