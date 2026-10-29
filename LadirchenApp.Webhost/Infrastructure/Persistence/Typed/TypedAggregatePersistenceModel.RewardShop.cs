using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

using Microsoft.EntityFrameworkCore;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

internal static partial class TypedAggregatePersistenceModel
{
  private static void ConfigureRewardShop(ModelBuilder modelBuilder)
  {
    var reward = modelBuilder.Entity<EconomyShopRewardEntity>();
    reward.ToTable(
      "FamilyEconomyShopRewards",
      table =>
      {
        table.HasCheckConstraint("CK_FamilyEconomyShopRewards_Category", "\"Category\" >= 0 AND \"Category\" <= 5");
        table.HasCheckConstraint("CK_FamilyEconomyShopRewards_Price", "\"Price\" >= 0");
        table.HasCheckConstraint("CK_FamilyEconomyShopRewards_Quantity", "\"Quantity\" >= 0");
        table.HasCheckConstraint("CK_FamilyEconomyShopRewards_Status", "\"Status\" >= 0 AND \"Status\" <= 2");
        table.HasCheckConstraint(
          "CK_FamilyEconomyShopRewards_AvailabilityWindow",
          "\"AvailableFrom\" IS NULL OR \"AvailableUntil\" IS NULL OR \"AvailableFrom\" <= \"AvailableUntil\""
        );
      }
    );
    reward.HasKey(item => new { item.FamilyId, item.RewardId });
    reward.Property(item => item.RewardId).HasMaxLength(128);
    reward.Property(item => item.Price).HasColumnType("numeric");

    var request = modelBuilder.Entity<EconomyRedemptionRequestEntity>();
    request.ToTable(
      "FamilyEconomyRedemptionRequests",
      table =>
      {
        table.HasCheckConstraint("CK_FamilyEconomyRedemptionRequests_Price", "\"Price\" >= 0");
        table.HasCheckConstraint("CK_FamilyEconomyRedemptionRequests_Status", "\"Status\" >= 0 AND \"Status\" <= 3");
        table.HasCheckConstraint(
          "CK_FamilyEconomyRedemptionRequests_Decision",
          "(\"Status\" = 0 AND \"DecidedAt\" IS NULL) OR (\"Status\" <> 0 AND \"DecidedAt\" IS NOT NULL)"
        );
      }
    );
    request.HasKey(item => new { item.FamilyId, item.RequestId });
    request.Property(item => item.RequestId).HasMaxLength(128);
    request.Property(item => item.RewardId).HasMaxLength(128);
    request.Property(item => item.RequesterMemberId).HasMaxLength(128);
    request.Property(item => item.Price).HasColumnType("numeric");
    request.HasOne<EconomyShopRewardEntity>().WithMany()
      .HasForeignKey(item => new { item.FamilyId, item.RewardId })
      .OnDelete(DeleteBehavior.Restrict);
    request.HasIndex(item => new { item.FamilyId, item.RewardId })
      .HasFilter("\"Status\" = 0")
      .IsUnique();
    request.HasIndex(item => new { item.FamilyId, item.RequesterMemberId, item.Status });
  }
}
