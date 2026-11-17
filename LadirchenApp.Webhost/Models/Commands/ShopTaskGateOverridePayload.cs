using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record ShopTaskGateOverridePayload(
  [property: JsonRequired] string MemberId,
  [property: JsonRequired] bool Enabled
);