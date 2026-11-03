using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using WebPush;

namespace LadirchenApp.Webhost.Infrastructure;

public sealed class WebPushNotificationDispatcher : BackgroundService
{
  private const int BatchSize = 50;
  private static readonly TimeSpan _dispatchInterval = TimeSpan.FromSeconds(1);
  private readonly WebPushConfiguration? _configuration;
  private readonly ILogger<WebPushNotificationDispatcher> _logger;
  private readonly IServiceScopeFactory _scopeFactory;
  private readonly TimeProvider _timeProvider;

  public WebPushNotificationDispatcher(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<WebPushNotificationDispatcher> logger
  )
  {
    _configuration = WebPushConfiguration.Resolve(configuration);
    _scopeFactory = scopeFactory;
    _timeProvider = timeProvider;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    if (_configuration is null)
    {
      _logger.LogWarning(
        "Web Push dispatch is disabled because the VAPID environment variables are missing."
      );

      return;
    }

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
      _logger.LogError(exception, "Dispatching Web Push notifications failed.");
    }
  }

  private async Task DispatchBatchAsync(CancellationToken cancellationToken)
  {
    await using var scope = _scopeFactory.CreateAsyncScope();
    var database = scope.ServiceProvider.GetRequiredService<LadirchenDbContext>();
    var now = _timeProvider.GetUtcNow();
    var messages = await database.WebPushNotifications
      .Include(item => item.Subscription)
      .Where(item =>
        item.PublishedAt == null
        && (item.NextAttemptAt == null || item.NextAttemptAt <= now)
      )
      .OrderBy(item => item.OccurredAt)
      .Take(BatchSize)
      .ToArrayAsync(cancellationToken);
    using var client = new WebPushClient();
    var vapid = new VapidDetails(
      _configuration!.Subject,
      _configuration.PublicKey,
      _configuration.PrivateKey
    );

    foreach (var message in messages)
    {
      try
      {
        var subscription = new PushSubscription(
          message.Subscription.Endpoint,
          message.Subscription.P256dh,
          message.Subscription.Auth
        );
        var useEnglish = string.Equals(
          message.Subscription.Locale,
          "en",
          StringComparison.OrdinalIgnoreCase
        );
        var payload = JsonSerializer.Serialize(new
        {
          body = useEnglish ? message.BodyEn : message.BodyDe,
          tag = message.Tag,
          title = useEnglish ? message.TitleEn : message.TitleDe,
          url = message.Url,
        });
        await client.SendNotificationAsync(subscription, payload, vapid, cancellationToken);
        message.PublishedAt = _timeProvider.GetUtcNow();
        message.LastError = null;
        message.NextAttemptAt = null;
      }
      catch (WebPushException exception) when (
        exception.StatusCode is HttpStatusCode.Gone or HttpStatusCode.NotFound
      )
      {
        database.WebPushSubscriptions.Remove(message.Subscription);
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
          "Publishing Web Push message {MessageId} failed on attempt {Attempt}.",
          message.MessageId,
          message.PublishAttempts
        );
      }

      await database.SaveChangesAsync(cancellationToken);
    }
  }

  private static TimeSpan RetryDelay(int attempts)
  {
    var seconds = Math.Min(Math.Pow(2, Math.Min(attempts, 8)), 300);

    return TimeSpan.FromSeconds(seconds);
  }

  private static string Truncate(string value, int maximumLength)
  {
    return value.Length <= maximumLength ? value : value[..maximumLength];
  }
}