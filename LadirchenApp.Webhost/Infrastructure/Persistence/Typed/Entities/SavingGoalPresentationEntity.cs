using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class SavingGoalPresentationEntity
{
  public Guid FamilyId { get; set; }
  public string GoalId { get; set; } = string.Empty;
  public string Icon { get; set; } = string.Empty;
  public FamilyEconomyRootEntity Economy { get; set; } = null!;
}
