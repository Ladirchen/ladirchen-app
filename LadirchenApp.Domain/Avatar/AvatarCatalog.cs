using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Avatar;

public sealed class AvatarCatalog
{
  private static readonly IReadOnlyDictionary<string, string[]> _values = new Dictionary<string, string[]>
  {
    ["age"] = ["child", "adult", "senior"],
    ["skin tone"] = ["skin-light", "skin-medium", "skin-tan", "skin-deep"],
    ["face shape"] = ["soft", "round", "oval", "angular"],
    ["face"] = ["happy", "freckles", "wink", "surprised", "confident", "dreamy", "silly", "sparkle"],
    ["hair"] = ["short", "waves", "ponytail", "curls", "afro", "bob", "braids", "bun", "space-buns", "undercut", "popstar-space-buns", "forest-braid"],
    ["hair color"] = ["hair-black", "hair-brown", "hair-auburn", "hair-blonde", "hair-purple", "hair-blue", "hair-gray", "hair-silver", "hair-white"],
    ["outfit"] = ["superhero", "dinosaur", "monster", "vampire", "shark", "robot", "space", "fairy", "hoodie", "overalls", "explorer", "party", "sporty", "pajamas", "glitter-stage-jacket", "forest-adventurer", "shirt", "blouse", "cardigan", "blazer"],
    ["outfit color"] = ["outfit-blue", "outfit-green", "outfit-coral", "outfit-gold", "outfit-purple", "outfit-navy", "outfit-emerald", "outfit-lavender", "outfit-mint", "outfit-ocean", "outfit-rose", "outfit-slate"],
    ["accessory"] = ["none", "glasses", "headphones", "cat-ears", "cap", "crown", "star-glasses", "flower-crown", "propeller-cap", "gemstone-earrings", "headset-microphone", "fantasy-ears", "blue-ocarina"],
    ["fun accessory"] = ["none", "mustache", "whiskers", "rainbow", "clown-nose", "pirate", "monster-horns"],
    ["seasonal accessory"] = ["none", "witch", "pumpkin", "santa", "reindeer", "bat", "elf", "snow-monster"],
  };

  public IReadOnlyList<string> Options(string part, AvatarProfile profile)
  {
    if (!_values.TryGetValue(part, out var options))
    {
      throw new DomainRuleException(DomainError.InvalidValue, "Unknown avatar part.");
    }

    if (part == "age")
    {
      return profile == AvatarProfile.Child ? ["child"] : ["adult", "senior"];
    }

    if (part == "outfit")
    {
      return profile == AvatarProfile.Child
        ? ["superhero", "dinosaur", "monster", "vampire", "shark", "robot", "space", "fairy", "hoodie", "overalls", "explorer", "party", "sporty", "pajamas", "glitter-stage-jacket", "forest-adventurer"]
        : ["explorer", "party", "sporty", "shirt", "blouse", "cardigan", "blazer"];
    }

    if (part == "accessory")
    {
      return profile == AvatarProfile.Child
        ? _values[part]
        : ["none", "glasses", "headphones", "cat-ears", "cap", "crown", "star-glasses", "flower-crown", "propeller-cap"];
    }

    if (part == "hair")
    {
      return profile == AvatarProfile.Child
        ? _values[part]
        : ["short", "waves", "ponytail", "curls", "afro", "bob", "bun", "undercut"];
    }

    if (part == "hair color")
    {
      return profile == AvatarProfile.Child
        ? ["hair-black", "hair-brown", "hair-auburn", "hair-blonde", "hair-purple", "hair-blue"]
        : ["hair-black", "hair-brown", "hair-auburn", "hair-blonde", "hair-gray", "hair-silver", "hair-white"];
    }

    return options;
  }

  public void Validate(AvatarAppearance appearance, AvatarProfile profile)
  {
    ArgumentNullException.ThrowIfNull(appearance);
    ValidateValue("age", appearance.Age, profile);
    ValidateValue("skin tone", appearance.SkinTone, profile);
    ValidateValue("face shape", appearance.FaceShape, profile);
    ValidateValue("face", appearance.Face, profile);
    ValidateValue("hair", appearance.Hair, profile);
    ValidateValue("hair color", appearance.HairColor, profile);
    ValidateValue("outfit", appearance.Outfit, profile);
    ValidateValue("outfit color", appearance.OutfitColor, profile);
    ValidateValue("accessory", appearance.Accessory, profile);
    ValidateValue("fun accessory", appearance.FunAccessory, profile);
    ValidateValue("seasonal accessory", appearance.SeasonalAccessory, profile);
  }

  private void ValidateValue(string part, string value, AvatarProfile profile)
  {
    Rules.Require(Options(part, profile).Contains(value), DomainError.InvalidValue, $"Unknown avatar {part} value.");
  }
}