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
  IReadOnlyList<RedemptionRequestState> RedemptionRequests
);
