using System;
using System.Collections.Generic;
using System.Linq;

namespace LadirchenApp.Application.Persistence;

public sealed record FamilyEconomyPresentationState
{
  public FamilyEconomyPresentationState(
    IEnumerable<ContributionPresentationState> contributions,
    IEnumerable<SavingGoalPresentationState> savingGoals,
    IEnumerable<ShopRewardPresentationState> shopRewards,
    IEnumerable<GuardianGiftPresentationState> guardianGifts
  )
  {
    ArgumentNullException.ThrowIfNull(contributions);
    ArgumentNullException.ThrowIfNull(savingGoals);
    ArgumentNullException.ThrowIfNull(shopRewards);
    ArgumentNullException.ThrowIfNull(guardianGifts);
    Contributions = contributions.ToArray();
    SavingGoals = savingGoals.ToArray();
    ShopRewards = shopRewards.ToArray();
    GuardianGifts = guardianGifts.ToArray();
  }

  public IReadOnlyList<ContributionPresentationState> Contributions { get; }
  public IReadOnlyList<SavingGoalPresentationState> SavingGoals { get; }
  public IReadOnlyList<ShopRewardPresentationState> ShopRewards { get; }
  public IReadOnlyList<GuardianGiftPresentationState> GuardianGifts { get; }

  public static FamilyEconomyPresentationState Empty { get; } = new([], [], [], []);
}
