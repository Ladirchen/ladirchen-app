using System.Collections.Generic;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;

namespace LadirchenApp.Webhost.Tests.Infrastructure;

public sealed class ForwardedHeadersConfigurationTests
{
  [Test]
  public async Task ForwardedHeadersAreEnabledOnlyInsideVercel()
  {
    var vercel = CreateConfiguration(("VERCEL", "1"));
    var local = CreateConfiguration();

    await Assert.That(ForwardedHeadersConfiguration.IsEnabled(vercel)).IsTrue();
    await Assert.That(ForwardedHeadersConfiguration.IsEnabled(local)).IsFalse();
  }

  [Test]
  public async Task VercelProxyConfigurationTrustsOneForwardingHop()
  {
    var options = new ForwardedHeadersOptions();

    ForwardedHeadersConfiguration.Configure(options);

    await Assert.That(options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedFor)).IsTrue();
    await Assert.That(options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedProto)).IsTrue();
    await Assert.That(options.ForwardLimit).IsEqualTo(1);
    await Assert.That(options.KnownIPNetworks).IsEmpty();
    await Assert.That(options.KnownProxies).IsEmpty();
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
