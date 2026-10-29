using Microsoft.EntityFrameworkCore;

namespace LadirchenApp.Webhost.Infrastructure.Persistence;

public sealed class LadirchenDbContext : DbContext
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

  public DbSet<FamilyAggregateSnapshotEntity> FamilyAggregateSnapshots
  {
    get
    {
      return Set<FamilyAggregateSnapshotEntity>();
    }
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    var account = modelBuilder.Entity<AuthenticationAccountEntity>();
    account.ToTable("AuthenticationAccounts");
    account.HasKey(item => item.Username);
    account.Property(item => item.Username).HasMaxLength(100);
    account.Property(item => item.FamilyName).HasMaxLength(200);
    account.Property(item => item.MemberId).HasMaxLength(100);
    account.Property(item => item.PasswordHash).IsRequired();
    account.Property(item => item.PasswordSalt).IsRequired();
    account.HasIndex(item => item.FamilyId);

    var aggregate = modelBuilder.Entity<FamilyAggregateSnapshotEntity>();
    aggregate.ToTable("FamilyAggregateSnapshots");
    aggregate.HasKey(item => new { item.FamilyId, item.AggregateType });
    aggregate.Property(item => item.AggregateType).HasMaxLength(100);
    aggregate.Property(item => item.Revision).IsConcurrencyToken();
    aggregate.Property(item => item.StateJson).IsRequired();
    aggregate.Property(item => item.UpdatedBy).HasMaxLength(100);
  }
}
