using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

using Microsoft.EntityFrameworkCore;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

internal static partial class TypedAggregatePersistenceModel
{
  private static void ConfigureFamilyProgression(ModelBuilder modelBuilder)
  {
    var progression = modelBuilder.Entity<FamilyProgressionRootEntity>();
    progression.ToTable(
      "FamilyProgressions",
      table =>
      {
        table.HasCheckConstraint("CK_FamilyProgressions_Revision", "\"Revision\" >= 0");
        table.HasCheckConstraint("CK_FamilyProgressions_StoredLevel", "\"StoredLevel\" >= 0 AND \"StoredLevel\" <= 4");
        table.HasCheckConstraint("CK_FamilyProgressions_CompletedWeeklyStreak", "\"CompletedWeeklyStreak\" >= 0");
        table.HasCheckConstraint("CK_FamilyProgressions_CurrentWeekTarget", "\"CurrentWeekTarget\" >= 1");
        table.HasCheckConstraint(
          "CK_FamilyProgressions_MaximumSavingsInterestRate",
          "\"MaximumSavingsInterestRate\" > 0 AND \"MaximumSavingsInterestRate\" <= 100"
        );
      }
    );
    progression.HasKey(item => item.FamilyId);
    progression.HasOne<FamilyRootEntity>().WithOne()
      .HasForeignKey<FamilyProgressionRootEntity>(item => item.FamilyId)
      .OnDelete(DeleteBehavior.Restrict);
    progression.Property(item => item.Revision).IsConcurrencyToken();
    progression.Property(item => item.UpdatedBy).HasMaxLength(128);
    progression.Property(item => item.MaximumSavingsInterestRate).HasColumnType("numeric");
    progression.HasMany(item => item.Participants).WithOne(item => item.Progression)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);
    progression.HasMany(item => item.ClosedWeeks).WithOne(item => item.Progression)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);

    var participant = modelBuilder.Entity<FamilyProgressionParticipantEntity>();
    participant.ToTable(
      "FamilyProgressionParticipants",
      table => table.HasCheckConstraint(
        "CK_FamilyProgressionParticipants_StoredWeeklyStreak",
        "\"StoredWeeklyStreak\" >= 0"
      )
    );
    participant.HasKey(item => new { item.FamilyId, item.MemberId });
    participant.Property(item => item.MemberId).HasMaxLength(128);

    var week = modelBuilder.Entity<ClosedFamilyWeekEntity>();
    week.ToTable(
      "ClosedFamilyWeeks",
      table =>
      {
        table.HasCheckConstraint("CK_ClosedFamilyWeeks_IsoYear", "\"IsoYear\" >= 1 AND \"IsoYear\" <= 9999");
        table.HasCheckConstraint("CK_ClosedFamilyWeeks_IsoWeek", "\"IsoWeek\" >= 1 AND \"IsoWeek\" <= 53");
        table.HasCheckConstraint("CK_ClosedFamilyWeeks_Outcome", "\"Outcome\" >= 0 AND \"Outcome\" <= 1");
      }
    );
    week.HasKey(item => new { item.FamilyId, item.IsoYear, item.IsoWeek });
    week.HasMany(item => item.MemberEvaluations).WithOne(item => item.ClosedWeek)
      .HasForeignKey(item => new { item.FamilyId, item.IsoYear, item.IsoWeek })
      .OnDelete(DeleteBehavior.Cascade);

    var evaluation = modelBuilder.Entity<ClosedFamilyWeekEvaluationEntity>();
    evaluation.ToTable(
      "ClosedFamilyWeekEvaluations",
      table =>
      {
        table.HasCheckConstraint("CK_ClosedFamilyWeekEvaluations_StreakDays", "\"StreakDays\" >= 0");
        table.HasCheckConstraint(
          "CK_ClosedFamilyWeekEvaluations_CompletionPercent",
          "\"CompletionPercent\" >= 0 AND \"CompletionPercent\" <= 100"
        );
        table.HasCheckConstraint(
          "CK_ClosedFamilyWeekEvaluations_AverageBasicRating",
          "\"AverageBasicRating\" >= 0 AND \"AverageBasicRating\" <= 5"
        );
        table.HasCheckConstraint("CK_ClosedFamilyWeekEvaluations_UncappedValue", "\"UncappedParticipationValue\" >= 0");
        table.HasCheckConstraint("CK_ClosedFamilyWeekEvaluations_CappedValue", "\"CappedParticipationValue\" >= 0 AND \"CappedParticipationValue\" <= 8");
        table.HasCheckConstraint("CK_ClosedFamilyWeekEvaluations_LadiStagePosition", "\"LadiStagePosition\" >= 1 AND \"LadiStagePosition\" <= 6");
      }
    );
    evaluation.HasKey(item => new { item.FamilyId, item.IsoYear, item.IsoWeek, item.MemberId });
    evaluation.Property(item => item.MemberId).HasMaxLength(128);
    evaluation.Property(item => item.CompletionPercent).HasColumnType("numeric");
    evaluation.Property(item => item.AverageBasicRating).HasColumnType("numeric");
    evaluation.Property(item => item.UncappedParticipationValue).HasColumnType("numeric");
    evaluation.Property(item => item.CappedParticipationValue).HasColumnType("numeric");
    evaluation.Property(item => item.LadiStageId).HasMaxLength(128);
  }
}
