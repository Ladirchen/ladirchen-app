using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models;

public sealed record CreateMemberInvitationRequest(
  [property: JsonRequired] string Email,
  string? GuardianAccess,
  [property: JsonRequired] string Name,
  [property: JsonRequired] string Role
);
