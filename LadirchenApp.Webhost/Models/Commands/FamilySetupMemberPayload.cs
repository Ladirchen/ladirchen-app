using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record FamilySetupMemberPayload(
  [property: JsonRequired] string Id,
  [property: JsonRequired] string Name,
  [property: JsonRequired] string Avatar,
  [property: JsonRequired] string Color,
  [property: JsonRequired] string Role,
  string? GuardianAccess,
  string? Nickname,
  string? Email,
  bool? InvitationPending,
  bool? ParticipatesInWeeklyGoal,
  AvatarAppearancePayload? Appearance
);
