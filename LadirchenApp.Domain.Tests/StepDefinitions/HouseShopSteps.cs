using System;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.House.Shop;

using Reqnroll;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

[Binding]
public sealed class HouseShopSteps
{
  private readonly FamilyHouseShop _shop = new();
  private readonly HouseShopPolicy _policy = new();
  private HouseItem _item = null!;
  private FurnitureSet _set = null!;
  private HouseTheme _theme = null!;
  private RoomDesign _design = null!;
  private decimal _initialBalance;
  private bool _actorIsChild = true;
  private bool _result;
  private string _requestedArea = string.Empty;
  private string _actualArea = string.Empty;
  private DateOnly? _newStart;
  private DateOnly? _newEnd;
  private DateTime _localDateTime;
  private bool _newBadge;
  private string _catalogArea = string.Empty;

  [Given("a child can afford an unowned house accessory")]
  public void AffordableAccessory()
  {
    CreateItem();
    _shop.Restore(100, 4);
    _initialBalance = _shop.Balance;
  }

  [When("the child purchases the accessory")]
  public void PurchaseAccessory()
  {
    _result = _shop.PurchaseAccessory(_item, true);
  }

  [Then("its price is deducted from the child's wallet")]
  public async Task ItemPriceDeducted()
  {
    await Assert.That(_shop.Balance).IsEqualTo(_initialBalance - _item.Price);
  }

  [Then("the accessory is owned")]
  public async Task AccessoryOwned()
  {
    await Assert.That(_item.Owned).IsTrue();
  }

  [Then("the accessory is stored unequipped")]
  public async Task AccessoryUnequipped()
  {
    await Assert.That(_item.Equipped).IsFalse();
  }

  [Given("a house accessory is {string}")]
  public void AccessoryState(string state)
  {
    AffordableAccessory();

    if (state == "owned")
    {
      _shop.PurchaseAccessory(_item, true);
      _shop.Restore(100, 4);
    }

    _initialBalance = _shop.Balance;
  }

  [Given("the viewer is {string}")]
  public void Viewer(string viewer)
  {
    _actorIsChild = viewer == "child";
  }

  [Given("the available balance is {string}")]
  public void AvailableBalance(string state)
  {
    _shop.Restore(state == "sufficient" ? 100 : 0, _shop.HouseLevel);
    _initialBalance = _shop.Balance;
  }

  [When("the viewer attempts to purchase the accessory")]
  public void ViewerPurchasesAccessory()
  {
    _result = _shop.PurchaseAccessory(_item, _actorIsChild);
  }

  [Then("ownership and balance remain unchanged")]
  public async Task OwnershipAndBalanceUnchanged()
  {
    await Assert.That(_result).IsFalse();
    await Assert.That(_shop.Balance).IsEqualTo(_initialBalance);
  }

  [Given("a child can afford an unowned furniture set")]
  public void AffordableSet()
  {
    CreateSet();
    _shop.Restore(100, 4);
    _initialBalance = _shop.Balance;
  }

  [Given("the house meets the set's minimum level")]
  public void SetLevelMet()
  {
    _shop.Restore(_shop.Balance, _set.MinimumLevel);
  }

  [When("the child purchases the furniture set")]
  public void PurchaseSet()
  {
    _result = _shop.PurchaseSet(_set, true);
  }

  [Then("the set price is deducted from the child's wallet")]
  public async Task SetPriceDeducted()
  {
    await Assert.That(_shop.Balance).IsEqualTo(_initialBalance - _set.Price);
  }

  [Then("every accessory in the set is owned")]
  public async Task SetItemsOwned()
  {
    await Assert.That(_set.Items.All(item => item.Owned)).IsTrue();
  }

  [Then("every accessory in the set is stored unequipped")]
  public async Task SetItemsUnequipped()
  {
    await Assert.That(_set.Items.All(item => !item.Equipped)).IsTrue();
  }

  [Given("a furniture set is {string}")]
  public void SetOwnership(string ownership)
  {
    AffordableSet();

    if (ownership == "owned")
    {
      _shop.PurchaseSet(_set, true);
      _shop.Restore(100, 4);
    }

    _initialBalance = _shop.Balance;
  }

  [Given("its minimum house level is {string}")]
  public void SetLevelState(string state)
  {
    _shop.Restore(_shop.Balance, state == "met" ? _set.MinimumLevel : _set.MinimumLevel - 1);
  }

  [Given("the child's available balance is {string}")]
  public void ChildBalanceState(string state)
  {
    _shop.Restore(state == "sufficient" ? 100 : 0, _shop.HouseLevel);
    _initialBalance = _shop.Balance;
  }

  [When("the child attempts to purchase the furniture set")]
  public void AttemptSetPurchase()
  {
    _result = _shop.PurchaseSet(_set, true);
  }

  [Then("the purchase is {word}")]
  public async Task PurchaseResult(string result)
  {
    await Assert.That(_result).IsEqualTo(result == "accepted");
  }

  [Given("a child can afford an unowned house theme")]
  public void AffordableTheme()
  {
    CreateTheme();
    _shop.Restore(100, 4);
    _initialBalance = _shop.Balance;
  }

  [When("the child purchases the theme")]
  public void PurchaseTheme()
  {
    _result = _shop.PurchaseTheme(_theme, true);
  }

  [Then("the theme price is deducted from the child's wallet")]
  public async Task ThemePriceDeducted()
  {
    await Assert.That(_shop.Balance).IsEqualTo(_initialBalance - _theme.Price);
  }

  [Then("the theme is owned and selected")]
  public async Task ThemeOwnedAndSelected()
  {
    await Assert.That(_theme.Owned).IsTrue();
    await Assert.That(_shop.SelectedThemeId).IsEqualTo(_theme.Id);
  }

  [Then("the theme's room designs are owned and selected for their rooms")]
  [Then("every room design belonging to the theme is owned")]
  public async Task ThemeDesignsOwned()
  {
    await Assert.That(_theme.Designs.All(item => item.Owned && item.Selected)).IsTrue();
  }

  [Given("the family does not own a house theme")]
  public void UnownedTheme()
  {
    CreateTheme();
  }

  [When("a family member attempts to select it")]
  public void SelectUnownedTheme()
  {
    _result = _shop.SelectTheme(_theme);
  }

  [Then("the selected house theme remains unchanged")]
  public async Task ThemeSelectionUnchanged()
  {
    await Assert.That(_result).IsFalse();
    await Assert.That(_shop.SelectedThemeId).IsEqualTo("default");
  }

  [Given("the family owns a house theme")]
  public void OwnedTheme()
  {
    AffordableTheme();
    _shop.PurchaseTheme(_theme, true);
  }

  [When("a family member selects the theme")]
  public void SelectTheme()
  {
    _result = _shop.SelectTheme(_theme);
  }

  [Then("the house theme is selected")]
  public async Task ThemeSelected()
  {
    await Assert.That(_shop.SelectedThemeId).IsEqualTo(_theme.Id);
  }

  [Then("each of those designs is selected for its room")]
  public async Task DesignsSelected()
  {
    await Assert.That(_theme.Designs.All(item => item.Selected)).IsTrue();
  }

  [Given("a child can afford an unowned room design")]
  public void AffordableDesign()
  {
    CreateTheme();
    _design = _theme.Designs[0];
    _shop.Restore(100, 4);
    _initialBalance = _shop.Balance;
  }

  [Given("the family owns the design's house theme")]
  public void OwnDesignTheme()
  {
    _shop.PurchaseTheme(_theme, true);
    _shop.Restore(100, 4);
    _design = new RoomDesign("extra-design", _theme.Id, "kitchen", 20, 2);
    _initialBalance = _shop.Balance;
  }

  [Given("the house meets the design's minimum level")]
  public void DesignLevelMet()
  {
    _shop.Restore(_shop.Balance, _design.MinimumLevel);
  }

  [When("the child purchases the room design")]
  public void PurchaseDesign()
  {
    _result = _shop.PurchaseDesign(_design, _theme, true);
  }

  [Then("the design price is deducted from the child's wallet")]
  public async Task DesignPriceDeducted()
  {
    await Assert.That(_shop.Balance).IsEqualTo(_initialBalance - _design.Price);
  }

  [Then("the design is owned and selected for its room")]
  public async Task DesignOwnedSelected()
  {
    await Assert.That(_design.Owned && _design.Selected).IsTrue();
  }

  [Then("its house theme becomes selected")]
  public async Task DesignThemeSelected()
  {
    await Assert.That(_shop.SelectedThemeId).IsEqualTo(_theme.Id);
  }

  [Given("the room design purchase requirement {string} is not met")]
  public void MissingDesignRequirement(string requirement)
  {
    AffordableDesign();

    if (requirement != "owned house theme")
    {
      _shop.PurchaseTheme(_theme, true);
      _shop.Restore(requirement == "available balance" ? 0 : 100, requirement == "minimum house level" ? 0 : 4);
      _design = new RoomDesign("extra-design", _theme.Id, "kitchen", 20, 2);
    }

    _initialBalance = _shop.Balance;
  }

  [When("the child attempts to purchase the room design")]
  public void AttemptDesignPurchase()
  {
    _result = _shop.PurchaseDesign(_design, _theme, true);
  }

  [Then("ownership, selection and balance remain unchanged")]
  public async Task DesignUnchanged()
  {
    await Assert.That(_result).IsFalse();
    await Assert.That(_design.Owned).IsFalse();
    await Assert.That(_shop.Balance).IsEqualTo(_initialBalance);
  }

  [Given("a room design is not owned or requires a higher house level")]
  public void IneligibleDesign()
  {
    _design = new RoomDesign("design", "theme", "living room", 20, 4);
    _shop.Restore(100, 2);
  }

  [When("a family member attempts to select the room design")]
  public void SelectDesign()
  {
    _result = _shop.SelectDesign(_design);
  }

  [Then("the selected room design remains unchanged")]
  public async Task DesignSelectionUnchanged()
  {
    await Assert.That(_result).IsFalse();
    await Assert.That(_design.Selected).IsFalse();
  }

  [Given("the family owns a movable house entity")]
  [Given("an owned movable house item can be arranged in an unlocked area")]
  public void OwnedMovableItem()
  {
    AffordableAccessory();
    _shop.PurchaseAccessory(_item, true);
    _shop.UnlockArea("living room");
  }

  [Given("its target area is unlocked")]
  public void TargetAreaUnlocked()
  {
    _shop.UnlockArea("living room");
  }

  [When("a family member moves the entity into that area")]
  public void MoveEntity()
  {
    _result = _shop.Move(_item, "living room", 60, 60);
  }

  [Then("the house layout contains the new placement")]
  public async Task NewPlacementStored()
  {
    await Assert.That(_result).IsTrue();
    await Assert.That(_item.Placement).IsEqualTo(new HousePlacement("living room", 60, 60, 1));
  }

  [Given("an owned furniture item has fixed mobility")]
  public void OwnedFixedItem()
  {
    _item = NewItem(HouseItemMobility.Fixed);
    _shop.Restore(100, 4);
    _shop.PurchaseAccessory(_item, true);
    _shop.UnlockArea("living room");
  }

  [When("a family member attempts to move, store or place it")]
  public void ManipulateFixedItem()
  {
    _result = _shop.Move(_item, "living room", 60, 60)
      || _shop.Store(_item)
      || _shop.Place(_item, "living room");
  }

  [Then("the furniture placement remains unchanged")]
  public async Task FixedPlacementUnchanged()
  {
    await Assert.That(_result).IsFalse();
    await Assert.That(_item.Placement).IsEqualTo(_item.InitialPlacement);
  }

  [Given("an owned movable furniture item is equipped")]
  public void EquippedMovableItem()
  {
    OwnedMovableItem();
    _shop.Move(_item, "living room", 50, 50);
  }

  [When("a family member stores it")]
  public void StoreItem()
  {
    _result = _shop.Store(_item);
  }

  [Then("the furniture item is unequipped")]
  public async Task ItemUnequipped()
  {
    await Assert.That(_item.Equipped).IsFalse();
  }

  [Given("an owned movable furniture item is stored")]
  public void StoredMovableItem()
  {
    OwnedMovableItem();
  }

  [When("a family member places it in an unlocked compatible area")]
  public void PlaceItem()
  {
    _result = _shop.Place(_item, "living room");
  }

  [Then("the item is equipped in that area")]
  public async Task ItemEquipped()
  {
    await Assert.That(_item.Equipped).IsTrue();
    await Assert.That(_item.Placement.Area).IsEqualTo("living room");
  }

  [Then("its placement uses the area's default position")]
  public async Task DefaultPosition()
  {
    await Assert.That(_item.Placement.X).IsEqualTo(_item.InitialPlacement.X);
    await Assert.That(_item.Placement.Y).IsEqualTo(_item.InitialPlacement.Y);
  }

  [Given("an owned stored furniture item is intended for {string} use")]
  public void ItemIntendedUse(string placement)
  {
    _item = new HouseItem("item", 10, HouseItemCategory.Furniture, HouseItemMobility.Movable, placement, new("living room", 50, 50, 1));
    _shop.Restore(100, 4);
    _shop.PurchaseAccessory(_item, true);
    _shop.UnlockArea("living room");
    _shop.UnlockArea("garden");
  }

  [When("a family member requests placement in {string}")]
  public void RequestPlacement(string area)
  {
    _requestedArea = area;
    _actualArea = _policy.CompatibleArea(_item.IntendedArea, area);
  }

  [Then("the furniture is placed in {string}")]
  public async Task FurniturePlacedIn(string area)
  {
    await Assert.That(_actualArea).IsEqualTo(area);
  }

  [Given("a family member has moved a movable house entity")]
  public void MovedEntity()
  {
    OwnedMovableItem();
    _shop.Move(_item, "living room", 70, 70);
  }

  [When("a family member resets the entity's placement")]
  public void ResetPlacement()
  {
    _result = _shop.Reset(_item);
  }

  [Then("its initial area, position and scale are restored")]
  public async Task InitialPlacementRestored()
  {
    await Assert.That(_item.Placement).IsEqualTo(_item.InitialPlacement);
  }

  [Given("the family owns every accessory belonging to a furniture set")]
  public void OwnEverySetItem()
  {
    AffordableSet();
    _shop.PurchaseSet(_set, true);
  }

  [When("ownership of the furniture set is determined")]
  public void DetermineSetOwnership()
  {
    _result = _set.Owned;
  }

  [Then("the furniture set is owned")]
  public async Task FurnitureSetOwned()
  {
    await Assert.That(_result).IsTrue();
  }

  [Given("a house accessory belongs to the {string} category")]
  public void ItemCategory(string category)
  {
    var value = Enum.Parse<HouseItemCategory>(category, true);
    _item = new HouseItem("item", 10, value, HouseItemMobility.Movable, "inside", new("living room", 50, 50, 1));
  }

  [When("a family member views the house shop")]
  public void ViewHouseShop()
  {
    _catalogArea = _policy.CatalogArea(_item.Category);
  }

  [Then("the house item is shown in the {string} catalog area")]
  public async Task CatalogArea(string area)
  {
    await Assert.That(_catalogArea).IsEqualTo(area);
  }

  [When("a family member moves it to x {float} and y {float}")]
  public void MoveTo(decimal x, decimal y)
  {
    _shop.Move(_item, "living room", x, y);
  }

  [Then("its stored x coordinate is {float}")]
  public async Task StoredX(decimal x)
  {
    await Assert.That(_item.Placement.X).IsEqualTo(x);
  }

  [Then("its stored y coordinate is {float}")]
  public async Task StoredY(decimal y)
  {
    await Assert.That(_item.Placement.Y).IsEqualTo(y);
  }

  [Given("a house item has a New-badge schedule from {string} through {string}")]
  public void NewBadgeSchedule(string start, string end)
  {
    _newStart = DateOnly.Parse(start, System.Globalization.CultureInfo.InvariantCulture);
    _newEnd = DateOnly.Parse(end, System.Globalization.CultureInfo.InvariantCulture);
  }

  [Given("a house item has no New-badge schedule")]
  public void NoNewBadgeSchedule()
  {
    _newStart = null;
    _newEnd = null;
    _localDateTime = DateTime.Now;
  }

  [Given("the family local date and time is {string}")]
  public void FamilyLocalDateTime(string value)
  {
    _localDateTime = DateTime.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
  }

  [When("the house catalog determines the item's New badge")]
  public void DetermineNewBadge()
  {
    _newBadge = _policy.IsNew(_newStart, _newEnd, _localDateTime);
  }

  [Then("the New badge is {word}")]
  public async Task NewBadge(string visibility)
  {
    await Assert.That(_newBadge).IsEqualTo(visibility == "visible");
  }

  private void CreateItem()
  {
    _item = NewItem(HouseItemMobility.Movable);
  }

  private static HouseItem NewItem(HouseItemMobility mobility)
  {
    return new HouseItem("item", 20, HouseItemCategory.Furniture, mobility, "inside", new HousePlacement("living room", 40, 40, 1));
  }

  private void CreateSet()
  {
    _set = new FurnitureSet("set", 50, 2, [NewItem(HouseItemMobility.Movable), NewItem(HouseItemMobility.Movable)]);
  }

  private void CreateTheme()
  {
    _design = new RoomDesign("design", "theme", "living room", 20, 2);
    _theme = new HouseTheme("theme", 40, [_design]);
  }
}
