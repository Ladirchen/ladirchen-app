using System;
using System.Collections.Generic;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class FamilyEconomyRootEntity
{
  public Guid FamilyId { get; set; }
  public long Revision { get; set; }
  public string UpdatedBy { get; set; } = string.Empty;
  public DateTimeOffset UpdatedAt { get; set; }
  public decimal PerfectRatingBonusPercent { get; set; }
  public ICollection<FamilyEconomyWalletEntity> Wallets { get; set; } = new List<FamilyEconomyWalletEntity>();
  public ICollection<EconomyContributionEntity> Contributions { get; set; } = new List<EconomyContributionEntity>();
  public ICollection<EconomyUsedContributionIdEntity> UsedContributionIds { get; set; } =
    new List<EconomyUsedContributionIdEntity>();
  public ICollection<EconomyPromotionEntity> Promotions { get; set; } = new List<EconomyPromotionEntity>();
  public ICollection<EconomySavingGoalEntity> SavingGoals { get; set; } = new List<EconomySavingGoalEntity>();
  public ICollection<EconomySavingsVoucherEntity> SavingsVouchers { get; set; } =
    new List<EconomySavingsVoucherEntity>();
  public ICollection<EconomyGuardianGiftEntity> GuardianGifts { get; set; } = new List<EconomyGuardianGiftEntity>();
  public ICollection<EconomySettledInterestPeriodEntity> SettledInterestPeriods { get; set; } =
    new List<EconomySettledInterestPeriodEntity>();
  public ICollection<EconomyShopRewardEntity> ShopRewards { get; set; } = new List<EconomyShopRewardEntity>();
  public ICollection<EconomyRedemptionRequestEntity> RedemptionRequests { get; set; } =
    new List<EconomyRedemptionRequestEntity>();
  public ICollection<EconomyAppliedOperationEntity> AppliedOperations { get; set; } =
    new List<EconomyAppliedOperationEntity>();
  public ICollection<EconomyLedgerEntryEntity> LedgerEntries { get; set; } =
    new List<EconomyLedgerEntryEntity>();
  public ICollection<ContributionPresentationEntity> ContributionPresentations { get; set; } =
    new List<ContributionPresentationEntity>();
  public ICollection<SavingGoalPresentationEntity> SavingGoalPresentations { get; set; } =
    new List<SavingGoalPresentationEntity>();
  public ICollection<ShopRewardPresentationEntity> ShopRewardPresentations { get; set; } =
    new List<ShopRewardPresentationEntity>();
  public ICollection<GuardianGiftPresentationEntity> GuardianGiftPresentations { get; set; } =
    new List<GuardianGiftPresentationEntity>();
}
