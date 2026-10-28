using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Avatar;

public sealed class AvatarCatalog
{
  private static readonly IReadOnlyDictionary<string, string[]> Values = new Dictionary<string, string[]>
  {
    ["age"] = ["child", "adult", "senior"],
    ["skin tone"] = ["light", "medium", "dark"],
    ["face shape"] = ["round", "oval"],
    ["face"] = ["smile", "happy"],
    ["hair"] = ["short", "long", "curly"],
    ["hair color"] = ["brown", "black", "blonde"],
    ["outfit"] = ["play", "casual", "formal"],
    ["outfit color"] = ["blue", "green", "red"],
    ["accessory"] = ["none", "glasses"],
    ["fun accessory"] = ["none", "party-hat"],
    ["seasonal accessory"] = ["none", "winter-hat"],
  };

  public IReadOnlyList<string> Options(string part, AvatarProfile profile)
  {
    if (!Values.TryGetValue(part, out var options))
    {
      throw new DomainRuleException(DomainError.InvalidValue, "Unknown avatar part.");
    }

    if (part == "age")
    {
      return profile == AvatarProfile.Child ? ["child"] : ["adult", "senior"];
    }

    if (part == "outfit")
    {
      return profile == AvatarProfile.Child ? ["play"] : ["casual", "formal"];
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
