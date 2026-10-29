using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class ContributionPresentationEntity
{
  public Guid FamilyId { get; set; }
  public string ContributionId { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string Icon { get; set; } = string.Empty;
  public bool RewardCelebrated { get; set; }
  public FamilyEconomyRootEntity Economy { get; set; } = null!;
}
