using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record SavingsGoalGiftPayload(
  [property: JsonRequired] string Id,
  [property: JsonRequired] decimal Amount,
  string? GiftId,
  string? SenderName
);
