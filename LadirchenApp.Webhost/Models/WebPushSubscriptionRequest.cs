using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models;

public sealed record WebPushSubscriptionRequest(
  [property: JsonRequired] string Endpoint,
  [property: JsonRequired] string P256dh,
  [property: JsonRequired] string Auth,
  string? Locale
);