using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Extensions;
using LadirchenApp.Webhost.Helpers;
using LadirchenApp.Webhost.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;

namespace LadirchenApp.Webhost.Tests.Hosting;

public sealed class AboutInfoEndpointTests
{
  [Test]
  public async Task AboutEndpointReturnsUncachedWebhostAssemblyMetadata()
  {
    var builder = WebApplication.CreateSlimBuilder();
    await using var app = builder.Build();
    app.MapAboutInfo();
    app.Urls.Add("http://127.0.0.1:0");
    await app.StartAsync();
    var addresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!;
    using var client = new HttpClient { BaseAddress = new Uri(addresses.Addresses.Single()) };

    using var response = await client.GetAsync("/about");
    var info = await response.Content.ReadFromJsonAsync<AboutInfo>();
    var expected = typeof(AssemblyHelper).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    await Assert.That(response.Headers.CacheControl!.NoStore).IsTrue();
    await Assert.That(info!.Name).IsEqualTo("LadirchenApp.Webhost");
    await Assert.That(info.InformationalVersion).IsEqualTo(expected);
    await Assert.That(info.SemVer).IsEqualTo(expected.Split('+')[0]);
  }
}
