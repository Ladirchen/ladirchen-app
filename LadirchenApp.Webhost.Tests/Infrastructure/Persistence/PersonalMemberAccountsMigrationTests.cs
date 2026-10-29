using System;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Testcontainers.PostgreSql;

namespace LadirchenApp.Webhost.Tests.Infrastructure.Persistence;

public sealed class PersonalMemberAccountsMigrationTests
{
  [Test]
  public async Task RollbackRemovesPendingChildrenAndTheirHousePlacements()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    await using var database = new LadirchenDbContext(options);
    await database.Database.MigrateAsync("20261029200633_AddPersonalMemberAccounts");
    var familyId = Guid.NewGuid();
    var updatedAt = DateTimeOffset.Parse("2026-10-29T12:00:00Z");
    await database.Database.ExecuteSqlInterpolatedAsync(
      $"""
      INSERT INTO "Families"
        ("FamilyId", "Revision", "UpdatedBy", "UpdatedAt", "TimeZone", "Currency",
         "LadirchenPerCurrencyUnit", "OnboardingCompleted")
      VALUES
        ({familyId}, {1L}, {"guardian"}, {updatedAt}, {"Europe/Zurich"}, {0}, {20}, {false});

      INSERT INTO "FamilyMembers"
        ("FamilyId", "MemberId", "Name", "Role", "GuardianAccess", "InvitationPending",
         "ParticipatesInWeeklyGoal")
      VALUES
        ({familyId}, {"guardian"}, {"Guardian"}, {1}, {0}, {false}, {false}),
        ({familyId}, {"pending-child"}, {"Child"}, {0}, {1}, {true}, {false});

      INSERT INTO "HouseCustomizations"
        ("FamilyId", "Revision", "UpdatedBy", "UpdatedAt", "SelectedThemeId", "SubscriptionTier")
      VALUES
        ({familyId}, {1L}, {"guardian"}, {updatedAt}, {"sunny-dollhouse"}, {0});

      INSERT INTO "HousePlacements"
        ("FamilyId", "PlacementId", "EntityType", "EntityId", "Area", "X", "Y", "Scale")
      VALUES
        ({familyId}, {"member-guardian"}, {"member"}, {"guardian"}, {"living-room"}, {35m}, {72m}, {0.78m}),
        ({familyId}, {"member-pending-child"}, {"member"}, {"pending-child"}, {"living-room"}, {45m}, {72m}, {0.75m});
      """
    );

    await database.Database.MigrateAsync("20261029193227_ArchiveLegacyFamilyAggregateSnapshots");
    var pendingChildCount = await ExecuteScalarAsync<long>(
      database,
      "SELECT count(*) FROM \"FamilyMembers\" WHERE \"MemberId\" = 'pending-child'"
    );
    var childPlacementCount = await ExecuteScalarAsync<long>(
      database,
      "SELECT count(*) FROM \"HousePlacements\" WHERE \"EntityId\" = 'pending-child'"
    );
    var guardianCount = await ExecuteScalarAsync<long>(
      database,
      "SELECT count(*) FROM \"FamilyMembers\" WHERE \"MemberId\" = 'guardian'"
    );
    var guardianPlacementCount = await ExecuteScalarAsync<long>(
      database,
      "SELECT count(*) FROM \"HousePlacements\" WHERE \"EntityId\" = 'guardian'"
    );
    var constraintCount = await ExecuteScalarAsync<long>(
      database,
      "SELECT count(*) FROM pg_constraint WHERE conname = 'CK_FamilyMembers_PendingInvitation'"
    );

    await Assert.That(pendingChildCount).IsEqualTo(0);
    await Assert.That(childPlacementCount).IsEqualTo(0);
    await Assert.That(guardianCount).IsEqualTo(1);
    await Assert.That(guardianPlacementCount).IsEqualTo(1);
    await Assert.That(constraintCount).IsEqualTo(1);
  }

  private static DbContextOptions<LadirchenDbContext> CreateOptions(string connectionString)
  {
    return new DbContextOptionsBuilder<LadirchenDbContext>()
      .UseNpgsql(connectionString)
      .Options;
  }

  private static async Task<T> ExecuteScalarAsync<T>(
    LadirchenDbContext database,
    string commandText
  )
  {
    await database.Database.OpenConnectionAsync();
    await using var command = database.Database.GetDbConnection().CreateCommand();
    command.CommandText = commandText;
    var value = await command.ExecuteScalarAsync()
      ?? throw new InvalidOperationException("The scalar query returned no value.");
    var converted = Convert.ChangeType(value, typeof(T));

    return converted is T result
      ? result
      : throw new InvalidOperationException("The scalar query returned an unexpected type.");
  }

  private static async Task<PostgreSqlContainer> StartPostgreSqlAsync()
  {
    var postgres = new PostgreSqlBuilder("postgres:18-alpine")
      .Build();
    await postgres.StartAsync();

    return postgres;
  }
}