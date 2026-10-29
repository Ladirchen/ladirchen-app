using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class ShopRewardPresentationEntity
{
  public Guid FamilyId { get; set; }
  public string RewardId { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string Icon { get; set; } = string.Empty;
  public string Conditions { get; set; } = string.Empty;
  public FamilyEconomyRootEntity Economy { get; set; } = null!;
}
