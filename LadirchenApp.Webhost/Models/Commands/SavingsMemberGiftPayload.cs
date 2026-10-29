using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record SavingsMemberGiftPayload(
  [property: JsonRequired] string RecipientId,
  [property: JsonRequired] decimal Amount,
  [property: JsonRequired] string GiftId,
  string? SenderName
);
