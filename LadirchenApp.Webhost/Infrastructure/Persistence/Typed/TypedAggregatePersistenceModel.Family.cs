using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

using Microsoft.EntityFrameworkCore;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

internal static partial class TypedAggregatePersistenceModel
{
  private static void ConfigureFamily(ModelBuilder modelBuilder)
  {
    var family = modelBuilder.Entity<FamilyRootEntity>();
    family.ToTable(
      "Families",
      table =>
      {
        table.HasCheckConstraint("CK_Families_Revision", "\"Revision\" >= 0");
        table.HasCheckConstraint(
          "CK_Families_ExchangeRate",
          "\"LadirchenPerCurrencyUnit\" >= 1"
        );
        table.HasCheckConstraint("CK_Families_Currency", "\"Currency\" >= 0 AND \"Currency\" <= 2");
      }
    );
    family.HasKey(item => item.FamilyId);
    family.Property(item => item.Revision).IsConcurrencyToken();
    family.Property(item => item.UpdatedBy).HasMaxLength(128);
    family.Property(item => item.ActiveChildId).HasMaxLength(128);
    family
      .HasMany(item => item.Members)
      .WithOne(item => item.Family)
      .HasForeignKey(item => item.FamilyId)
      .OnDelete(DeleteBehavior.Cascade);
    family
      .HasMany(item => item.Pets)
      .WithOne(item => item.Family)
      .HasForeignKey(item => item.FamilyId)
      .OnDelete(DeleteBehavior.Cascade);

    var member = modelBuilder.Entity<FamilyMemberEntity>();
    member.ToTable(
      "FamilyMembers",
      table =>
      {
        table.HasCheckConstraint("CK_FamilyMembers_Role", "\"Role\" >= 0 AND \"Role\" <= 1");
        table.HasCheckConstraint(
          "CK_FamilyMembers_GuardianAccess",
          "\"GuardianAccess\" >= 0 AND \"GuardianAccess\" <= 1"
        );
        table.HasCheckConstraint(
          "CK_FamilyMembers_RoleAccess",
          "\"Role\" = 1 OR \"GuardianAccess\" = 1"
        );
        table.HasCheckConstraint(
          "CK_FamilyMembers_PendingInvitation",
          "NOT \"InvitationPending\" OR \"Role\" = 1"
        );
      }
    );
    member.HasKey(item => new { item.FamilyId, item.MemberId });
    member.Property(item => item.MemberId).HasMaxLength(128);
    member.Property(item => item.Nickname).HasMaxLength(18);
    member.HasIndex(item => new { item.FamilyId, item.EmailAddress });

    var pet = modelBuilder.Entity<FamilyPetEntity>();
    pet.ToTable(
      "FamilyPets",
      table => table.HasCheckConstraint("CK_FamilyPets_Kind", "\"Kind\" >= 0 AND \"Kind\" <= 4")
    );
    pet.HasKey(item => new { item.FamilyId, item.PetId });
    pet.Property(item => item.PetId).HasMaxLength(128);
  }
}
