using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record ShopRewardReadModel(
  [property: JsonRequired] string Id,
  string? TranslationKey,
  [property: JsonRequired] string Title,
  [property: JsonRequired] string Description,
  [property: JsonRequired] string Icon,
  [property: JsonRequired] decimal Price,
  [property: JsonRequired] string Category,
  [property: JsonRequired] int Quantity,
  [property: JsonRequired] string Conditions,
  string? AvailableFrom,
  string? AvailableUntil,
  bool? IsVisible,
  [property: JsonRequired] string Status,
  string? RequesterId,
  bool? Published,
  string? PublicationStatus,
  string? NextStatusChangeAt,
  IReadOnlyList<string>? RequestableByMemberIds,
  IReadOnlyList<string>? CancellableByMemberIds,
  IReadOnlyList<string>? ApprovableByMemberIds
);
