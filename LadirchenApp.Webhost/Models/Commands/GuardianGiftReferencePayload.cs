using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record GuardianGiftReferencePayload([property: JsonRequired] string GiftId);
