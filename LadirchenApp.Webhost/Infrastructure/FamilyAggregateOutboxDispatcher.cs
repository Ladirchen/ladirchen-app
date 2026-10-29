using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Infrastructure.Persistence;
using LadirchenApp.Webhost.Models;
using LadirchenApp.Webhost.Realtime;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LadirchenApp.Webhost.Infrastructure;

public sealed class FamilyAggregateOutboxDispatcher : BackgroundService
{
  private static readonly TimeSpan _dispatchInterval = TimeSpan.FromSeconds(1);
  private const int BatchSize = 50;
  private readonly ILogger<FamilyAggregateOutboxDispatcher> _logger;
  private readonly IServiceScopeFactory _scopeFactory;
  private readonly TimeProvider _timeProvider;

  public FamilyAggregateOutboxDispatcher(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<FamilyAggregateOutboxDispatcher> logger
  )
  {
    _scopeFactory = scopeFactory;
    _timeProvider = timeProvider;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    try
    {
      await GracefulShutdownOperation.RunAsync(DispatchSafelyAsync, stoppingToken);
      using var timer = new PeriodicTimer(_dispatchInterval);

      while (await timer.WaitForNextTickAsync(stoppingToken))
      {
        await GracefulShutdownOperation.RunAsync(DispatchSafelyAsync, stoppingToken);
      }
    }
    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
  }

  private async Task DispatchSafelyAsync(CancellationToken cancellationToken)
  {
    try
    {
      await DispatchBatchAsync(cancellationToken);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Dispatching family aggregate notifications from the outbox failed.");
    }
  }

  private async Task DispatchBatchAsync(CancellationToken cancellationToken)
  {
    await using var scope = _scopeFactory.CreateAsyncScope();
    var database = scope.ServiceProvider.GetRequiredService<LadirchenDbContext>();
    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<FamilyUpdatesHub>>();
    var now = _timeProvider.GetUtcNow();
    var messages = await database.FamilyAggregateOutboxMessages
      .Where(message =>
        message.PublishedAt == null
        && (message.NextAttemptAt == null || message.NextAttemptAt <= now)
      )
      .OrderBy(message => message.OccurredAt)
      .Take(BatchSize)
      .ToArrayAsync(cancellationToken);

    foreach (var message in messages)
    {
      try
      {
        await hubContext.Clients
          .Group(FamilyUpdatesHub.GroupName(message.FamilyId))
          .SendAsync(
            "FamilyAggregateChanged",
            new FamilyAggregateChangedMessage(
              message.FamilyId.ToString("D"),
              message.AggregateType,
              message.Revision,
              message.UpdatedBy,
              message.UpdatedAt
            ),
            cancellationToken
          );
        message.PublishedAt = _timeProvider.GetUtcNow();
        message.LastError = null;
        message.NextAttemptAt = null;
      }
      catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
      {
        throw;
      }
      catch (Exception exception)
      {
        message.PublishAttempts = checked(message.PublishAttempts + 1);
        message.LastError = Truncate(exception.Message, 2000);
        message.NextAttemptAt = _timeProvider.GetUtcNow() + RetryDelay(message.PublishAttempts);
        _logger.LogWarning(
          exception,
          "Publishing outbox message {MessageId} for family {FamilyId} failed on attempt {Attempt}.",
          message.MessageId,
          message.FamilyId,
          message.PublishAttempts
        );
      }

      await database.SaveChangesAsync(cancellationToken);
    }
  }

  private static TimeSpan RetryDelay(int attempts)
  {
    var seconds = Math.Min(Math.Pow(2, Math.Min(attempts, 6)), 60);

    return TimeSpan.FromSeconds(seconds);
  }

  private static string Truncate(string value, int maximumLength)
  {
    return value.Length <= maximumLength ? value : value[..maximumLength];
  }
}
