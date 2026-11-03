using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence;

public sealed class WebPushNotificationEntity
{
  public Guid MessageId
  {
    get; set;
  }
  public Guid SubscriptionId
  {
    get; set;
  }
  public Guid FamilyId
  {
    get; set;
  }
  public string MemberId { get; set; } = string.Empty;
  public string TitleDe { get; set; } = string.Empty;
  public string BodyDe { get; set; } = string.Empty;
  public string TitleEn { get; set; } = string.Empty;
  public string BodyEn { get; set; } = string.Empty;
  public string Url { get; set; } = string.Empty;
  public string Tag { get; set; } = string.Empty;
  public DateTimeOffset OccurredAt
  {
    get; set;
  }
  public DateTimeOffset? PublishedAt
  {
    get; set;
  }
  public DateTimeOffset? NextAttemptAt
  {
    get; set;
  }
  public int PublishAttempts
  {
    get; set;
  }
  public string? LastError
  {
    get; set;
  }
  public WebPushSubscriptionEntity Subscription { get; set; } = null!;
}