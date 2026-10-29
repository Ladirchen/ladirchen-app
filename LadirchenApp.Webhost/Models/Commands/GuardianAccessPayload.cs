using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record GuardianAccessPayload(
  [property: JsonRequired] string MemberId,
  [property: JsonRequired] string GuardianAccess
);
