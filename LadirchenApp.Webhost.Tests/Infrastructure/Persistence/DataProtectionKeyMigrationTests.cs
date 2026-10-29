using System.Threading.Tasks;

using LadirchenApp.Webhost.Infrastructure.Persistence;

using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Testcontainers.PostgreSql;

namespace LadirchenApp.Webhost.Tests.Infrastructure.Persistence;

public sealed class DataProtectionKeyMigrationTests
{
  [Test]
  public async Task LatestMigrationCreatesDataProtectionKeysAndPersistsKey()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    const string friendlyName = "integration-test-key";
    const string xml = "<key id=\"integration-test-key\" />";

    await using (var database = new LadirchenDbContext(options))
    {
      await database.Database.MigrateAsync();
      database.DataProtectionKeys.Add(
        new DataProtectionKey
        {
          FriendlyName = friendlyName,
          Xml = xml,
        }
      );
      await database.SaveChangesAsync();
    }

    await using var verificationDatabase = new LadirchenDbContext(options);
    var persistedKey = await verificationDatabase.DataProtectionKeys
      .AsNoTracking()
      .SingleAsync();

    await Assert.That(persistedKey.FriendlyName).IsEqualTo(friendlyName);
    await Assert.That(persistedKey.Xml).IsEqualTo(xml);
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
