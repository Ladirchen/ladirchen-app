using LadirchenApp.Webhost.Infrastructure.Persistence.Typed;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LadirchenApp.Webhost.Infrastructure.Persistence;

public sealed class LadirchenDbContext : DbContext, IDataProtectionKeyContext
{
  public LadirchenDbContext(DbContextOptions<LadirchenDbContext> options)
    : base(options) { }

  public DbSet<AuthenticationAccountEntity> AuthenticationAccounts
  {
    get
    {
      return Set<AuthenticationAccountEntity>();
    }
  }

  public DbSet<DataProtectionKey> DataProtectionKeys
  {
    get
    {
      return Set<DataProtectionKey>();
    }
  }

  public DbSet<BetaFamilyRegistrationEntity> BetaFamilyRegistrations
  {
    get
    {
      return Set<BetaFamilyRegistrationEntity>();
    }
  }

  public DbSet<MemberAccountInvitationEntity> MemberAccountInvitations
  {
    get
    {
      return Set<MemberAccountInvitationEntity>();
    }
  }

  public DbSet<FamilyCommandReceiptEntity> FamilyCommandReceipts
  {
    get
    {
      return Set<FamilyCommandReceiptEntity>();
    }
  }

  public DbSet<FamilyAggregateOutboxMessageEntity> FamilyAggregateOutboxMessages
  {
    get
    {
      return Set<FamilyAggregateOutboxMessageEntity>();
    }
  }

  public DbSet<FamilyRootEntity> Families
  {
    get
    {
      return Set<FamilyRootEntity>();
    }
  }

  public DbSet<FamilyEconomyRootEntity> FamilyEconomies
  {
    get
    {
      return Set<FamilyEconomyRootEntity>();
    }
  }

  public DbSet<FamilyProgressionRootEntity> FamilyProgressions
  {
    get
    {
      return Set<FamilyProgressionRootEntity>();
    }
  }

  public DbSet<HouseCustomizationRootEntity> HouseCustomizations
  {
    get
    {
      return Set<HouseCustomizationRootEntity>();
    }
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    var account = modelBuilder.Entity<AuthenticationAccountEntity>();
    account.ToTable("AuthenticationAccounts");
    account.HasKey(item => item.Username);
    account.Property(item => item.Username).HasMaxLength(100);
    account.Property(item => item.EmailAddress).HasMaxLength(320);
    account.Property(item => item.NormalizedEmail).HasMaxLength(320);
    account.Property(item => item.FamilyName).HasMaxLength(200);
    account.Property(item => item.MemberId).HasMaxLength(100);
    account.Property(item => item.PasswordHash).IsRequired();
    account.Property(item => item.PasswordSalt).IsRequired();
    account.HasIndex(item => item.FamilyId);
    account
      .HasIndex(item => item.NormalizedEmail)
      .IsUnique()
      .HasFilter("\"NormalizedEmail\" IS NOT NULL");
    account.HasIndex(item => new { item.FamilyId, item.MemberId }).IsUnique();

    var betaRegistration = modelBuilder.Entity<BetaFamilyRegistrationEntity>();
    betaRegistration.ToTable(
      "BetaFamilyRegistrations",
      table => table.HasCheckConstraint("CK_BetaFamilyRegistrations_Singleton", "\"Id\" = 1")
    );
    betaRegistration.HasKey(item => item.Id);
    betaRegistration.HasIndex(item => item.FamilyId).IsUnique();

    var memberInvitation = modelBuilder.Entity<MemberAccountInvitationEntity>();
    memberInvitation.ToTable("MemberAccountInvitations");
    memberInvitation.HasKey(item => item.InvitationId);
    memberInvitation.Property(item => item.FamilyName).HasMaxLength(200);
    memberInvitation.Property(item => item.MemberId).HasMaxLength(128);
    memberInvitation.Property(item => item.EmailAddress).HasMaxLength(320);
    memberInvitation.Property(item => item.NormalizedEmail).HasMaxLength(320);
    memberInvitation.Property(item => item.ActivationCodeHash).IsRequired();
    memberInvitation
      .HasIndex(item => item.NormalizedEmail)
      .IsUnique()
      .HasFilter("\"RedeemedAt\" IS NULL");
    memberInvitation
      .HasIndex(item => new { item.FamilyId, item.MemberId })
      .IsUnique()
      .HasFilter("\"RedeemedAt\" IS NULL");

    var receipt = modelBuilder.Entity<FamilyCommandReceiptEntity>();
    receipt.ToTable("FamilyCommandReceipts");
    receipt.HasKey(item => item.CommandId);
    receipt.Property(item => item.ActorId).HasMaxLength(128);
    receipt.Property(item => item.CommandType).HasMaxLength(100);
    receipt.Property(item => item.RequestFingerprint).HasMaxLength(64);
    receipt.Property(item => item.ResultJson);
    receipt.HasIndex(item => item.FamilyId);

    var outbox = modelBuilder.Entity<FamilyAggregateOutboxMessageEntity>();
    outbox.ToTable("FamilyAggregateOutboxMessages");
    outbox.HasKey(item => item.MessageId);
    outbox.Property(item => item.AggregateType).HasMaxLength(100);
    outbox.Property(item => item.UpdatedBy).HasMaxLength(128);
    outbox.Property(item => item.LastError).HasMaxLength(2000);
    outbox.HasIndex(item => new { item.FamilyId, item.AggregateType, item.Revision }).IsUnique();
    outbox.HasIndex(item => new { item.PublishedAt, item.NextAttemptAt });
    outbox
      .HasOne<FamilyCommandReceiptEntity>()
      .WithMany()
      .HasForeignKey(item => item.CommandId)
      .OnDelete(DeleteBehavior.Cascade);

    TypedAggregatePersistenceModel.Configure(modelBuilder);
  }
}
