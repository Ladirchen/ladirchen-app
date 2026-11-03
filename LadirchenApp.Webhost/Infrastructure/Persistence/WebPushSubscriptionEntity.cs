using System;
using System.Collections.Generic;

namespace LadirchenApp.Webhost.Infrastructure.Persistence;

public sealed class WebPushSubscriptionEntity
{
  public Guid SubscriptionId
  {
    get; set;
  }
  public Guid FamilyId
  {
    get; set;
  }
  public string MemberId { get; set; } = string.Empty;
  public string EndpointHash { get; set; } = string.Empty;
  public string Endpoint { get; set; } = string.Empty;
  public string P256dh { get; set; } = string.Empty;
  public string Auth { get; set; } = string.Empty;
  public string Locale { get; set; } = "de";
  public DateTimeOffset CreatedAt
  {
    get; set;
  }
  public DateTimeOffset UpdatedAt
  {
    get; set;
  }
  public ICollection<WebPushNotificationEntity> Notifications
  {
    get; set;
  } =
    new List<WebPushNotificationEntity>();
}