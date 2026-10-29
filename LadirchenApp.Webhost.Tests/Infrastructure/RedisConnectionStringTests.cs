using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Infrastructure;

using Microsoft.Extensions.Configuration;

namespace LadirchenApp.Webhost.Tests.Infrastructure;

public sealed class RedisConnectionStringTests
{
  [Test]
  public async Task ProviderRedisUrlIsConvertedToTypedRedisOptions()
  {
    var options = RedisConnectionString.Normalize(
      "rediss://default:p%40ss@redis.example.test:6380/2"
    );
    var endpoint = options.EndPoints.Single() as DnsEndPoint
      ?? throw new InvalidOperationException("The Redis endpoint was not a DNS endpoint.");

    await Assert.That(endpoint.Host).IsEqualTo("redis.example.test");
    await Assert.That(endpoint.Port).IsEqualTo(6380);
    await Assert.That(options.User).IsEqualTo("default");
    await Assert.That(options.Password).IsEqualTo("p@ss");
    await Assert.That(options.Ssl).IsTrue();
    await Assert.That(options.DefaultDatabase).IsEqualTo(2);
    await Assert.That(options.AbortOnConnectFail).IsFalse();
    await Assert.That(options.ChannelPrefix.ToString()).IsEqualTo(RedisConnectionString.SignalRChannelPrefix);
  }

  [Test]
  public async Task NativeStackExchangeRedisConfigurationRemainsSupported()
  {
    var options = RedisConnectionString.Normalize(
      "redis.example.test:6379,user=app,password=secret,ssl=false"
    );
    var endpoint = options.EndPoints.Single() as DnsEndPoint
      ?? throw new InvalidOperationException("The Redis endpoint was not a DNS endpoint.");

    await Assert.That(endpoint.Host).IsEqualTo("redis.example.test");
    await Assert.That(endpoint.Port).IsEqualTo(6379);
    await Assert.That(options.User).IsEqualTo("app");
    await Assert.That(options.Password).IsEqualTo("secret");
    await Assert.That(options.ClientName).IsEqualTo("LadirchenApp");
  }

  [Test]
  public async Task MissingProductionBackplaneIsRejected()
  {
    var configuration = CreateConfiguration();

    await Assert.That(() =>
      RedisConnectionString.Resolve(configuration, required: true)
    ).Throws<InvalidOperationException>();
  }

  [Test]
  public async Task PreviewWebProcessCanRunWithoutRedisBackplane()
  {
    var configuration = CreateConfiguration(("VERCEL_ENV", "preview"));

    var required = RedisConnectionString.IsRequiredForWeb(configuration);
    var options = RedisConnectionString.Resolve(configuration, required);

    await Assert.That(required).IsFalse();
    await Assert.That(options).IsNull();
  }

  [Test]
  public async Task ProductionWebProcessRequiresRedisBackplane()
  {
    var configuration = CreateConfiguration(("VERCEL_ENV", "production"));

    var required = RedisConnectionString.IsRequiredForWeb(configuration);

    await Assert.That(required).IsTrue();
  }

  [Test]
  public async Task UnsupportedRedisUrlQueryIsRejectedInsteadOfIgnored()
  {
    await Assert.That(() =>
      RedisConnectionString.Normalize("rediss://default:secret@redis.example.test:6380?unknown=true")
    ).Throws<ArgumentException>();
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
}
