using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House.Shop;

public sealed partial class FamilyHouseShop
{
  public void Restore(
    int houseLevel,
    string selectedThemeId,
    IEnumerable<HouseTheme> themes
  )
  {
    ArgumentNullException.ThrowIfNull(themes);
    Restore(houseLevel);
    var selectedTheme = themes.SingleOrDefault(item => item.Id == selectedThemeId);
    Rules.Require(
      selectedTheme is not null && selectedTheme.Owned,
      DomainError.InvalidState,
      "The selected house theme must be owned."
    );
    SelectedThemeId = selectedThemeId;
  }
}