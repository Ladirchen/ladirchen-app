using System;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure;
using LadirchenApp.Webhost.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Testcontainers.PostgreSql;

namespace LadirchenApp.Webhost.Tests.Infrastructure.Authentication;

public sealed class CurrentAuthenticationSessionReaderTests
{
  [Test]
  public async Task ReadsThePersistedAccountForTheAuthenticatedFamilyMember()
  {
    await using var postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();
    await postgres.StartAsync();
    var options = new DbContextOptionsBuilder<LadirchenDbContext>()
      .UseNpgsql(postgres.GetConnectionString())
      .Options;
    await using var database = new LadirchenDbContext(options);
    await database.Database.MigrateAsync();
    var familyId = Guid.NewGuid();
    const string memberId = "guardian-1";
    database.AuthenticationAccounts.Add(
      new AuthenticationAccountEntity
      {
        EmailAddress = "guardian@example.test",
        FamilyId = familyId,
        FamilyName = "Sun Family",
        MemberId = memberId,
        NormalizedEmail = "guardian@example.test",
        PasswordHash = [1],
        PasswordIterations = 1,
        PasswordSalt = [2],
        Username = "guardian",
      }
    );
    await database.SaveChangesAsync();
    var reader = new CurrentAuthenticationSessionReader(database);

    var session = await reader.ReadAsync(
      new FamilyId(familyId),
      new FamilyMemberId(memberId),
      CancellationToken.None
    );
    var missing = await reader.ReadAsync(
      new FamilyId(familyId),
      new FamilyMemberId("unknown-member"),
      CancellationToken.None
    );

    await Assert.That(session).IsNotNull();
    await Assert.That(session!.FamilyId).IsEqualTo(familyId);
    await Assert.That(session.FamilyName).IsEqualTo("Sun Family");
    await Assert.That(session.MemberId).IsEqualTo(memberId);
    await Assert.That(missing).IsNull();
  }
}
