using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

using Microsoft.EntityFrameworkCore;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

internal static partial class TypedAggregatePersistenceModel
{
  private static void ConfigureSavings(ModelBuilder modelBuilder)
  {
    var goal = modelBuilder.Entity<EconomySavingGoalEntity>();
    goal.ToTable(
      "FamilyEconomySavingGoals",
      table =>
      {
        table.HasCheckConstraint("CK_FamilyEconomySavingGoals_Target", "\"Target\" > 0");
        table.HasCheckConstraint(
          "CK_FamilyEconomySavingGoals_Visibility",
          "\"Visibility\" >= 0 AND \"Visibility\" <= 2"
        );
        table.HasCheckConstraint(
          "CK_FamilyEconomySavingGoals_OwnerVisibility",
          "\"OwnerMemberId\" IS NOT NULL OR \"Visibility\" = 0"
        );
        table.HasCheckConstraint(
          "CK_FamilyEconomySavingGoals_Amounts",
          "\"Saved\" >= 0 AND \"StarterBonus\" >= 0 AND \"InterestEarned\" >= 0"
        );
        table.HasCheckConstraint(
          "CK_FamilyEconomySavingGoals_LastInterestPeriod",
          "(\"LastInterestYear\" IS NULL AND \"LastInterestWeek\" IS NULL "
          + "AND \"LastInterestRate\" IS NULL) OR (\"LastInterestYear\" IS NOT NULL "
          + "AND \"LastInterestWeek\" IS NOT NULL AND \"LastInterestRate\" IS NOT NULL)"
        );
        table.HasCheckConstraint(
          "CK_FamilyEconomySavingGoals_LastInterestRate",
          "\"LastInterestRate\" IS NULL OR (\"LastInterestRate\" >= 0 AND \"LastInterestRate\" <= 100)"
        );
        table.HasCheckConstraint(
          "CK_FamilyEconomySavingGoals_LastInterestWeek",
          "\"LastInterestYear\" IS NULL OR (\"LastInterestYear\" >= 1 AND \"LastInterestYear\" <= 9999 "
          + "AND \"LastInterestWeek\" >= 1 AND \"LastInterestWeek\" <= 53)"
        );
      }
    );
    goal.HasKey(item => new { item.FamilyId, item.GoalId });
    goal.Property(item => item.GoalId).HasMaxLength(128);
    goal.Property(item => item.OwnerMemberId).HasMaxLength(128);
    goal.Property(item => item.Target).HasColumnType("numeric");
    goal.Property(item => item.Saved).HasColumnType("numeric");
    goal.Property(item => item.StarterBonus).HasColumnType("numeric");
    goal.Property(item => item.InterestEarned).HasColumnType("numeric");
    goal.Property(item => item.LastInterestRate).HasColumnType("numeric");
    goal.HasMany(item => item.Participants).WithOne(item => item.Goal)
      .HasForeignKey(item => new { item.FamilyId, item.GoalId })
      .OnDelete(DeleteBehavior.Cascade);
    goal.HasMany(item => item.RefundableContributions).WithOne(item => item.Goal)
      .HasForeignKey(item => new { item.FamilyId, item.GoalId })
      .OnDelete(DeleteBehavior.Cascade);
    goal.HasOne(item => item.CancellationRequest).WithOne(item => item.Goal)
      .HasForeignKey<EconomyGoalCancellationEntity>(item => new { item.FamilyId, item.GoalId })
      .OnDelete(DeleteBehavior.Cascade);

    var participant = modelBuilder.Entity<EconomySavingGoalParticipantEntity>();
    participant.ToTable("FamilyEconomySavingGoalParticipants");
    participant.HasKey(item => new { item.FamilyId, item.GoalId, item.MemberId });
    participant.Property(item => item.GoalId).HasMaxLength(128);
    participant.Property(item => item.MemberId).HasMaxLength(128);

    var refund = modelBuilder.Entity<EconomySavingGoalRefundEntity>();
    refund.ToTable(
      "FamilyEconomySavingGoalRefunds",
      table => table.HasCheckConstraint("CK_FamilyEconomySavingGoalRefunds_Amount", "\"Amount\" > 0")
    );
    refund.HasKey(item => new { item.FamilyId, item.GoalId, item.MemberId });
    refund.Property(item => item.GoalId).HasMaxLength(128);
    refund.Property(item => item.MemberId).HasMaxLength(128);
    refund.Property(item => item.Amount).HasColumnType("numeric");

    var cancellation = modelBuilder.Entity<EconomyGoalCancellationEntity>();
    cancellation.ToTable("FamilyEconomyGoalCancellationRequests");
    cancellation.HasKey(item => new { item.FamilyId, item.GoalId });
    cancellation.Property(item => item.GoalId).HasMaxLength(128);
    cancellation.Property(item => item.RequestedByMemberId).HasMaxLength(128);
    cancellation.HasMany(item => item.Consents).WithOne(item => item.CancellationRequest)
      .HasForeignKey(item => new { item.FamilyId, item.GoalId })
      .OnDelete(DeleteBehavior.Cascade);

    var cancellationConsent = modelBuilder.Entity<EconomyGoalCancellationConsentEntity>();
    cancellationConsent.ToTable("FamilyEconomyGoalCancellationConsents");
    cancellationConsent.HasKey(item => new { item.FamilyId, item.GoalId, item.MemberId });
    cancellationConsent.Property(item => item.GoalId).HasMaxLength(128);
    cancellationConsent.Property(item => item.MemberId).HasMaxLength(128);
    cancellationConsent.HasOne<EconomySavingGoalParticipantEntity>().WithMany()
      .HasForeignKey(item => new { item.FamilyId, item.GoalId, item.MemberId })
      .OnDelete(DeleteBehavior.Restrict);

    ConfigureSavingsVouchers(modelBuilder);
    ConfigureGuardianGiftsAndSettlements(modelBuilder);
  }

  private static void ConfigureSavingsVouchers(ModelBuilder modelBuilder)
  {
    var voucher = modelBuilder.Entity<EconomySavingsVoucherEntity>();
    voucher.ToTable(
      "FamilyEconomySavingsVouchers",
      table => table.HasCheckConstraint("CK_FamilyEconomySavingsVouchers_Amount", "\"Amount\" > 0")
    );
    voucher.HasKey(item => new { item.FamilyId, item.GoalId });
    voucher.Property(item => item.GoalId).HasMaxLength(128);
    voucher.Property(item => item.Amount).HasColumnType("numeric");
    voucher.Property(item => item.OwnerMemberId).HasMaxLength(128);
    voucher.HasOne<EconomySavingGoalEntity>().WithMany()
      .HasForeignKey(item => new { item.FamilyId, item.GoalId })
      .OnDelete(DeleteBehavior.Restrict);
    voucher.HasMany(item => item.Participants).WithOne(item => item.Voucher)
      .HasForeignKey(item => new { item.FamilyId, item.GoalId })
      .OnDelete(DeleteBehavior.Cascade);
    voucher.HasMany(item => item.Consents).WithOne(item => item.Voucher)
      .HasForeignKey(item => new { item.FamilyId, item.GoalId })
      .OnDelete(DeleteBehavior.Cascade);

    var participant = modelBuilder.Entity<EconomySavingsVoucherParticipantEntity>();
    participant.ToTable("FamilyEconomySavingsVoucherParticipants");
    participant.HasKey(item => new { item.FamilyId, item.GoalId, item.MemberId });
    participant.Property(item => item.GoalId).HasMaxLength(128);
    participant.Property(item => item.MemberId).HasMaxLength(128);

    var consent = modelBuilder.Entity<EconomySavingsVoucherConsentEntity>();
    consent.ToTable("FamilyEconomySavingsVoucherConsents");
    consent.HasKey(item => new { item.FamilyId, item.GoalId, item.MemberId });
    consent.Property(item => item.GoalId).HasMaxLength(128);
    consent.Property(item => item.MemberId).HasMaxLength(128);
    consent.HasOne<EconomySavingsVoucherParticipantEntity>().WithMany()
      .HasForeignKey(item => new { item.FamilyId, item.GoalId, item.MemberId })
      .OnDelete(DeleteBehavior.Restrict);
  }

  private static void ConfigureGuardianGiftsAndSettlements(ModelBuilder modelBuilder)
  {
    var gift = modelBuilder.Entity<EconomyGuardianGiftEntity>();
    gift.ToTable(
      "FamilyEconomyGuardianGifts",
      table =>
      {
        table.HasCheckConstraint("CK_FamilyEconomyGuardianGifts_Amount", "\"Amount\" > 0");
        table.HasCheckConstraint(
          "CK_FamilyEconomyGuardianGifts_Destination",
          "\"Destination\" >= 0 AND \"Destination\" <= 1"
        );
        table.HasCheckConstraint(
          "CK_FamilyEconomyGuardianGifts_BalanceDestination",
          "\"Destination\" <> 0 OR \"GoalId\" IS NULL"
        );
      }
    );
    gift.HasKey(item => new { item.FamilyId, item.StorageId });
    gift.Property(item => item.GiftId).HasMaxLength(128);
    gift.Property(item => item.OwnerMemberId).HasMaxLength(128);
    gift.Property(item => item.GuardianMemberId).HasMaxLength(128);
    gift.Property(item => item.GoalId).HasMaxLength(128);
    gift.Property(item => item.Amount).HasColumnType("numeric");
    gift.HasIndex(item => new { item.FamilyId, item.GiftId }).IsUnique()
      .HasFilter("\"GiftId\" IS NOT NULL");

    var period = modelBuilder.Entity<EconomySettledInterestPeriodEntity>();
    period.ToTable(
      "FamilyEconomySettledInterestPeriods",
      table => table.HasCheckConstraint(
        "CK_FamilyEconomySettledInterestPeriods_Week",
        "\"IsoYear\" >= 1 AND \"IsoYear\" <= 9999 AND \"IsoWeek\" >= 1 AND \"IsoWeek\" <= 53"
      )
    );
    period.HasKey(item => new { item.FamilyId, item.IsoYear, item.IsoWeek });
  }
}
