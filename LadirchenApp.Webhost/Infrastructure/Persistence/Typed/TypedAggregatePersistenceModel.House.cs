using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

using Microsoft.EntityFrameworkCore;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

internal static partial class TypedAggregatePersistenceModel
{
  private static void ConfigureHouseCustomization(ModelBuilder modelBuilder)
  {
    var house = modelBuilder.Entity<HouseCustomizationRootEntity>();
    house.ToTable(
      "HouseCustomizations",
      table =>
      {
        table.HasCheckConstraint("CK_HouseCustomizations_Revision", "\"Revision\" >= 0");
        table.HasCheckConstraint("CK_HouseCustomizations_SubscriptionTier", "\"SubscriptionTier\" >= 0 AND \"SubscriptionTier\" <= 1");
      }
    );
    house.HasKey(item => item.FamilyId);
    house.HasOne<FamilyRootEntity>().WithOne()
      .HasForeignKey<HouseCustomizationRootEntity>(item => item.FamilyId)
      .OnDelete(DeleteBehavior.Restrict);
    house.Property(item => item.Revision).IsConcurrencyToken();
    house.Property(item => item.UpdatedBy).HasMaxLength(128);
    house.Property(item => item.SelectedThemeId).HasMaxLength(128);
    house.HasMany(item => item.Accessories).WithOne(item => item.HouseCustomization)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);
    house.HasMany(item => item.Themes).WithOne(item => item.HouseCustomization)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);
    house.HasMany(item => item.RoomDesigns).WithOne(item => item.HouseCustomization)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);
    house.HasMany(item => item.Placements).WithOne(item => item.HouseCustomization)
      .HasForeignKey(item => item.FamilyId).OnDelete(DeleteBehavior.Cascade);

    var accessory = modelBuilder.Entity<HouseAccessoryEntity>();
    accessory.ToTable(
      "HouseAccessories",
      table => table.HasCheckConstraint(
        "CK_HouseAccessories_Equipped",
        "NOT \"Equipped\" OR \"Owned\""
      )
    );
    accessory.HasKey(item => new { item.FamilyId, item.AccessoryId });
    accessory.Property(item => item.AccessoryId).HasMaxLength(128);

    var theme = modelBuilder.Entity<HouseThemeEntity>();
    theme.ToTable("HouseThemes");
    theme.HasKey(item => new { item.FamilyId, item.ThemeId });
    theme.Property(item => item.ThemeId).HasMaxLength(128);

    var roomDesign = modelBuilder.Entity<HouseRoomDesignEntity>();
    roomDesign.ToTable(
      "HouseRoomDesigns",
      table => table.HasCheckConstraint(
        "CK_HouseRoomDesigns_Selected",
        "NOT \"Selected\" OR \"Owned\""
      )
    );
    roomDesign.HasKey(item => new { item.FamilyId, item.RoomDesignId });
    roomDesign.Property(item => item.RoomDesignId).HasMaxLength(128);

    var placement = modelBuilder.Entity<HousePlacementEntity>();
    placement.ToTable(
      "HousePlacements",
      table => table.HasCheckConstraint("CK_HousePlacements_Scale", "\"Scale\" > 0")
    );
    placement.HasKey(item => new { item.FamilyId, item.PlacementId });
    placement.Property(item => item.PlacementId).HasMaxLength(128);
    placement.Property(item => item.EntityType).HasMaxLength(32);
    placement.Property(item => item.EntityId).HasMaxLength(128);
    placement.Property(item => item.Area).HasMaxLength(128);
    placement.Property(item => item.X).HasColumnType("numeric");
    placement.Property(item => item.Y).HasColumnType("numeric");
    placement.Property(item => item.Scale).HasColumnType("numeric");
  }
}
