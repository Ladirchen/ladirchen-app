using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.House.Shop;

public sealed partial class FamilyHouseShop
{
  private static readonly HashSet<string> InitiallyOwnedAccessoryIds =
  [
    "flower-boxes",
    "round-rug",
    "cozy-sofa",
    "kitchen-counter",
    "retro-fridge",
    "family-dining-table",
  ];
  private static readonly HashSet<string> InitiallyOwnedRoomDesignIds =
  [
    "garden-ladi-hills",
    "living-ladi-classic",
    "kitchen-ladi-classic",
  ];

  private readonly Dictionary<string, HouseItem> _stateAccessories = new(StringComparer.Ordinal);
  private readonly Dictionary<string, HouseTheme> _stateThemes = new(StringComparer.Ordinal);
  private readonly Dictionary<string, RoomDesign> _stateRoomDesigns = new(StringComparer.Ordinal);
  private readonly Dictionary<string, HouseLayoutEntity> _statePlacements = new(StringComparer.Ordinal);
  private readonly Dictionary<string, HousePlacement> _initialPlacements = new(StringComparer.Ordinal);

  public HouseSubscriptionTier SubscriptionTier { get; private set; } = HouseSubscriptionTier.Free;

  internal static FamilyHouseShop Reconstitute(
    FamilyAggregate family,
    int currentHouseLevel,
    FamilyHouseShopState state
  )
  {
    ArgumentNullException.ThrowIfNull(family);
    ArgumentNullException.ThrowIfNull(state);
    Rules.Require(
      family.Id == state.Id,
      DomainError.InvalidState,
      "The house customization belongs to another family."
    );
    var shop = new FamilyHouseShop(state.Id);
    shop.SetCurrentHouseLevel(currentHouseLevel);
    shop.ReconstituteState(state);
    shop.SetInitialPlacements(family);

    return shop;
  }

  public static FamilyHouseShop CreateInitial(
    FamilyAggregate family,
    int currentHouseLevel,
    HouseSubscriptionTier subscriptionTier
  )
  {
    ArgumentNullException.ThrowIfNull(family);
    Rules.Defined(subscriptionTier);
    var shop = new FamilyHouseShop(family.Id);
    shop.SetCurrentHouseLevel(currentHouseLevel);
    shop.ReconstituteState(CreateInitialState(family, subscriptionTier));
    shop.SetInitialPlacements(family);

    return shop;
  }

  internal FamilyHouseShopState CaptureState()
  {
    return new FamilyHouseShopState(
      Id,
      SelectedThemeId,
      SubscriptionTier,
      _stateAccessories.Values
        .OrderBy(item => item.Id, StringComparer.Ordinal)
        .Select(item => new HouseItemState(item.Id, item.Owned, item.Equipped)),
      _stateThemes.Values
        .OrderBy(item => item.Id, StringComparer.Ordinal)
        .Select(item => new HouseThemeState(item.Id, item.Owned)),
      _stateRoomDesigns.Values
        .OrderBy(item => item.Id, StringComparer.Ordinal)
        .Select(item => new RoomDesignState(item.Id, item.Owned, item.Selected)),
      _statePlacements.Values
        .OrderBy(item => item.Id, StringComparer.Ordinal)
        .Select(item => new HouseLayoutEntityState(
          item.Id,
          item.EntityType,
          item.EntityId,
          item.Placement
        ))
    );
  }

  public void SynchronizeFamilyEntities(FamilyAggregate family)
  {
    ArgumentNullException.ThrowIfNull(family);
    Rules.Require(
      family.Id == Id,
      DomainError.InvalidState,
      "The house customization belongs to another family."
    );
    var memberIds = family.Members.Select(item => item.Id.Value).ToHashSet(StringComparer.Ordinal);
    var petIds = family.Pets.Select(item => item.Id.Value).ToHashSet(StringComparer.Ordinal);
    var obsoletePlacementIds = _statePlacements.Values
      .Where(item =>
        item.EntityType == "member" && !memberIds.Contains(item.EntityId)
          || item.EntityType == "pet" && !petIds.Contains(item.EntityId)
      )
      .Select(item => item.Id)
      .ToArray();

    foreach (var placementId in obsoletePlacementIds)
    {
      _statePlacements.Remove(placementId);
    }

    SetInitialPlacements(family);
  }

  private void ReconstituteState(FamilyHouseShopState state)
  {
    SubscriptionTier = Rules.Defined(state.SubscriptionTier);
    var accessoryStates = IndexAccessories(state.Accessories);
    var themeStates = IndexThemes(state.Themes);
    var designStates = IndexRoomDesigns(state.RoomDesigns);
    var placementStates = IndexPlacements(state.Placements);
    RequireKnownCatalogEntries(accessoryStates, themeStates, designStates);
    ReconstituteAccessories(accessoryStates, placementStates);
    ReconstituteRoomDesigns(designStates);
    ReconstituteThemes(themeStates);
    ReconstitutePlacements(placementStates);
    var selectedTheme = _stateThemes.GetValueOrDefault(state.SelectedThemeId);
    Rules.Require(
      selectedTheme is not null && selectedTheme.Owned,
      DomainError.InvalidState,
      "The selected house theme must be owned."
    );
    SelectedThemeId = selectedTheme!.Id;
  }

  private void ReconstituteAccessories(
    IReadOnlyDictionary<string, HouseItemState> states,
    IReadOnlyDictionary<string, HouseLayoutEntityState> placements
  )
  {
    foreach (var catalogItem in HouseShopCatalog.Accessories)
    {
      var definition = catalogItem.Value;
      var state = states.GetValueOrDefault(catalogItem.Key)
        ?? new HouseItemState(catalogItem.Key, false, false);
      var placement = placements.Values.SingleOrDefault(item =>
        item.EntityType == "furniture" && item.EntityId == catalogItem.Key
      )?.Placement ?? definition.Initial;
      var item = new HouseItem(
        catalogItem.Key,
        definition.Price,
        Category(catalogItem.Key),
        definition.Movable ? HouseItemMobility.Movable : HouseItemMobility.Fixed,
        definition.IntendedArea,
        definition.Initial,
        definition.MinimumLevel,
        definition.AssignedArea
      );
      Rules.Require(
        !state.Equipped || state.Owned,
        DomainError.InvalidState,
        "Equipped furniture must be owned."
      );
      item.Owned = state.Owned;
      item.Equipped = state.Equipped;
      item.Placement = placement;
      _stateAccessories.Add(item.Id, item);
    }
  }

  private void ReconstituteRoomDesigns(IReadOnlyDictionary<string, RoomDesignState> states)
  {
    foreach (var catalogItem in HouseShopCatalog.RoomDesigns)
    {
      var definition = catalogItem.Value;
      var state = states.GetValueOrDefault(catalogItem.Key)
        ?? new RoomDesignState(catalogItem.Key, false, false);
      var design = new RoomDesign(
        catalogItem.Key,
        definition.ThemeId,
        definition.Area,
        definition.Price,
        definition.MinimumLevel
      );
      Rules.Require(
        !state.Selected || state.Owned,
        DomainError.InvalidState,
        "A selected room design must be owned."
      );
      design.Owned = state.Owned;
      design.Selected = state.Selected;
      _stateRoomDesigns.Add(design.Id, design);
    }

    Rules.Require(
      _stateRoomDesigns.Values
        .Where(item => item.Selected)
        .GroupBy(item => item.Room, StringComparer.Ordinal)
        .All(group => group.Count() == 1),
      DomainError.InvalidState,
      "A room can have only one selected design."
    );
  }

  private void ReconstituteThemes(IReadOnlyDictionary<string, HouseThemeState> states)
  {
    foreach (var catalogItem in HouseShopCatalog.Themes)
    {
      var state = states.GetValueOrDefault(catalogItem.Key)
        ?? new HouseThemeState(catalogItem.Key, false);
      var theme = new HouseTheme(
        catalogItem.Key,
        catalogItem.Value,
        _stateRoomDesigns.Values.Where(item => item.ThemeId == catalogItem.Key)
      );
      Rules.Require(
        state.Owned || theme.Designs.All(item => !item.Owned),
        DomainError.InvalidState,
        "Owned room designs require their house theme."
      );
      theme.Owned = state.Owned;
      _stateThemes.Add(theme.Id, theme);
    }
  }

  private void ReconstitutePlacements(
    IReadOnlyDictionary<string, HouseLayoutEntityState> states
  )
  {
    foreach (var state in states.Values)
    {
      var active = state.EntityType != "furniture";

      if (state.EntityType == "furniture")
      {
        var accessory = _stateAccessories.GetValueOrDefault(state.EntityId);
        Rules.Require(
          accessory is not null,
          DomainError.InvalidState,
          "A furniture placement must reference a known house accessory."
        );
        active = accessory!.Owned && accessory.Equipped;
      }

      _statePlacements.Add(
        state.Id,
        new HouseLayoutEntity(
          state.Id,
          state.EntityType,
          state.EntityId,
          state.Placement,
          active
        )
      );
    }

    foreach (var accessory in _stateAccessories.Values)
    {
      if (_statePlacements.Values.Any(item =>
        item.EntityType == "furniture" && item.EntityId == accessory.Id
      ))
      {
        continue;
      }

      var placementId = $"furniture-{accessory.Id}";
      Rules.Require(
        !_statePlacements.ContainsKey(placementId),
        DomainError.Duplicate,
        "The default furniture placement identifier is already used."
      );
      _statePlacements.Add(
        placementId,
        new HouseLayoutEntity(
          placementId,
          "furniture",
          accessory.Id,
          accessory.Placement,
          accessory.Owned && accessory.Equipped
        )
      );
    }
  }

  private static Dictionary<string, HouseItemState> IndexAccessories(
    IEnumerable<HouseItemState> states
  )
  {
    var result = new Dictionary<string, HouseItemState>(StringComparer.Ordinal);

    foreach (var state in states)
    {
      ArgumentNullException.ThrowIfNull(state);
      Rules.Require(
        result.TryAdd(state.Id, state),
        DomainError.Duplicate,
        "The house accessory state is duplicated."
      );
    }

    return result;
  }

  private static Dictionary<string, HouseThemeState> IndexThemes(
    IEnumerable<HouseThemeState> states
  )
  {
    var result = new Dictionary<string, HouseThemeState>(StringComparer.Ordinal);

    foreach (var state in states)
    {
      ArgumentNullException.ThrowIfNull(state);
      Rules.Require(
        result.TryAdd(state.Id, state),
        DomainError.Duplicate,
        "The house theme state is duplicated."
      );
    }

    return result;
  }

  private static Dictionary<string, RoomDesignState> IndexRoomDesigns(
    IEnumerable<RoomDesignState> states
  )
  {
    var result = new Dictionary<string, RoomDesignState>(StringComparer.Ordinal);

    foreach (var state in states)
    {
      ArgumentNullException.ThrowIfNull(state);
      Rules.Require(
        result.TryAdd(state.Id, state),
        DomainError.Duplicate,
        "The room-design state is duplicated."
      );
    }

    return result;
  }

  private static Dictionary<string, HouseLayoutEntityState> IndexPlacements(
    IEnumerable<HouseLayoutEntityState> states
  )
  {
    var result = new Dictionary<string, HouseLayoutEntityState>(StringComparer.Ordinal);
    var entityKeys = new HashSet<(string EntityType, string EntityId)>();

    foreach (var state in states)
    {
      ArgumentNullException.ThrowIfNull(state);
      Rules.Require(
        result.TryAdd(state.Id, state),
        DomainError.Duplicate,
        "The house-placement state is duplicated."
      );
      Rules.Require(
        entityKeys.Add((state.EntityType, state.EntityId)),
        DomainError.Duplicate,
        "A house entity can have only one placement."
      );
    }

    return result;
  }

  private static void RequireKnownCatalogEntries(
    IReadOnlyDictionary<string, HouseItemState> accessories,
    IReadOnlyDictionary<string, HouseThemeState> themes,
    IReadOnlyDictionary<string, RoomDesignState> roomDesigns
  )
  {
    Rules.Require(
      accessories.Keys.All(HouseShopCatalog.Accessories.ContainsKey),
      DomainError.NotFound,
      "The stored house accessory is unknown."
    );
    Rules.Require(
      themes.Keys.All(HouseShopCatalog.Themes.ContainsKey),
      DomainError.NotFound,
      "The stored house theme is unknown."
    );
    Rules.Require(
      roomDesigns.Keys.All(HouseShopCatalog.RoomDesigns.ContainsKey),
      DomainError.NotFound,
      "The stored room design is unknown."
    );
  }

  private static HouseItemCategory Category(string id)
  {
    return HouseShopCatalog.CatalogAreaForAccessory(id) switch
    {
      "outside" => HouseItemCategory.Garden,
      "special" => HouseItemCategory.Special,
      _ => HouseItemCategory.Furniture,
    };
  }

  private static FamilyHouseShopState CreateInitialState(
    FamilyAggregate family,
    HouseSubscriptionTier subscriptionTier
  )
  {
    var placementPolicy = new HouseInitialPlacementPolicy();
    var placements = HouseShopCatalog.Accessories.Select(item =>
      new HouseLayoutEntityState(
        $"furniture-{item.Key}",
        "furniture",
        item.Key,
        item.Value.Initial
      )
    ).ToList();
    placements.Add(
      new HouseLayoutEntityState(
        "family-ladi",
        "ladi",
        "family-ladi",
        placementPolicy.FamilyLadi()
      )
    );

    for (var index = 0; index < family.Members.Count; index++)
    {
      var member = family.Members[index];
      placements.Add(
        new HouseLayoutEntityState(
          $"member-{member.Id.Value}",
          "member",
          member.Id.Value,
          placementPolicy.FamilyMember(index, member.Role)
        )
      );
    }

    for (var index = 0; index < family.Pets.Count; index++)
    {
      var pet = family.Pets[index];
      placements.Add(
        new HouseLayoutEntityState(
          $"pet-{pet.Id.Value}",
          "pet",
          pet.Id.Value,
          placementPolicy.FamilyPet(index)
        )
      );
    }

    return new FamilyHouseShopState(
      family.Id,
      "sunny-dollhouse",
      subscriptionTier,
      HouseShopCatalog.Accessories.Keys.Select(id =>
        new HouseItemState(
          id,
          InitiallyOwnedAccessoryIds.Contains(id),
          InitiallyOwnedAccessoryIds.Contains(id)
        )
      ),
      HouseShopCatalog.Themes.Keys.Select(id =>
        new HouseThemeState(id, id == "sunny-dollhouse")
      ),
      HouseShopCatalog.RoomDesigns.Keys.Select(id =>
        new RoomDesignState(
          id,
          InitiallyOwnedRoomDesignIds.Contains(id),
          InitiallyOwnedRoomDesignIds.Contains(id)
        )
      ),
      placements
    );
  }

  private void SetInitialPlacements(FamilyAggregate family)
  {
    _initialPlacements.Clear();
    var policy = new HouseInitialPlacementPolicy();

    foreach (var accessory in _stateAccessories.Values)
    {
      SetInitialPlacement("furniture", accessory.Id, accessory.InitialPlacement);
    }

    SetInitialPlacement("ladi", "family-ladi", policy.FamilyLadi());

    for (var index = 0; index < family.Members.Count; index++)
    {
      var member = family.Members[index];
      SetInitialPlacement(
        "member",
        member.Id.Value,
        policy.FamilyMember(index, member.Role)
      );
    }

    for (var index = 0; index < family.Pets.Count; index++)
    {
      var pet = family.Pets[index];
      SetInitialPlacement("pet", pet.Id.Value, policy.FamilyPet(index));
    }
  }

  private void SetInitialPlacement(
    string entityType,
    string entityId,
    HousePlacement placement
  )
  {
    var entity = _statePlacements.Values.SingleOrDefault(item =>
      item.EntityType == entityType && item.EntityId == entityId
    );

    if (entity is null)
    {
      var id = entityType switch
      {
        "ladi" => "family-ladi",
        _ => $"{entityType}-{entityId}",
      };
      Rules.Require(
        !_statePlacements.ContainsKey(id),
        DomainError.Duplicate,
        "The initial house placement identifier is already used."
      );
      entity = new HouseLayoutEntity(id, entityType, entityId, placement, true);
      _statePlacements.Add(id, entity);
    }

    if (entity is not null)
    {
      _initialPlacements[entity.Id] = placement;
    }
  }
}
