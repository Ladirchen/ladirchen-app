using System.Collections.Generic;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

internal sealed record FamilyEconomyState(
  FamilyId Id,
  ContributionSettings ContributionSettings,
  IReadOnlyList<MemberWalletState> Wallets,
  IReadOnlyList<ContributionState> Contributions,
  IReadOnlyList<ContributionId> UsedContributionIds,
  IReadOnlyList<PromotionState> Promotions,
  IReadOnlyList<TransactionId> AppliedOperationIds,
  IReadOnlyList<LedgerEntryState> Ledger,
  IReadOnlyList<SavingGoalState> SavingGoals,
  IReadOnlyList<SavingsVoucherState> SavingsVouchers,
  IReadOnlyList<GuardianGoalGiftState> GuardianGifts,
  IReadOnlyList<IsoWeekPeriod> SettledInterestPeriods,
  IReadOnlyList<ShopRewardState> ShopRewards,
  IReadOnlyList<RedemptionRequestState> RedemptionRequests,
  IReadOnlyList<FamilyMemberId> ShopTaskGateOverrideMemberIds
)
{
  public FamilyEconomyState(
    FamilyId id,
    ContributionSettings contributionSettings,
    IReadOnlyList<MemberWalletState> wallets,
    IReadOnlyList<ContributionState> contributions,
    IReadOnlyList<ContributionId> usedContributionIds,
    IReadOnlyList<PromotionState> promotions,
    IReadOnlyList<TransactionId> appliedOperationIds,
    IReadOnlyList<LedgerEntryState> ledger,
    IReadOnlyList<SavingGoalState> savingGoals,
    IReadOnlyList<SavingsVoucherState> savingsVouchers,
    IReadOnlyList<GuardianGoalGiftState> guardianGifts,
    IReadOnlyList<IsoWeekPeriod> settledInterestPeriods,
    IReadOnlyList<ShopRewardState> shopRewards,
    IReadOnlyList<RedemptionRequestState> redemptionRequests
  )
    : this(
      id,
      contributionSettings,
      wallets,
      contributions,
      usedContributionIds,
      promotions,
      appliedOperationIds,
      ledger,
      savingGoals,
      savingsVouchers,
      guardianGifts,
      settledInterestPeriods,
      shopRewards,
      redemptionRequests,
      []
    )
  {
  }
}
