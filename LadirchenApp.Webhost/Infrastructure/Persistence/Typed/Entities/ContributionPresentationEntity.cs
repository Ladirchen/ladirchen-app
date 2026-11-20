using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class ContributionPresentationEntity
{
  public Guid FamilyId { get; set; }
  public string ContributionId { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string Icon { get; set; } = string.Empty;
  public string? Category { get; set; }
  public DateOnly? DueDate { get; set; }
  public string Recurrence { get; set; } = "none";
  public bool IsFocus { get; set; }
  public string? FocusPeriod { get; set; }
  public DateOnly? FocusStartDate { get; set; }
  public DateOnly? FocusEndDate { get; set; }
  public decimal FocusCompletionBonus { get; set; }
  public string? FocusId { get; set; }
  public int? PreparationMinutes { get; set; }
  public bool FocusBonusPaid { get; set; }
  public bool RewardCelebrated { get; set; }
  public FamilyEconomyRootEntity Economy { get; set; } = null!;
}
