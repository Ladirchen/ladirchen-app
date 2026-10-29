using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class EconomyUsedContributionIdEntity
{
  public Guid FamilyId { get; set; }
  public string ContributionId { get; set; } = string.Empty;
  public FamilyEconomyRootEntity Economy { get; set; } = null!;
}
