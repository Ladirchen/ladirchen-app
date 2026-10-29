using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Extensions;
using LadirchenApp.Webhost.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace LadirchenApp.Webhost.Tests.Hosting;

public sealed class ProcessRoleServiceRegistrationTests
{
  [Test]
  public async Task WebRoleDoesNotRunScheduledBackendWorkers()
  {
    var builder = CreateBuilder();

    builder.SetupLadierchenWebhost(new AppSettings());
    using var services = builder.Services.BuildServiceProvider();
    var hostOptions = services.GetRequiredService<IOptions<HostOptions>>().Value;

    await Assert.That(HasHostedService<FamilyAggregateOutboxDispatcher>(builder)).IsFalse();
    await Assert.That(HasHostedService<WeeklyInterestSettlementService>(builder)).IsFalse();
    await Assert.That(hostOptions.ShutdownTimeout).IsEqualTo(System.TimeSpan.FromSeconds(25));
  }

  [Test]
  public async Task DaemonRoleOwnsScheduledBackendWorkersAndGracefulShutdownWindow()
  {
    var builder = CreateBuilder();
    builder.Configuration["REDIS_URL"] = "localhost:6379";

    builder.SetupLadierchenDaemon();
    using var services = builder.Services.BuildServiceProvider();
    var hostOptions = services.GetRequiredService<IOptions<HostOptions>>().Value;

    await Assert.That(HasHostedService<FamilyAggregateOutboxDispatcher>(builder)).IsTrue();
    await Assert.That(HasHostedService<WeeklyInterestSettlementService>(builder)).IsTrue();
    await Assert.That(hostOptions.ServicesStopConcurrently).IsTrue();
    await Assert.That(hostOptions.ShutdownTimeout).IsEqualTo(System.TimeSpan.FromSeconds(55));
  }

  private static WebApplicationBuilder CreateBuilder()
  {
    var builder = WebApplication.CreateBuilder(
      new WebApplicationOptions
      {
        EnvironmentName = Environments.Development,
      }
    );
    builder.Configuration["ConnectionStrings:Ladirchen"] =
      "Host=localhost;Database=ladirchen;Username=ladirchen;Password=ladirchen";

    return builder;
  }

  private static bool HasHostedService<TService>(WebApplicationBuilder builder)
  {
    return builder.Services.Any(descriptor =>
      descriptor.ServiceType == typeof(IHostedService)
      && descriptor.ImplementationType == typeof(TService)
    );
  }
}
