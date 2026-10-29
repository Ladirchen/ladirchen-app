using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record FamilyPermissionsReadModel(
  [property: JsonRequired] bool CanInviteMembers,
  [property: JsonRequired] bool CanManageContent,
  [property: JsonRequired] bool CanManageFamily,
  [property: JsonRequired] bool CanManageGoals,
  [property: JsonRequired] bool CanSupportChildGoals,
  [property: JsonRequired] bool CanViewFamilyGoals,
  [property: JsonRequired] bool CanViewGuardianGoals
);
