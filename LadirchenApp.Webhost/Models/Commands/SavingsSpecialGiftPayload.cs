using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record SavingsSpecialGiftPayload(
  [property: JsonRequired] string ChildId,
  [property: JsonRequired] decimal Amount,
  [property: JsonRequired] string GiftId,
  [property: JsonRequired] string Reason,
  string? SenderName
);
