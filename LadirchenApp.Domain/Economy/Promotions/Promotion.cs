using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed class Promotion : Entity<PromotionId>
{
  internal Promotion(PromotionId id, ContributionId contributionId, RewardMultiplier multiplier,
    LadirchenAmount teamworkBonus, PromotionDeadline deadline) : base(id)
  {
    ArgumentNullException.ThrowIfNull(contributionId);
    ArgumentNullException.ThrowIfNull(multiplier);
    ArgumentNullException.ThrowIfNull(teamworkBonus);
    ArgumentNullException.ThrowIfNull(deadline);
    ContributionId = contributionId;
    Multiplier = multiplier;
    TeamworkBonus = teamworkBonus;
    Deadline = deadline;
  }
  public ContributionId ContributionId
  {
    get;
  }
  public RewardMultiplier Multiplier
  {
    get;
  }
  public LadirchenAmount TeamworkBonus
  {
    get;
  }
  public PromotionDeadline Deadline
  {
    get;
  }
  public bool Active { get; private set; } = true;
  internal void SetActive(bool active)
  {
    Active = active;
  }
}