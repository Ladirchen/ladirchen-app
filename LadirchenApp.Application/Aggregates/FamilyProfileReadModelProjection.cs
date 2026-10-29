using System.Collections.Generic;

using LadirchenApp.Domain.Avatar;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Family.Permissions;
using LadirchenApp.Domain.Family.Setup;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed record FamilyProfileReadModelProjection(
  long Revision,
  IReadOnlyList<FamilyMember> Members,
  IReadOnlyList<FamilyPetState> Pets,
  IReadOnlyDictionary<FamilyMemberId, int> WeeklyStreaks,
  IReadOnlySet<FamilyMemberId> EmailVisibleMemberIds,
  IReadOnlyDictionary<FamilyMemberId, FamilyPermissions> Permissions,
  AvatarCatalog AvatarCatalog,
  bool OnboardingCompleted,
  string SubscriptionTier,
  IanaTimeZone TimeZone
) : IFamilyReadModelProjection
{
  public string ReadModelType
  {
    get
    {
      return "family-profile";
    }
  }
}
