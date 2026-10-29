using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Infrastructure;

using Microsoft.Extensions.Configuration;

namespace LadirchenApp.Webhost.Tests.Hosting;

public sealed class HostingEndpointResolverTests
{
  [Test]
  public async Task WebEndpointUsesVercelPortWhenConfigured()
  {
    var configuration = CreateConfiguration(("PORT", "4317"));

    var url = HostingEndpointResolver.ResolveWebUrl(configuration);

    await Assert.That(url).IsEqualTo("http://0.0.0.0:4317");
  }

  [Test]
  public async Task WebEndpointPreservesFrameworkDefaultsWithoutVercelPort()
  {
    var configuration = CreateConfiguration();

    var url = HostingEndpointResolver.ResolveWebUrl(configuration);

    await Assert.That(url).IsNull();
  }

  [Test]
  public async Task DaemonEndpointUsesDedicatedDefaultHealthPort()
  {
    var url = HostingEndpointResolver.ResolveDaemonUrl();

    await Assert.That(url).IsEqualTo("http://0.0.0.0:8081");
  }

  [Test]
  public async Task InvalidPortIsRejectedBeforeHostStartup()
  {
    var configuration = CreateConfiguration(("PORT", "70000"));

    await Assert.That(() =>
      HostingEndpointResolver.ResolveWebUrl(configuration)
    ).Throws<InvalidOperationException>();
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
