using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Avatar;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Family.Permissions;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed partial class GetFamilyAggregateQueryHandler
{
  private async Task<FamilyProfileReadModelProjection> ProjectFamilyProfileAsync(
    LoadedAggregate<Domain.Family.Family> loadedFamily,
    FamilyMemberId authenticatedMemberId,
    FamilyMemberId viewerId,
    CancellationToken cancellationToken
  )
  {
    var family = loadedFamily.Root;
    var loadedProgression = RequiredDependency(
      await _progressionRepository.LoadAsync(family, cancellationToken),
      "family progression"
    );
    var loadedCustomization = RequiredDependency(
      await _houseCustomizationRepository.LoadAsync(
        family,
        loadedProgression.Root.StoredLevel,
        cancellationToken
      ),
      "home customization"
    );

    return ProjectFamilyProfile(
      loadedFamily,
      loadedProgression,
      loadedCustomization,
      authenticatedMemberId,
      viewerId
    );
  }

  private static FamilyProfileReadModelProjection ProjectFamilyProfile(
    LoadedAggregate<Domain.Family.Family> loadedFamily,
    LoadedAggregate<Domain.House.FamilyHouse> loadedProgression,
    LoadedAggregate<FamilyHouseShop> loadedCustomization,
    FamilyMemberId authenticatedMemberId,
    FamilyMemberId viewerId
  )
  {
    var family = loadedFamily.Root;
    var permissionPolicy = new FamilyPermissionPolicy();
    var authenticatedPermissions = permissionPolicy.Resolve(
      family.ActiveMember(authenticatedMemberId)
    );
    var viewerPermissions = permissionPolicy.Resolve(family.ActiveMember(viewerId));
    var externalViewer = family.ActiveMember(viewerId).Access == GuardianAccess.External;
    var canManageInvitations = (
      authenticatedPermissions.InviteMembers && viewerPermissions.InviteMembers
    ) || (
      authenticatedPermissions.ManageFamily && viewerPermissions.ManageFamily
    );
    var members = family.Members
      .Where(member => canManageInvitations || !member.InvitationPending)
      .Where(member => !externalViewer || member.Id == viewerId || member.Role == MemberRole.Child)
      .ToArray();
    var emailVisibleMemberIds = canManageInvitations
      ? members.Select(member => member.Id).ToHashSet()
      : new[] { viewerId }.ToHashSet();
    var permissions = new FamilyPermissions(
      viewerPermissions.InviteMembers && authenticatedPermissions.InviteMembers,
      viewerPermissions.ManageGoals && authenticatedPermissions.ManageGoals,
      viewerPermissions.ManageContent && authenticatedPermissions.ManageContent,
      viewerPermissions.ManageFamily && authenticatedPermissions.ManageFamily,
      viewerPermissions.SupportGoals && authenticatedPermissions.SupportGoals,
      viewerPermissions.ViewFamilyGoals && authenticatedPermissions.ViewFamilyGoals,
      viewerPermissions.ViewGuardianGoals && authenticatedPermissions.ViewGuardianGoals,
      viewerPermissions.ManageChildTasks && authenticatedPermissions.ManageChildTasks,
      viewerPermissions.ReviewChildTasks && authenticatedPermissions.ReviewChildTasks,
      viewerPermissions.ViewTaskAssignments && authenticatedPermissions.ViewTaskAssignments,
      viewerPermissions.ViewHouse && authenticatedPermissions.ViewHouse,
      viewerPermissions.PurchaseHouseContentWithoutBalance
        && authenticatedPermissions.PurchaseHouseContentWithoutBalance,
      viewerPermissions.ManageShop && authenticatedPermissions.ManageShop,
      viewerPermissions.ReceivePushNotifications
        && authenticatedPermissions.ReceivePushNotifications
    );
    var weeklyStreaks = members.ToDictionary(
      member => member.Id,
      member => loadedProgression.Root.StoredWeeklyStreakFor(member.Id)
    );

    return new FamilyProfileReadModelProjection(
      loadedFamily.Revision,
      members,
      family.Pets,
      weeklyStreaks,
      emailVisibleMemberIds,
      new System.Collections.Generic.Dictionary<FamilyMemberId, FamilyPermissions>
      {
        [viewerId] = permissions,
      },
      new AvatarCatalog(),
      family.OnboardingCompleted,
      FormatSubscriptionTier(loadedCustomization.Root.SubscriptionTier),
      family.TimeZone
    );
  }

  private static string FormatSubscriptionTier(HouseSubscriptionTier tier)
  {
    return tier switch
    {
      HouseSubscriptionTier.Free => "free",
      HouseSubscriptionTier.Pro => "pro",
      _ => throw new ArgumentOutOfRangeException(nameof(tier)),
    };
  }
}
