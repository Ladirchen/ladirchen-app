using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record InviteGuardianPayload(
  [property: JsonRequired] string Id,
  [property: JsonRequired] string Name,
  [property: JsonRequired] string Email,
  [property: JsonRequired] string GuardianAccess
);
