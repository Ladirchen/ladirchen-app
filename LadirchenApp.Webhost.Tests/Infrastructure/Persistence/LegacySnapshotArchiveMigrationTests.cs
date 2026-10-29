using System;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Testcontainers.PostgreSql;

namespace LadirchenApp.Webhost.Tests.Infrastructure.Persistence;

public sealed class LegacySnapshotArchiveMigrationTests
{
  [Test]
  public async Task LegacySnapshotDataIsArchivedWithoutBeingDeleted()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    await using var database = new LadirchenDbContext(options);
    await database.Database.MigrateAsync("20261029181947_AddTypedAggregatePersistence");
    var familyId = Guid.NewGuid();
    const string stateJson = "{\"preserved\":true}";
    await database.Database.ExecuteSqlInterpolatedAsync(
      $"""
      INSERT INTO "FamilyAggregateSnapshots"
        ("FamilyId", "AggregateType", "Revision", "SchemaVersion", "StateJson", "UpdatedAt", "UpdatedBy")
      VALUES
        ({familyId}, {"family-profile"}, {1L}, {1}, {stateJson}, {DateTimeOffset.Parse("2026-10-29T12:00:00Z")}, {"guardian"});
      """
    );

    await database.Database.MigrateAsync();
    await database.Database.OpenConnectionAsync();
    await using var command = database.Database.GetDbConnection().CreateCommand();
    command.CommandText =
      "SELECT \"StateJson\" FROM \"LegacyFamilyAggregateSnapshots\" WHERE \"FamilyId\" = @familyId";
    var parameter = command.CreateParameter();
    parameter.ParameterName = "familyId";
    parameter.Value = familyId;
    command.Parameters.Add(parameter);
    var archivedState = await command.ExecuteScalarAsync();

    await Assert.That(archivedState).IsEqualTo(stateJson);
  }

  private static DbContextOptions<LadirchenDbContext> CreateOptions(string connectionString)
  {
    return new DbContextOptionsBuilder<LadirchenDbContext>()
      .UseNpgsql(connectionString)
      .Options;
  }

  private static async Task<PostgreSqlContainer> StartPostgreSqlAsync()
  {
    var postgres = new PostgreSqlBuilder("postgres:18-alpine")
      .Build();
    await postgres.StartAsync();

    return postgres;
  }
}
