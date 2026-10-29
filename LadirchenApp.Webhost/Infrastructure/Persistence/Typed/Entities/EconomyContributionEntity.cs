using System;
using System.Collections.Generic;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class EconomyContributionEntity
{
  public Guid FamilyId { get; set; }
  public string ContributionId { get; set; } = string.Empty;
  public string Title { get; set; } = string.Empty;
  public int Kind { get; set; }
  public decimal Reward { get; set; }
  public decimal EnergyPercent { get; set; }
  public string? AssigneeId { get; set; }
  public int Status { get; set; }
  public int? Rating { get; set; }
  public decimal? EarnedBaseReward { get; set; }
  public decimal? EarnedRatingBonus { get; set; }
  public decimal? EarnedPromotionMultiplier { get; set; }
  public DateTimeOffset? ApprovedAt { get; set; }
  public FamilyEconomyRootEntity Economy { get; set; } = null!;
  public ICollection<EconomyContributionPartnerEntity> InvitedPartners { get; set; } =
    new List<EconomyContributionPartnerEntity>();
}
