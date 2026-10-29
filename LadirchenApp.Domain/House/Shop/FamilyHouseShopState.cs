using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House.Shop;

internal sealed record FamilyHouseShopState
{
  public FamilyHouseShopState(
    FamilyId id,
    string selectedThemeId,
    HouseSubscriptionTier subscriptionTier,
    IEnumerable<HouseItemState> accessories,
    IEnumerable<HouseThemeState> themes,
    IEnumerable<RoomDesignState> roomDesigns,
    IEnumerable<HouseLayoutEntityState> placements
  )
  {
    ArgumentNullException.ThrowIfNull(id);
    ArgumentNullException.ThrowIfNull(accessories);
    ArgumentNullException.ThrowIfNull(themes);
    ArgumentNullException.ThrowIfNull(roomDesigns);
    ArgumentNullException.ThrowIfNull(placements);
    Id = id;
    SelectedThemeId = Rules.Text(selectedThemeId);
    SubscriptionTier = Rules.Defined(subscriptionTier);
    Accessories = accessories.ToArray();
    Themes = themes.ToArray();
    RoomDesigns = roomDesigns.ToArray();
    Placements = placements.ToArray();
  }

  public FamilyId Id { get; }
  public string SelectedThemeId { get; }
  public HouseSubscriptionTier SubscriptionTier { get; }
  public IReadOnlyList<HouseItemState> Accessories { get; }
  public IReadOnlyList<HouseThemeState> Themes { get; }
  public IReadOnlyList<RoomDesignState> RoomDesigns { get; }
  public IReadOnlyList<HouseLayoutEntityState> Placements { get; }
}
