using System.Threading.Tasks;

using LadirchenApp.Webhost.Infrastructure.Persistence;

using Npgsql;

namespace LadirchenApp.Webhost.Tests.Infrastructure.Persistence;

public sealed class PostgreSqlConnectionStringTests
{
  [Test]
  public async Task VercelStyleDatabaseUrlIsConvertedForNpgsql()
  {
    var normalized = PostgreSqlConnectionString.Normalize(
      "postgresql://app%40user:p%40ss@ep-example-pooler.eu-central-1.aws.neon.tech:5432/ladirchen?sslmode=require&channel_binding=require"
    );
    var parsed = new NpgsqlConnectionStringBuilder(normalized);

    await Assert.That(parsed.Host).IsEqualTo("ep-example-pooler.eu-central-1.aws.neon.tech");
    await Assert.That(parsed.Database).IsEqualTo("ladirchen");
    await Assert.That(parsed.Username).IsEqualTo("app@user");
    await Assert.That(parsed.Password).IsEqualTo("p@ss");
    await Assert.That(parsed.SslMode.ToString()).IsEqualTo("Require");
    await Assert.That(parsed.ChannelBinding.ToString()).IsEqualTo("Require");
    await Assert.That(parsed.Pooling).IsTrue();
    await Assert.That(parsed.MaxPoolSize).IsEqualTo(20);
  }

  [Test]
  public async Task NativeNpgsqlConnectionStringRemainsSupported()
  {
    var normalized = PostgreSqlConnectionString.Normalize(
      "Host=localhost;Database=ladirchen;Username=app;Password=secret;Maximum Pool Size=7"
    );
    var parsed = new NpgsqlConnectionStringBuilder(normalized);

    await Assert.That(parsed.Host).IsEqualTo("localhost");
    await Assert.That(parsed.Database).IsEqualTo("ladirchen");
    await Assert.That(parsed.MaxPoolSize).IsEqualTo(7);
    await Assert.That(parsed.ApplicationName).IsEqualTo("LadirchenApp");
  }
}
