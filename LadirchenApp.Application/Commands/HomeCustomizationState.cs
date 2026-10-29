using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

using LadirchenApp.Domain.House.Shop;

namespace LadirchenApp.Application.Commands;

internal sealed class HomeCustomizationState
{
  private readonly Dictionary<string, HouseItem> _accessories;
  private readonly Dictionary<string, FurnitureSet> _furnitureSets;
  private readonly Dictionary<string, HouseTheme> _themes;
  private readonly Dictionary<string, RoomDesign> _roomDesigns;
  private readonly Dictionary<string, HouseLayoutEntity> _placements;
  private readonly Dictionary<string, HousePlacement> _initialPlacements;

  public HomeCustomizationState(
    JsonObject presentation,
    FamilyHouseShop shop,
    HouseSubscriptionTier subscriptionTier,
    Dictionary<string, HouseItem> accessories,
    Dictionary<string, FurnitureSet> furnitureSets,
    Dictionary<string, HouseTheme> themes,
    Dictionary<string, RoomDesign> roomDesigns,
    Dictionary<string, HouseLayoutEntity> placements,
    Dictionary<string, HousePlacement> initialPlacements
  )
  {
    Presentation = presentation ?? throw new ArgumentNullException(nameof(presentation));
    Shop = shop ?? throw new ArgumentNullException(nameof(shop));
    SubscriptionTier = subscriptionTier;
    _accessories = accessories ?? throw new ArgumentNullException(nameof(accessories));
    _furnitureSets = furnitureSets ?? throw new ArgumentNullException(nameof(furnitureSets));
    _themes = themes ?? throw new ArgumentNullException(nameof(themes));
    _roomDesigns = roomDesigns ?? throw new ArgumentNullException(nameof(roomDesigns));
    _placements = placements ?? throw new ArgumentNullException(nameof(placements));
    _initialPlacements = initialPlacements ?? throw new ArgumentNullException(nameof(initialPlacements));
  }

  public JsonObject Presentation { get; }
  public FamilyHouseShop Shop { get; }
  public HouseSubscriptionTier SubscriptionTier { get; }
  public IReadOnlyCollection<HouseItem> Accessories
  {
    get
    {
      return _accessories.Values;
    }
  }
  public IReadOnlyCollection<HouseTheme> Themes
  {
    get
    {
      return _themes.Values;
    }
  }
  public IReadOnlyCollection<FurnitureSet> FurnitureSets
  {
    get
    {
      return _furnitureSets.Values;
    }
  }
  public IReadOnlyCollection<RoomDesign> RoomDesigns
  {
    get
    {
      return _roomDesigns.Values;
    }
  }
  public IReadOnlyCollection<HouseLayoutEntity> Placements
  {
    get
    {
      return _placements.Values;
    }
  }

  public HouseItem Accessory(string id)
  {
    return _accessories.GetValueOrDefault(id)
      ?? throw new FamilyCommandException("The house accessory is unknown.");
  }

  public FurnitureSet FurnitureSet(string id)
  {
    return _furnitureSets.GetValueOrDefault(id)
      ?? throw new FamilyCommandException("The furniture set is unknown.");
  }

  public HouseTheme Theme(string id)
  {
    return _themes.GetValueOrDefault(id)
      ?? throw new FamilyCommandException("The house theme is unknown.");
  }

  public RoomDesign RoomDesign(string id)
  {
    return _roomDesigns.GetValueOrDefault(id)
      ?? throw new FamilyCommandException("The room design is unknown.");
  }

  public HouseTheme ThemeFor(RoomDesign design)
  {
    ArgumentNullException.ThrowIfNull(design);

    return Theme(design.ThemeId);
  }

  public HouseLayoutEntity Placement(string id)
  {
    return _placements.GetValueOrDefault(id)
      ?? throw new FamilyCommandException("The house placement is unknown.");
  }

  public HousePlacement InitialPlacement(string placementId)
  {
    return _initialPlacements.GetValueOrDefault(placementId)
      ?? throw new FamilyCommandException("The initial house placement is unknown.");
  }

  public HouseItem? FurnitureFor(HouseLayoutEntity entity)
  {
    ArgumentNullException.ThrowIfNull(entity);

    return entity.EntityType == "furniture"
      ? Accessory(entity.EntityId)
      : null;
  }

  public void UpdatePlacement(HouseLayoutEntity entity, HousePlacement placement)
  {
    ArgumentNullException.ThrowIfNull(entity);
    ArgumentNullException.ThrowIfNull(placement);
    _placements[entity.Id] = entity with { Placement = placement, Active = true };
  }

  public void UpdateFurniturePlacement(HouseItem item)
  {
    ArgumentNullException.ThrowIfNull(item);
    var placement = _placements.Values.SingleOrDefault(entity =>
      entity.EntityType == "furniture" && entity.EntityId == item.Id
    ) ?? throw new FamilyCommandException("The house accessory placement does not exist.");
    _placements[placement.Id] = placement with
    {
      Placement = item.Placement,
      Active = item.Owned && item.Equipped,
    };
  }
}
