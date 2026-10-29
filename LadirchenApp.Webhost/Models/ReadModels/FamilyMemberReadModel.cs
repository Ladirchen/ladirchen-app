using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record FamilyMemberReadModel(
  [property: JsonRequired] string Id,
  [property: JsonRequired] string Name,
  string? Nickname,
  [property: JsonRequired] string Avatar,
  [property: JsonRequired] string Color,
  [property: JsonRequired] string Role,
  string? GuardianAccess,
  bool? ParticipatesInWeeklyGoal,
  [property: JsonRequired] int WeeklyStreak,
  string? Email,
  bool? InvitationPending,
  AvatarAppearanceReadModel? Appearance,
  FamilyPermissionsReadModel? Permissions
);
