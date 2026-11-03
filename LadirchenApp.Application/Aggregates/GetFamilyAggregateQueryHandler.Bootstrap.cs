using System;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Domain.Family.Permissions;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed partial class GetFamilyAggregateQueryHandler
{
  public async Task<FamilyBootstrapReadModelProjection?> HandleBootstrapAsync(
    GetFamilyBootstrapQuery query,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(query);
    var loadedFamily = await _familyRepository.LoadAsync(query.FamilyId, cancellationToken);

    if (loadedFamily is null)
    {
      return null;
    }

    var family = loadedFamily.Root;
    family.RequireActorAccess(query.AuthenticatedMemberId, query.ViewerId);
    var loadedEconomy = RequiredDependency(
      await _economyRepository.LoadAsync(family, cancellationToken),
      "family economy"
    );
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
    var viewerPermissions = new FamilyPermissionPolicy().Resolve(
      family.ActiveMember(query.ViewerId)
    );
    var contributions = ProjectContributions(family, loadedEconomy, query.ViewerId);
    var profile = ProjectFamilyProfile(
      loadedFamily,
      loadedProgression,
      loadedCustomization,
      query.AuthenticatedMemberId,
      query.ViewerId
    );
    var rewardShop = ProjectRewardShop(
      family,
      loadedEconomy,
      query.AuthenticatedMemberId,
      query.ViewerId
    );
    var savings = ProjectSavings(
      family,
      loadedEconomy,
      loadedProgression,
      query.ViewerId
    );
    var progression = viewerPermissions.ViewHouse
      ? ProjectFamilyProgression(family, loadedProgression, loadedEconomy)
      : null;
    var home = viewerPermissions.ViewHouse
      ? ProjectHomeCustomization(
        family,
        loadedProgression,
        loadedCustomization,
        loadedEconomy,
        query.AuthenticatedMemberId
      )
      : null;

    return new FamilyBootstrapReadModelProjection(
      contributions,
      profile,
      progression,
      home,
      rewardShop,
      savings
    );
  }
}
