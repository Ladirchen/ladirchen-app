using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

using Microsoft.EntityFrameworkCore;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

internal static partial class TypedAggregatePersistenceModel
{
  private static void ConfigureContributions(ModelBuilder modelBuilder)
  {
    var usedId = modelBuilder.Entity<EconomyUsedContributionIdEntity>();
    usedId.ToTable("FamilyEconomyUsedContributionIds");
    usedId.HasKey(item => new { item.FamilyId, item.ContributionId });
    usedId.Property(item => item.ContributionId).HasMaxLength(128);

    var contribution = modelBuilder.Entity<EconomyContributionEntity>();
    contribution.ToTable(
      "FamilyEconomyContributions",
      table =>
      {
        table.HasCheckConstraint("CK_FamilyEconomyContributions_Reward", "\"Reward\" >= 0");
        table.HasCheckConstraint("CK_FamilyEconomyContributions_Kind", "\"Kind\" >= 0 AND \"Kind\" <= 1");
        table.HasCheckConstraint("CK_FamilyEconomyContributions_Status", "\"Status\" >= 0 AND \"Status\" <= 2");
        table.HasCheckConstraint(
          "CK_FamilyEconomyContributions_Assignee",
          "\"Status\" = 0 OR \"AssigneeId\" IS NOT NULL"
        );
        table.HasCheckConstraint(
          "CK_FamilyEconomyContributions_Energy",
          "\"EnergyPercent\" >= 0 AND \"EnergyPercent\" <= 100"
        );
        table.HasCheckConstraint(
          "CK_FamilyEconomyContributions_Rating",
          "\"Rating\" IS NULL OR (\"Rating\" >= 1 AND \"Rating\" <= 5)"
        );
        table.HasCheckConstraint(
          "CK_FamilyEconomyContributions_Approval",
          "(\"Status\" = 2 AND \"Rating\" IS NOT NULL AND \"EarnedBaseReward\" IS NOT NULL "
          + "AND \"EarnedRatingBonus\" IS NOT NULL AND \"EarnedPromotionMultiplier\" IS NOT NULL "
          + "AND \"ApprovedAt\" IS NOT NULL) OR (\"Status\" <> 2 AND \"Rating\" IS NULL "
          + "AND \"EarnedBaseReward\" IS NULL AND \"EarnedRatingBonus\" IS NULL "
          + "AND \"EarnedPromotionMultiplier\" IS NULL AND \"ApprovedAt\" IS NULL)"
        );
      }
    );
    contribution.HasKey(item => new { item.FamilyId, item.ContributionId });
    contribution.Property(item => item.ContributionId).HasMaxLength(128);
    contribution.Property(item => item.Reward).HasColumnType("numeric");
    contribution.Property(item => item.EnergyPercent).HasColumnType("numeric");
    contribution.Property(item => item.AssigneeId).HasMaxLength(128);
    contribution.Property(item => item.EarnedBaseReward).HasColumnType("numeric");
    contribution.Property(item => item.EarnedRatingBonus).HasColumnType("numeric");
    contribution.Property(item => item.EarnedPromotionMultiplier).HasColumnType("numeric");
    contribution.HasOne<EconomyUsedContributionIdEntity>().WithMany()
      .HasForeignKey(item => new { item.FamilyId, item.ContributionId })
      .OnDelete(DeleteBehavior.Restrict);
    contribution
      .HasMany(item => item.InvitedPartners)
      .WithOne(item => item.Contribution)
      .HasForeignKey(item => new { item.FamilyId, item.ContributionId })
      .OnDelete(DeleteBehavior.Cascade);

    var partner = modelBuilder.Entity<EconomyContributionPartnerEntity>();
    partner.ToTable("FamilyEconomyContributionPartners");
    partner.HasKey(item => new { item.FamilyId, item.ContributionId, item.MemberId });
    partner.Property(item => item.ContributionId).HasMaxLength(128);
    partner.Property(item => item.MemberId).HasMaxLength(128);

    var promotion = modelBuilder.Entity<EconomyPromotionEntity>();
    promotion.ToTable(
      "FamilyEconomyPromotions",
      table =>
      {
        table.HasCheckConstraint(
          "CK_FamilyEconomyPromotions_Multiplier",
          "\"Multiplier\" IN (2, 3)"
        );
        table.HasCheckConstraint(
          "CK_FamilyEconomyPromotions_TeamworkBonus",
          "\"TeamworkBonus\" >= 0"
        );
      }
    );
    promotion.HasKey(item => new { item.FamilyId, item.PromotionId });
    promotion.Property(item => item.PromotionId).HasMaxLength(128);
    promotion.Property(item => item.ContributionId).HasMaxLength(128);
    promotion.Property(item => item.Multiplier).HasColumnType("numeric");
    promotion.Property(item => item.TeamworkBonus).HasColumnType("numeric");
    promotion.Property(item => item.DeadlineLocalDateTime).HasColumnType("timestamp without time zone");
    promotion.HasOne<EconomyContributionEntity>().WithMany()
      .HasForeignKey(item => new { item.FamilyId, item.ContributionId })
      .OnDelete(DeleteBehavior.Restrict);
  }
}
