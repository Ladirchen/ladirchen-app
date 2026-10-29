using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

using Microsoft.EntityFrameworkCore;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

internal static partial class TypedAggregatePersistenceModel
{
  private static void ConfigureFamilyEconomy(ModelBuilder modelBuilder)
  {
    var economy = modelBuilder.Entity<FamilyEconomyRootEntity>();
    economy.ToTable(
      "FamilyEconomies",
      table =>
      {
        table.HasCheckConstraint("CK_FamilyEconomies_Revision", "\"Revision\" >= 0");
        table.HasCheckConstraint(
          "CK_FamilyEconomies_PerfectRatingBonus",
          "\"PerfectRatingBonusPercent\" >= 0 AND \"PerfectRatingBonusPercent\" <= 100"
        );
      }
    );
    economy.HasKey(item => item.FamilyId);
    economy.HasOne<FamilyRootEntity>().WithOne()
      .HasForeignKey<FamilyEconomyRootEntity>(item => item.FamilyId)
      .OnDelete(DeleteBehavior.Restrict);
    economy.Property(item => item.Revision).IsConcurrencyToken();
    economy.Property(item => item.UpdatedBy).HasMaxLength(128);
    economy.Property(item => item.PerfectRatingBonusPercent).HasColumnType("numeric");
    ConfigureEconomyRootCollections(economy);
    ConfigureEconomyWalletsAndHistory(modelBuilder);
    ConfigureContributions(modelBuilder);
    ConfigureSavings(modelBuilder);
    ConfigureRewardShop(modelBuilder);
    ConfigureEconomyPresentations(modelBuilder);
  }

  private static void ConfigureEconomyRootCollections(
    Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<FamilyEconomyRootEntity> economy
  )
  {
    economy.HasMany(item => item.Wallets).WithOne(item => item.Economy)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);
    economy.HasMany(item => item.Contributions).WithOne(item => item.Economy)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);
    economy.HasMany(item => item.UsedContributionIds).WithOne(item => item.Economy)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);
    economy.HasMany(item => item.Promotions).WithOne(item => item.Economy)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);
    economy.HasMany(item => item.SavingGoals).WithOne(item => item.Economy)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);
    economy.HasMany(item => item.SavingsVouchers).WithOne(item => item.Economy)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);
    economy.HasMany(item => item.GuardianGifts).WithOne(item => item.Economy)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);
    economy.HasMany(item => item.SettledInterestPeriods).WithOne(item => item.Economy)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);
    economy.HasMany(item => item.ShopRewards).WithOne(item => item.Economy)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);
    economy.HasMany(item => item.RedemptionRequests).WithOne(item => item.Economy)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);
    economy.HasMany(item => item.AppliedOperations).WithOne(item => item.Economy)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);
    economy.HasMany(item => item.LedgerEntries).WithOne(item => item.Economy)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);
    economy.HasMany(item => item.ContributionPresentations).WithOne(item => item.Economy)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);
    economy.HasMany(item => item.SavingGoalPresentations).WithOne(item => item.Economy)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);
    economy.HasMany(item => item.ShopRewardPresentations).WithOne(item => item.Economy)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);
    economy.HasMany(item => item.GuardianGiftPresentations).WithOne(item => item.Economy)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);
  }

  private static void ConfigureEconomyWalletsAndHistory(ModelBuilder modelBuilder)
  {
    var wallet = modelBuilder.Entity<FamilyEconomyWalletEntity>();
    wallet.ToTable(
      "FamilyEconomyWallets",
      table => table.HasCheckConstraint("CK_FamilyEconomyWallets_Balance", "\"Balance\" >= 0")
    );
    wallet.HasKey(item => new { item.FamilyId, item.MemberId });
    wallet.Property(item => item.MemberId).HasMaxLength(128);
    wallet.Property(item => item.Balance).HasColumnType("numeric");

    var operation = modelBuilder.Entity<EconomyAppliedOperationEntity>();
    operation.ToTable("FamilyEconomyAppliedOperations");
    operation.HasKey(item => new { item.FamilyId, item.OperationId });
    operation.Property(item => item.OperationId).HasMaxLength(128);

    var ledger = modelBuilder.Entity<EconomyLedgerEntryEntity>();
    ledger.ToTable(
      "FamilyEconomyLedgerEntries",
      table =>
      {
        table.HasCheckConstraint("CK_FamilyEconomyLedgerEntries_Amount", "\"Amount\" >= 0");
        table.HasCheckConstraint("CK_FamilyEconomyLedgerEntries_Kind", "\"Kind\" >= 0 AND \"Kind\" <= 7");
        table.HasCheckConstraint(
          "CK_FamilyEconomyLedgerEntries_SourceKind",
          "\"SourceKind\" >= 0 AND \"SourceKind\" <= 4"
        );
      }
    );
    ledger.HasKey(item => new { item.FamilyId, item.EntryId });
    ledger.Property(item => item.OperationId).HasMaxLength(128);
    ledger.Property(item => item.MemberId).HasMaxLength(128);
    ledger.Property(item => item.GoalId).HasMaxLength(128);
    ledger.Property(item => item.Amount).HasColumnType("numeric");
    ledger.Property(item => item.SourceId).HasMaxLength(128);
    ledger.Property(item => item.InitiatorMemberId).HasMaxLength(128);
    ledger.HasOne<EconomyAppliedOperationEntity>().WithMany()
      .HasForeignKey(item => new { item.FamilyId, item.OperationId })
      .OnDelete(DeleteBehavior.Restrict);
    ledger.HasIndex(item => new { item.FamilyId, item.OperationId });
    ledger.HasIndex(item => new { item.FamilyId, item.MemberId, item.OccurredAtUtc });
  }

  private static void ConfigureEconomyPresentations(ModelBuilder modelBuilder)
  {
    var contribution = modelBuilder.Entity<ContributionPresentationEntity>();
    contribution.ToTable("ContributionPresentations");
    contribution.HasKey(item => new { item.FamilyId, item.ContributionId });
    contribution.Property(item => item.ContributionId).HasMaxLength(128);

    var goal = modelBuilder.Entity<SavingGoalPresentationEntity>();
    goal.ToTable("SavingGoalPresentations");
    goal.HasKey(item => new { item.FamilyId, item.GoalId });
    goal.Property(item => item.GoalId).HasMaxLength(128);

    var reward = modelBuilder.Entity<ShopRewardPresentationEntity>();
    reward.ToTable("ShopRewardPresentations");
    reward.HasKey(item => new { item.FamilyId, item.RewardId });
    reward.Property(item => item.RewardId).HasMaxLength(128);

    var gift = modelBuilder.Entity<GuardianGiftPresentationEntity>();
    gift.ToTable("GuardianGiftPresentations");
    gift.HasKey(item => new { item.FamilyId, item.GiftId });
    gift.Property(item => item.GiftId).HasMaxLength(128);
  }
}
