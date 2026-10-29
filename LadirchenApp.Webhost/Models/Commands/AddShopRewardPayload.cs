using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record AddShopRewardPayload(
  [property: JsonRequired] string Id,
  [property: JsonRequired] string Title,
  [property: JsonRequired] string Description,
  [property: JsonRequired] string Icon,
  [property: JsonRequired] decimal Price,
  [property: JsonRequired] string Category,
  [property: JsonRequired] int Quantity,
  [property: JsonRequired] string Conditions,
  string? AvailableFrom,
  string? AvailableUntil,
  bool? IsVisible
);
