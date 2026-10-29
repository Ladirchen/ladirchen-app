using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Avatar;

public sealed class AvatarAppearanceService
{
  private readonly AvatarCatalog _catalog;
  private readonly Dictionary<FamilyMemberId, AvatarAppearance> _appearances = new();

  public AvatarAppearanceService(AvatarCatalog catalog)
  {
    _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
  }

  public AvatarAppearance Resolve(FamilyMemberId memberId, AvatarProfile profile, int profileIndex)
  {
    ArgumentNullException.ThrowIfNull(memberId);

    if (_appearances.TryGetValue(memberId, out var saved))
    {
      return saved;
    }

    return Default(profile, profileIndex);
  }

  public void Save(
    FamilyMemberId signedInMemberId,
    FamilyMemberId targetMemberId,
    FamilyMemberId activeProfileId,
    AvatarProfile profile,
    AvatarAppearance appearance
  )
  {
    Rules.Require(
      signedInMemberId == targetMemberId && signedInMemberId == activeProfileId,
      DomainError.Forbidden,
      "Only the signed-in active profile may save its avatar."
    );
    _catalog.Validate(appearance, profile);
    _appearances[targetMemberId] = appearance;
  }

  public AvatarAppearance? SavedFor(FamilyMemberId memberId)
  {
    return _appearances.TryGetValue(memberId, out var appearance) ? appearance : null;
  }

  private static AvatarAppearance Default(AvatarProfile profile, int profileIndex)
  {
    var skin = profileIndex % 2 == 0 ? "skin-light" : "skin-medium";

    if (profile == AvatarProfile.Child)
    {
      return new("child", skin, "soft", "happy", "short", "hair-brown", "hoodie", "outfit-blue", "none", "none", "none");
    }

    var age = profileIndex % 2 == 0 ? "adult" : "senior";

    return new(age, skin, "oval", "happy", "short", "hair-black", "shirt", "outfit-green", "glasses", "none", "none");
  }
}
