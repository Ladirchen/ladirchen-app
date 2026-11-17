using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Commands;
using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Shared;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LadirchenApp.Webhost.Infrastructure;

public sealed class TimeRewardExpirationService : BackgroundService
{
  private static readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30);
  private readonly ILogger<TimeRewardExpirationService> _logger;
  private readonly IServiceScopeFactory _scopeFactory;

  public TimeRewardExpirationService(
    IServiceScopeFactory scopeFactory,
    ILogger<TimeRewardExpirationService> logger
  )
  {
    _scopeFactory = scopeFactory;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    try
    {
      await ExpireTimersSafelyAsync(stoppingToken);
      using var timer = new PeriodicTimer(_pollingInterval);

      while (await timer.WaitForNextTickAsync(stoppingToken))
      {
        await ExpireTimersSafelyAsync(stoppingToken);
      }
    }
    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
  }

  private async Task ExpireTimersSafelyAsync(CancellationToken cancellationToken)
  {
    try
    {
      await using var scope = _scopeFactory.CreateAsyncScope();
      var familyRepository = scope.ServiceProvider.GetRequiredService<IFamilyRepository>();
      var economyRepository = scope.ServiceProvider.GetRequiredService<IFamilyEconomyRepository>();
      var commandHandler = scope.ServiceProvider.GetRequiredService<ExecuteFamilyCommandHandler>();
      var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
      var familyIds = await familyRepository.ListIdsAsync(cancellationToken);

      foreach (var familyId in familyIds)
      {
        var family = await familyRepository.LoadAsync(familyId, cancellationToken);

        if (family is null)
        {
          continue;
        }

        var economy = await economyRepository.LoadAsync(family.Root, cancellationToken);

        if (economy is null)
        {
          continue;
        }

        var administratorId = family.Root.Members.Single(member => member.IsAdministrator).Id;
        var now = timeProvider.GetUtcNow();
        var expiredRequestIds = economy.Root.RedemptionRequests
          .Where(request => request.Status == RedemptionStatus.Approved)
          .Where(request => request.TimerStartedAt is not null && request.TimerCancelledAt is null)
          .Where(request => request.RemainingSeconds(now) == 0)
          .Select(request => request.Id)
          .ToArray();

        foreach (var requestId in expiredRequestIds)
        {
          var command = new FamilyCommand(
            CreateCommandId(familyId, requestId),
            familyId,
            administratorId,
            "reward-shop.expire-timer",
            JsonSerializer.SerializeToElement(new { requestId = requestId.Value })
          );

          try
          {
            await commandHandler.HandleAsync(command, cancellationToken);
          }
          catch (FamilyAggregateConflictException)
          {
            // A concurrent family command won. The next polling pass re-evaluates the timer.
          }
        }
      }
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    catch (Exception exception)
    {
      _logger.LogError(exception, "The scheduled time-reward expiration failed.");
    }
  }

  private static Guid CreateCommandId(FamilyId familyId, RedemptionRequestId requestId)
  {
    var source = Encoding.UTF8.GetBytes($"time-reward-expired:{familyId.Value:D}:{requestId.Value}");
    var hash = SHA256.HashData(source);

    return new Guid(hash.AsSpan(0, 16));
  }
}
