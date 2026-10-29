using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class EconomyContributionPartnerEntity
{
  public Guid FamilyId { get; set; }
  public string ContributionId { get; set; } = string.Empty;
  public string MemberId { get; set; } = string.Empty;
  public EconomyContributionEntity Contribution { get; set; } = null!;
}
