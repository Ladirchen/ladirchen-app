using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Extensions;
using LadirchenApp.Webhost.Infrastructure.Persistence;
using LadirchenApp.Webhost.Tests.Infrastructure;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Testcontainers.PostgreSql;

namespace LadirchenApp.Webhost.Tests.Infrastructure.Persistence;

public sealed class DataProtectionPersistenceTests
{
  [Test]
  public async Task FreshServiceProviderCanUnprotectPayloadWithPostgreSqlKeyRing()
  {
    await using var postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();
    await postgres.StartAsync();
    var connectionString = postgres.GetConnectionString();
    const string certificatePassword = "test-certificate-password";
    var configuration = CreateConfiguration(
      ("DATA_PROTECTION_CERTIFICATE_BASE64", TestCertificateFactory.CreateEncodedPkcs12(certificatePassword)),
      ("DATA_PROTECTION_CERTIFICATE_PASSWORD", certificatePassword)
    );
    await MigrateAsync(connectionString);
    string protectedPayload;

    await using (var firstProvider = CreateServiceProvider(connectionString, configuration))
    {
      var protector = firstProvider
        .GetRequiredService<IDataProtectionProvider>()
        .CreateProtector("persistent-cookie-test");
      protectedPayload = protector.Protect("family-session");
    }

    await AssertKeyIsEncryptedAtRestAsync(connectionString);

    await using (var secondProvider = CreateServiceProvider(connectionString, configuration))
    {
      var protector = secondProvider
        .GetRequiredService<IDataProtectionProvider>()
        .CreateProtector("persistent-cookie-test");
      var unprotectedPayload = protector.Unprotect(protectedPayload);

      await Assert.That(unprotectedPayload).IsEqualTo("family-session");
    }
  }

  private static async Task AssertKeyIsEncryptedAtRestAsync(string connectionString)
  {
    var options = new DbContextOptionsBuilder<LadirchenDbContext>()
      .UseNpgsql(connectionString)
      .Options;
    await using var database = new LadirchenDbContext(options);
    var keyXml = await database.DataProtectionKeys
      .AsNoTracking()
      .Select(key => key.Xml)
      .SingleAsync();

    await Assert.That(keyXml).Contains("encryptedSecret");
    await Assert.That(keyXml).DoesNotContain("<masterKey");
  }

  private static IConfiguration CreateConfiguration(params (string Key, string Value)[] values)
  {
    var settings = new Dictionary<string, string?>();

    foreach (var (key, value) in values)
    {
      settings[key] = value;
    }

    return new ConfigurationBuilder()
      .AddInMemoryCollection(settings)
      .Build();
  }

  private static ServiceProvider CreateServiceProvider(
    string connectionString,
    IConfiguration configuration
  )
  {
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddDbContext<LadirchenDbContext>(options =>
      options.UseNpgsql(connectionString)
    );
    services.AddLadirchenDataProtection(configuration);

    return services.BuildServiceProvider(validateScopes: true);
  }

  private static async Task MigrateAsync(string connectionString)
  {
    var options = new DbContextOptionsBuilder<LadirchenDbContext>()
      .UseNpgsql(connectionString)
      .Options;
    await using var database = new LadirchenDbContext(options);
    await database.Database.MigrateAsync();
  }
}
