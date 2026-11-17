using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record ShopTimerPayload([property: JsonRequired] string RequestId);
